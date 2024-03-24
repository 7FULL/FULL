using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PhotonView))]
public class BuildingSystem : SerializedMonoBehaviour
{
    public static BuildingSystem Instance;
    
    private Grid grid;
    
    public Grid Grid => grid;
    
    private BuildingType currentBuilding;
    
    [SerializeField]
    [InspectorName("Place distance")]
    private float placeDistance = 3;

    [SerializeField]
    private List<Cell> cells = new List<Cell>();
    
    [SerializeField]
    private Dictionary<BuildingType, GameObject> buildingPrefabs;
    
    [SerializeField]
    private Dictionary<BuildingType, GameObject> buildingPreviewPrefabs;
    
    [ReadOnly]
    private bool canBuild = false;
    
    public bool CanBuild => canBuild;
    
    private Vector3Int lastCell;
    
    private Dictionary<string, EditBuilding> _editBuildings = new Dictionary<string, EditBuilding>();
    
    private bool _hashClicked = false;
    private bool _lastHashClicked = false;

    private PhotonView pv;
    
    public PhotonView Pv => pv;
    
    private EditBuilding lastBuilding;
    
    private void Awake()
    {
        Instance = this;
        
        pv = GetComponent<PhotonView>();
    }
    
    public void CantBuild()
    {
        canBuild = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        grid = GetComponentInChildren<Grid>();
    }

    // Update is called once per frame
    void Update()
    {
        //If we press somekey that is not numeric or tab or escape or space or awsd or left shift click or right click or mouse wheel
        if (Input.anyKeyDown)
        {
            if (
                !Input.GetKeyDown(KeyCode.Tab)
                && !Input.GetKeyDown(KeyCode.Escape)
                && !Input.GetKeyDown(KeyCode.Space)
                && !Input.GetKeyDown(KeyCode.W)
                && !Input.GetKeyDown(KeyCode.A)
                && !Input.GetKeyDown(KeyCode.S)
                && !Input.GetKeyDown(KeyCode.D)
                && !Input.GetKeyDown(KeyCode.LeftShift)
                && !Input.GetMouseButtonDown(2)
                && !Input.GetMouseButtonDown(0))
            {

                canBuild = true;

                //If we press the first lateral key of the mouse we select the first building ( Stairs )
                if (Input.GetKeyDown(KeyCode.Mouse4))
                {
                    currentBuilding = BuildingType.STAIRS;
                }

                //If we press the second lateral key of the mouse we select the second building ( Wall )
                if (Input.GetKeyDown(KeyCode.Mouse3))
                {
                    currentBuilding = BuildingType.WALL;
                }

                //If we press the F key we select the third building ( Floor )
                if (Input.GetKeyDown(KeyCode.F))
                {
                    currentBuilding = BuildingType.FLOOR;
                }

                //If we press the left alt key we select the fourth building ( Roof )
                if (Input.GetKeyDown(KeyCode.LeftAlt))
                {
                    currentBuilding = BuildingType.ROOF;
                }
            }
        }

        if (canBuild)
        {
            //If we press the left mouse button we want to place the current building
            if (Input.GetMouseButton(0))
            {
                PlaceBuilding();
            }
            
            //We show the preview of the building
            ShowPreview();
        }
        else
        {
            HidePreview();
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            _hashClicked = !_hashClicked;
        }
        
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;

            Transform transform1 = GameManager.Instance.Player.CharacterCamera.transform;
            Ray ray = new Ray(transform1.position, transform1.forward);

            if (Physics.Raycast(ray, out hit, 5f) && _lastHashClicked != _hashClicked)
            {
                if (hit.collider.CompareTag("EditBuildingCell"))
                {
                    EditBuildingCells cell = hit.collider.GetComponent<EditBuildingCells>();

                    if (cell != null)
                    {
                        cell.Edit();
                    }
                }
            }
            
            _lastHashClicked = _hashClicked;
        }
        
        //If we press E we want to edit the building
        if (Input.GetKeyDown(KeyCode.E))
        {
            canBuild = false;

            if (lastBuilding != null)
            {
                lastBuilding.Edit();
                
                lastBuilding = null;
            }
            
            //We send a raycast and compare the tag of the object we hit
            RaycastHit hit;
            
            Transform transform1 = GameManager.Instance.Player.CharacterCamera.transform;
            
            Ray ray = new Ray(transform1.position, transform1.forward);
            
            if (Physics.Raycast(ray, out hit, 5f))
            {
                bool aux = false;
                
                if (hit.collider.CompareTag("Building") || hit.collider.CompareTag("EditBuildingCell"))
                {
                    //We get the building component
                    EditBuilding building = hit.collider.GetComponent<EditBuilding>();
                    
                    if (building != null)
                    {
                        //We edit the building
                        lastBuilding = building;
                        
                        building.Edit();
                    }
                    else
                    {
                        //We get the building component
                        EditBuilding building1 = hit.collider.GetComponentInParent<EditBuilding>();
                    
                        if (building1 != null)
                        {
                            lastBuilding = building1;
                            
                            //We edit the building
                            building1.Edit();
                        }
                    }
                }
            }
            else
            {
                if (lastBuilding != null)
                {
                    lastBuilding.Edit();
                    
                    lastBuilding = null;
                }
            }
        }
    }
    
    private void PlaceBuilding()
    {
        Transform cameraTransform = GameManager.Instance.Player.CharacterCamera.Camera.transform;
        
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;
        
        Vector3 worldPoint = Vector3.zero;

        if (lastBuilding != null)
        {
            lastBuilding.CloseAllCells();
            
            lastBuilding = null;
        }
        
        //If we hit something
        if (Physics.Raycast(ray, out hit, placeDistance))
        {
            //We want to get the point of the hit and convert it to grid coordinates
            Vector3 point = hit.point;
            point = grid.WorldToCell(point);
            
            //We want to get the world position of the grid coordinates
            worldPoint = grid.GetCellCenterWorld(Vector3Int.FloorToInt(point));
        }
        else
        {
            //We get the cell we are looking at
            Vector3Int cell = grid.WorldToCell(cameraTransform.position + cameraTransform.forward * placeDistance);
            
            //We get the world position of the cell
            worldPoint = grid.GetCellCenterWorld(cell);
        }
        
        //We want to place the building at the world point
        bool farCell = PlaceBuilding(worldPoint);
        
        //If we are too far from the cell we want to place the building at the camera position
        if (!farCell)
        {
            Vector3Int cell = grid.WorldToCell(cameraTransform.position + cameraTransform.forward * (placeDistance/4f));
            
            //We get the world position of the cell
            worldPoint = grid.GetCellCenterWorld(cell);
            
            PlaceBuilding(worldPoint);
        }
    }
    
    private bool PlaceBuilding(Vector3 point, bool preview = false)
    {
        //If we have a current building
        if (buildingPrefabs[currentBuilding] != null)
        {
            Cell cell = null;
            
            //We check if the cell isnt occupied
            if (!IsCellOccupied(point))
            {
                cell = new Cell(point);
            }
            else
            {
                //We get the cell that is occupied
                foreach (Cell aux in cells)
                {
                    if (aux.Position == point)
                    {
                        cell = aux;
                        break;
                    }
                }
                
                //We check if the cell is available
            }

            if (cell != null)
            {
                GameObject building = null;
                
                if (!preview)
                {
                    //We generate an id to later add to the dictionary
                    string id = Random.Range(-1000000000, 1000000000).ToString();

                    while (_editBuildings.ContainsKey(id))
                    {
                        id = Random.Range(-1000000000, 1000000000).ToString();
                    }
                    
                    pv.RPC("PlaceBuildingRPC", RpcTarget.OthersBuffered, point, currentBuilding, 
                        GameManager.Instance.Player.CharacterCamera.transform.eulerAngles, id);
                    
                    building = Instantiate(buildingPrefabs[currentBuilding], point, buildingPrefabs[currentBuilding].transform.rotation);
                
                    building.transform.SetParent(grid.transform);
                    
                    EditBuilding editBuilding = building.GetComponent<EditBuilding>();
                    
                    _editBuildings.Add(id, editBuilding);
                    
                    editBuilding.SetId(id);
                }
                else
                {
                    building = buildingPreviewPrefabs[currentBuilding];
                }
                
                bool x = cell.RegisterBuilding(building, currentBuilding,
                    GameManager.Instance.Player.CharacterCamera.transform.eulerAngles, preview);


                if (!x && preview)
                {
                    HidePreview();
                }
                
                if (!x && !preview)
                {
                    Destroy(building);
                }
                else
                {
                    if (!preview)
                    {
                        cells.Add(cell);
                    }
                    return true;
                }
            }
            else
            {
                Debug.Log("Cell is null");
            }
        }
        
        return false;
    }

    [PunRPC]
    public void PlaceBuildingRPC(Vector3 point, BuildingType currentBuildingIndex, Vector3 eulerAngles, string id)
    {
        //If we have a current building
        if (buildingPrefabs[currentBuildingIndex] != null)
        {
            Cell cell = null;

            //We check if the cell isnt occupied
            if (!IsCellOccupied(point))
            {
                cell = new Cell(point);
            }
            else
            {
                //We get the cell that is occupied
                foreach (Cell aux in cells)
                {
                    if (aux.Position == point)
                    {
                        cell = aux;
                        break;
                    }
                }

                //We check if the cell is available
            }

            if (cell != null)
            {
                GameObject building = null;

                building = Instantiate(buildingPrefabs[currentBuildingIndex], point,
                    buildingPrefabs[currentBuildingIndex].transform.rotation);

                building.transform.SetParent(grid.transform);
                
                EditBuilding editBuilding = building.GetComponent<EditBuilding>();
                    
                _editBuildings.Add(id, editBuilding);
                
                editBuilding.SetId(id);

                bool x = cell.RegisterBuilding(building, currentBuildingIndex, eulerAngles);

                if (!x)
                {
                    Destroy(building);
                }
                else
                {
                    cells.Add(cell);
                }
            }
            else
            {
                Debug.Log("Cell is null");
            }
        }
    }
    
    public void UpdateBuilding(string id, GameObject building)
    {
        EditBuilding editBuilding = building.GetComponent<EditBuilding>();
        
        _editBuildings[id] = editBuilding;
    }

    [PunRPC]
    public void EditBuildingRPC(bool[] values, string id)
    {
        _editBuildings[id].EditBuildingRPC(values);
    }

    private bool IsCellOccupied(Vector3 point)
    {
        bool aux = false;

        if (cells != null && cells.Count > 0)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].Position == point)
                {
                    aux = true;
                    break;
                }
            }
        }
        
        return aux;
    }

    private void ShowPreview()
    {
        Vector3Int cell1 = Vector3Int.zero;
        
        //We activate the preview of the current building
        HidePreview();
        
        if (buildingPreviewPrefabs[currentBuilding] != null)
        {
            buildingPreviewPrefabs[currentBuilding].SetActive(true);
        }
        
        Transform cameraTransform = GameManager.Instance.Player.CharacterCamera.Camera.transform;
        
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;
        
        Vector3 worldPoint = Vector3.zero;
        
        //If we hit something
        if (Physics.Raycast(ray, out hit, placeDistance))
        {
            //We want to get the point of the hit and convert it to grid coordinates
            Vector3 point = hit.point;
            point = grid.WorldToCell(point);
            
            //We want to get the world position of the grid coordinates
            worldPoint = grid.GetCellCenterWorld(Vector3Int.FloorToInt(point));
        }
        else
        {
            //We get the cell we are looking at
            cell1 = grid.WorldToCell(cameraTransform.position + cameraTransform.forward * placeDistance);
            
            //We get the world position of the cell
            worldPoint = grid.GetCellCenterWorld(cell1);
        }

        if (lastCell != cell1)
        {
            //We want to place the building at the world point
            bool farCell = PlaceBuilding(worldPoint, true);
        
            //If we are too far from the cell we want to place the building at the camera position
            if (!farCell)
            {
                Vector3Int cell = grid.WorldToCell(cameraTransform.position + cameraTransform.forward * (placeDistance/4f));

                if (lastCell != cell)
                {
                    //We get the world position of the cell
                    worldPoint = grid.GetCellCenterWorld(cell);
            
                    PlaceBuilding(worldPoint, true);
            
                    lastCell = cell;
                }
                else
                {
                    HidePreview();
                }
            }
        }
        else
        {
            HidePreview();
        }
    }
    
    private void HidePreview()
    {
        //We deactivate all the previews
        foreach (KeyValuePair<BuildingType, GameObject> buildingPreviewPrefab in buildingPreviewPrefabs)
        {
            if (buildingPreviewPrefab.Value != null)
            {
                buildingPreviewPrefab.Value.SetActive(false);
            }
        }
    }
    
    public Cell GetCell(Vector3 point)
    {
        Cell cell = null;
        
        foreach (Cell aux in cells)
        {
            if (aux.Position == point)
            {
                cell = aux;
                break;
            }
        }
        
        return cell;
    }
}
