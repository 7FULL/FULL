using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    private Vector3 position;
    
    public Vector3 Position => position;
    
    //Each cell has available positions for each type of building
    private Dictionary<BuildingPosition, bool> availablePositions;
    
    //Buildings that are in this cell
    private List<GameObject> buildings;
    
    //Building positions
    private Dictionary<BuildingPosition, Vector3> buildingPositions;
    
    public Cell(Vector3 position)
    {
        this.position = position;
        
        availablePositions = new Dictionary<BuildingPosition, bool>();
        
        buildings = new List<GameObject>();
        
        //We set all the positions to true
        foreach (BuildingPosition buildingPosition in System.Enum.GetValues(typeof(BuildingPosition)))
        {
            availablePositions.Add(buildingPosition, true);
        }
        
        //We add the building positions for each type of building
        buildingPositions = new Dictionary<BuildingPosition, Vector3>();
        
        //Wall
        buildingPositions.Add(BuildingPosition.LEFT, new Vector3(position.x - 2.5f, position.y, position.z));
        buildingPositions.Add(BuildingPosition.RIGHT, new Vector3(position.x + 2.5f, position.y, position.z));
        buildingPositions.Add(BuildingPosition.FRONT, new Vector3(position.x, position.y, position.z + 2.5f));
        buildingPositions.Add(BuildingPosition.BACK, new Vector3(position.x, position.y, position.z - 2.5f));
        
        //Floor
        buildingPositions.Add(BuildingPosition.BOTTOM, new Vector3(position.x, position.y - 2.5f, position.z));
        
        //Roof and stairs
        buildingPositions.Add(BuildingPosition.INSIDE, new Vector3(position.x, position.y, position.z));
    }
    
    private bool IsAvailable(BuildingPosition buildingPosition)
    {
        return availablePositions[buildingPosition];
    }
    
    private void RegisterBuilding(BuildingPosition buildingPosition, GameObject building, BuildingType type, bool preview = false)
    {
        if (!preview)
        {
            availablePositions[buildingPosition] = false;
        
            buildings.Add(building);
        }
        //If its a wall we establish the standard rotation
        if (type == BuildingType.WALL)
        {
            building.transform.eulerAngles = new Vector3(0, 0, 90);
        }
        
        //We set the building position
        building.transform.position = buildingPositions[buildingPosition];
        
        //If it is a Front or Back wall we rotate it
        if (buildingPosition == BuildingPosition.FRONT || buildingPosition == BuildingPosition.BACK)
        {
            building.transform.Rotate(90, 0, 0);
        }
        
        //In case it is a stairs we rotate it in one of the sides depending on the player rotation
        float horizontalAngle = GameManager.Instance.Player.CharacterCamera.transform.eulerAngles.y;
        
        if (type == BuildingType.STAIRS)
        {
            building.transform.eulerAngles = new Vector3(0, 0, 0);
            
            if (horizontalAngle > 45 && horizontalAngle < 135)
            {
                building.transform.Rotate(0, 0, 0);
            }
            else if (horizontalAngle > 135 && horizontalAngle < 225)
            {
                building.transform.Rotate(0, 90, 0);
            }
            else if (horizontalAngle > 225 && horizontalAngle < 315)
            {
                building.transform.Rotate(0, 180, 0);
            }
            else
            {
                building.transform.Rotate(0, -90, 0);
            }
        }
    }

    public bool RegisterBuilding(GameObject building, BuildingType type, Vector3 playerRotation, bool preview = false)
    {
        //We get the nearest building position based on the player rotation
        BuildingPosition buildingPosition = GetNearestBuildingPosition(playerRotation, type);
        
        //We register the building position
        if (buildingPosition != BuildingPosition.NONE)
        {
            RegisterBuilding(buildingPosition, building, type, preview);
            
            return true;
        }
        else
        {
            //Debug.Log("No position available");
        }
        
        //Debug.Log("Building position: " + buildingPosition);
        //Debug.Log("Building type: " + type);
        
        return false;
    }
    
    private BuildingPosition GetNearestBuildingPosition(Vector3 playerRotation, BuildingType type)
    {
        BuildingPosition buildingPosition = BuildingPosition.NONE;
        
        float horizontalAngle = playerRotation.y;
        
        switch (type)
        {
            //If tis a wall we can only place left right front or back
            case BuildingType.WALL:
                if (horizontalAngle > 45 && horizontalAngle < 135 && IsAvailable(BuildingPosition.LEFT))
                {
                    buildingPosition = BuildingPosition.LEFT;
                }
                else if (horizontalAngle > 135 && horizontalAngle < 225 && IsAvailable(BuildingPosition.FRONT))
                {
                    buildingPosition = BuildingPosition.FRONT;
                }
                else if (horizontalAngle > 225 && horizontalAngle < 315 && IsAvailable(BuildingPosition.RIGHT))
                {
                    buildingPosition = BuildingPosition.RIGHT;
                }
                else if (IsAvailable(BuildingPosition.BACK))
                {
                    buildingPosition = BuildingPosition.BACK;
                }
                break;
            //If its a floor we can only placeit on bottom
            case BuildingType.FLOOR:
                if (IsAvailable(BuildingPosition.BOTTOM))
                {
                    buildingPosition = BuildingPosition.BOTTOM;
                }
                break;
            //If its a roof we can only place it inside
            case BuildingType.ROOF:
                if (IsAvailable(BuildingPosition.INSIDE))
                {
                    buildingPosition = BuildingPosition.INSIDE;
                }
                break;
            //If its a stairs we can only place it inside
            case BuildingType.STAIRS:
                if (IsAvailable(BuildingPosition.INSIDE))
                {
                    buildingPosition = BuildingPosition.INSIDE;
                }
                break;
        }
        
        return buildingPosition;
    }
}