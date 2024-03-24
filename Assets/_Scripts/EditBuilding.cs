using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EditBuilding : SerializedMonoBehaviour
{
    [SerializeField] 
    private Dictionary<bool[], GameObject> _buildingPrefabs = new Dictionary<bool[], GameObject>();
    
    private Dictionary<string, bool[]> _inverseBuildingPrefabs;
    
    [SerializeField]
    private EditBuildingCells[] _cells;
    
    [FormerlySerializedAs("_editCanvas")] [SerializeField]
    private GameObject _editContainer;
    
    private bool _isEditing = false;

    private GameObject currentBuilding;

    private string buildingName;

    private PhotonView pv;

    private string id = "";

    private void Awake()
    {
        _inverseBuildingPrefabs = new Dictionary<string, bool[]>();
        
        foreach (KeyValuePair<bool[], GameObject> buildingPrefab in _buildingPrefabs)
        {
            _inverseBuildingPrefabs.Add(
                buildingPrefab.Value.name, 
                buildingPrefab.Key);
        }
        
        currentBuilding = this.gameObject;
        
        buildingName = currentBuilding.name;  
        
        pv = BuildingSystem.Instance.Pv;
    }

    [Button]
    private void DebugDictionary()
    {
        //Debug all keys in bool array dictionary
        foreach (KeyValuePair<bool[], GameObject> par in _buildingPrefabs)
        {
            bool[] key = par.Key;
            GameObject value = par.Value;
            
            string keyString = "";
            
            foreach (bool b in key)
            {
                keyString += b + " ";
            }
            
            Debug.Log(keyString + " " + value.name);
        }
    }

    public void Edit()
    {
        _editContainer.SetActive(!_isEditing);
        
        if (_isEditing)
        {
            TryEdit();
        }
        else
        {
            InitializeEdit();
        }
        
        _isEditing = !_isEditing;
    }
    
    public void SetId(string id)
    {
        this.id = id;
    }
    
    private void TryEdit()
    {
        bool[] values = new bool[_cells.Length];
        
        for (int i = 0; i < _cells.Length; i++)
        {
            values[i] = _cells[i].IsEditing;
        }
        
        pv.RPC("EditBuildingRPC", RpcTarget.AllBuffered, values, id);
    }
    
    public void CloseAllCells()
    {
        foreach (EditBuildingCells cell in _cells)
        {
            cell.Reset();
        }
        
        _editContainer.SetActive(false);
        
        _isEditing = false;
    }
    
    public void EditBuildingRPC(bool[] values)
    {
        GameObject prefab = null;

        foreach (KeyValuePair<bool[], GameObject> par in _buildingPrefabs)
        {
            bool[] key = par.Key;
            GameObject value = par.Value;
            
            if (CompareArrays(key, values))
            {
                prefab = value;
                break;
            }
        }

        if (prefab != null)
        {
            GameObject building = currentBuilding;
            
            currentBuilding = Instantiate(prefab, BuildingSystem.Instance.Grid.transform);
            
            //Update position
            currentBuilding.transform.position = building.transform.position;
            
            //Update rotation
            currentBuilding.transform.rotation = building.transform.rotation;
            
            currentBuilding.GetComponent<EditBuilding>().SetId(id);
            
            BuildingSystem.Instance.UpdateBuilding(id, currentBuilding);
            
            Destroy(building);
        }
    }
    
    private bool CompareArrays(bool[] a, bool[] b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }
        
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
            {
                return false;
            }
        }
        
        return true;
    }

    private void InitializeEdit()
    {
        for (int i = 0; i < _cells.Length; i++)
        { 
            _cells[i].Reset();
        }
        
        bool[] values = _inverseBuildingPrefabs[buildingName];

        for (int i = 0; i < values.Length; i++)
        { 
            //TODO Esto va raro
            _cells[i].SetEditing(values[i]);
        }
        
        GameManager.Instance.Player.StartEditing();
    }

    private void Update()
    {
        if (_isEditing && Input.GetMouseButtonDown(1))
        {
            ResetAllCells();
        }
    }
    
    private void ResetAllCells()
    {
        foreach (EditBuildingCells cell in _cells)
        {
            cell.Reset();
        }
    }
}
