using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
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
        buildingPositions.Add(BuildingPosition.LEFT, new Vector3(position.x - 1.5f, position.y, position.z));
        buildingPositions.Add(BuildingPosition.RIGHT, new Vector3(position.x + 1.5f, position.y, position.z));
        buildingPositions.Add(BuildingPosition.FRONT, new Vector3(position.x, position.y, position.z + 1.5f));
        buildingPositions.Add(BuildingPosition.BACK, new Vector3(position.x, position.y, position.z - 1.5f));
        
        //Floor
        buildingPositions.Add(BuildingPosition.BOTTOM, new Vector3(position.x, position.y - 1.5f, position.z));
        
        //Roof and stairs
        buildingPositions.Add(BuildingPosition.INSIDE, new Vector3(position.x, position.y, position.z));
    }
    
    private bool IsAvailable(BuildingPosition buildingPosition)
    {
        bool aux = false;

        if (availablePositions[buildingPosition])
        {
            if (position.y <= 2)
            {
                aux = true;
            }
            else
            {
                switch (buildingPosition)
                {
                    //Stair and roof can only be placed inside
                    case BuildingPosition.INSIDE:
                        if (
                            !availablePositions[BuildingPosition.FRONT]
                            || !availablePositions[BuildingPosition.BACK]
                            || !availablePositions[BuildingPosition.LEFT]
                            || !availablePositions[BuildingPosition.RIGHT]
                            || !availablePositions[BuildingPosition.BOTTOM]
                        )
                        {
                            aux = true;
                        }

                        break;
                    //Floor can only be placed on bottom
                    case BuildingPosition.BOTTOM:
                        if (
                            !availablePositions[BuildingPosition.INSIDE]
                            || !availablePositions[BuildingPosition.FRONT]
                            || !availablePositions[BuildingPosition.BACK]
                            || !availablePositions[BuildingPosition.LEFT]
                            || !availablePositions[BuildingPosition.RIGHT]
                        )
                        {
                            aux = true;
                        }
                        break;
                    //Wall can only be placed on left right front or back
                    case BuildingPosition.LEFT:
                        if (
                            !availablePositions[BuildingPosition.INSIDE]
                            || !availablePositions[BuildingPosition.FRONT]
                            || !availablePositions[BuildingPosition.BACK]
                            || !availablePositions[BuildingPosition.BOTTOM]
                        )
                        {
                            aux = true;
                        }

                        break;
                    case BuildingPosition.RIGHT:
                        if (
                            !availablePositions[BuildingPosition.INSIDE]
                            || !availablePositions[BuildingPosition.FRONT]
                            || !availablePositions[BuildingPosition.BACK]
                            || !availablePositions[BuildingPosition.BOTTOM]
                        )
                        {
                            aux = true;
                        }

                        break;
                    case BuildingPosition.FRONT:
                        if (
                            !availablePositions[BuildingPosition.INSIDE]
                            || !availablePositions[BuildingPosition.LEFT]
                            || !availablePositions[BuildingPosition.RIGHT]
                            || !availablePositions[BuildingPosition.BOTTOM]
                        )
                        {
                            aux = true;
                        }

                        break;
                    case BuildingPosition.BACK:
                        if (
                            !availablePositions[BuildingPosition.INSIDE]
                            || !availablePositions[BuildingPosition.LEFT]
                            || !availablePositions[BuildingPosition.RIGHT]
                            || !availablePositions[BuildingPosition.BOTTOM]
                        )
                        {
                            aux = true;
                        }

                        break;
                }

                //In case we cant place the building checking our cell we check the cells around
                if (!aux)
                {
                    //We get the 16 cells around

                    //Bellow cells
                    Cell below = BuildingSystem.Instance.GetCell(new Vector3(position.x, position.y - 3, position.z));
                    Cell belowLeft =
                        BuildingSystem.Instance.GetCell(new Vector3(position.x - 3, position.y - 3, position.z));
                    Cell belowRight =
                        BuildingSystem.Instance.GetCell(new Vector3(position.x + 3, position.y - 3, position.z));
                    Cell belowFront =
                        BuildingSystem.Instance.GetCell(new Vector3(position.x, position.y - 3, position.z + 3));
                    Cell belowBack =
                        BuildingSystem.Instance.GetCell(new Vector3(position.x, position.y - 3, position.z - 3));

                    //Same level cells
                    Cell left = BuildingSystem.Instance.GetCell(new Vector3(position.x - 3, position.y, position.z));
                    Cell right = BuildingSystem.Instance.GetCell(new Vector3(position.x + 3, position.y, position.z));
                    Cell front = BuildingSystem.Instance.GetCell(new Vector3(position.x, position.y, position.z + 3));
                    Cell back = BuildingSystem.Instance.GetCell(new Vector3(position.x, position.y, position.z - 3));
                    Cell frontRight =
                        BuildingSystem.Instance.GetCell(new Vector3(position.x + 3, position.y, position.z + 3));
                    Cell frontLeft =
                        BuildingSystem.Instance.GetCell(new Vector3(position.x - 3, position.y, position.z + 3));
                    Cell backRight =
                        BuildingSystem.Instance.GetCell(new Vector3(position.x + 3, position.y, position.z - 3));
                    Cell backLeft =
                        BuildingSystem.Instance.GetCell(new Vector3(position.x - 3, position.y, position.z - 3));

                    //Above cells
                    Cell above = BuildingSystem.Instance.GetCell(new Vector3(position.x, position.y + 3, position.z));
                    Cell aboveLeft =
                        BuildingSystem.Instance.GetCell(new Vector3(position.x - 3, position.y + 3, position.z));
                    Cell aboveRight =
                        BuildingSystem.Instance.GetCell(new Vector3(position.x + 3, position.y + 3, position.z));
                    Cell aboveFront =
                        BuildingSystem.Instance.GetCell(new Vector3(position.x, position.y + 3, position.z + 3));
                    Cell aboveBack =
                        BuildingSystem.Instance.GetCell(new Vector3(position.x, position.y + 3, position.z - 3));
                    
                    //We check if we can place our building connecting it to the other cells

                    #region Floor

                    if (BuildingPosition.BOTTOM == buildingPosition)
                    {
                        #region Bellow cells

                        if (below != null)
                        {
                            //If any of the walls are placed we can place the floor
                            if (
                                !below.IsAvailable(BuildingPosition.LEFT)
                                || !below.IsAvailable(BuildingPosition.RIGHT)
                                || !below.IsAvailable(BuildingPosition.FRONT)
                                || !below.IsAvailable(BuildingPosition.BACK)
                                || !below.IsAvailable(BuildingPosition.INSIDE)
                                )
                            {
                                aux = true;
                            }
                        }

                        if (belowBack != null)
                        {
                            //Only if front wall is placed we can place the floor
                            if (!belowBack.IsAvailable(BuildingPosition.FRONT)
                                || !belowBack.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        if (belowFront != null)
                        {
                            //Only if back wall is placed we can place the floor
                            if (!belowFront.IsAvailable(BuildingPosition.BACK)
                                || !belowFront.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        if (belowLeft != null)
                        {
                            //Only if right wall is placed we can place the floor
                            if (!belowLeft.IsAvailable(BuildingPosition.RIGHT)
                                || !belowLeft.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        if (belowRight != null)
                        {
                            //Only if left wall is placed we can place the floor
                            if (!belowRight.IsAvailable(BuildingPosition.LEFT)
                                || !belowRight.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        #endregion
                        
                        #region Same level cells

                        if (left != null)
                        {
                            //Only if right wall is placed we can place the floor
                            if (!left.IsAvailable(BuildingPosition.RIGHT)
                                || !left.IsAvailable(BuildingPosition.BOTTOM)
                                || !left.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        if (right != null)
                        {
                            //Only if left wall is placed we can place the floor
                            if (!right.IsAvailable(BuildingPosition.LEFT)
                                || !right.IsAvailable(BuildingPosition.BOTTOM)
                                || !right.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        if (front != null)
                        {
                            //Only if back wall is placed we can place the floor
                            if (!front.IsAvailable(BuildingPosition.BACK)
                                || !front.IsAvailable(BuildingPosition.BOTTOM)
                                || !front.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        if (back != null)
                        {
                            //Only if front wall is placed we can place the floor
                            if (!back.IsAvailable(BuildingPosition.FRONT)
                                || !back.IsAvailable(BuildingPosition.BOTTOM)
                                || !back.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        #endregion

                        //The is no possibility to place the floor connecting it to the above cells
                    }

                    #endregion

                    #region Stairs

                    if (BuildingPosition.INSIDE == buildingPosition)
                    {
                        #region Below cells

                        if (below != null)
                        {
                            //If any of the walls are placed we can place the floor
                            if (
                                !below.IsAvailable(BuildingPosition.LEFT)
                                || !below.IsAvailable(BuildingPosition.RIGHT)
                                || !below.IsAvailable(BuildingPosition.FRONT)
                                || !below.IsAvailable(BuildingPosition.BACK)
                                || !below.IsAvailable(BuildingPosition.INSIDE)
                                )
                            {
                                aux = true;
                            }
                        }

                        if (belowBack != null)
                        {
                            //Only if front wall is placed we can place the floor
                            if (!belowBack.IsAvailable(BuildingPosition.FRONT)
                                || !belowBack.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        if (belowFront != null)
                        {
                            //Only if back wall is placed we can place the floor
                            if (!belowFront.IsAvailable(BuildingPosition.BACK)
                                || !belowFront.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        if (belowLeft != null)
                        {
                            //Only if right wall is placed we can place the floor
                            if (!belowLeft.IsAvailable(BuildingPosition.RIGHT)
                                || !belowLeft.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        if (belowRight != null)
                        {
                            //Only if left wall is placed we can place the floor
                            if (!belowRight.IsAvailable(BuildingPosition.LEFT)
                                || !belowRight.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        #endregion
                        
                        #region Same level cells
                        if (left != null)
                        {
                            //Only if right wall is placed we can place the floor
                            if (!left.IsAvailable(BuildingPosition.RIGHT)
                                || !left.IsAvailable(BuildingPosition.BOTTOM)
                                || !left.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        if (right != null)
                        {
                            //Only if left wall is placed we can place the floor
                            if (!right.IsAvailable(BuildingPosition.LEFT)
                                || !right.IsAvailable(BuildingPosition.BOTTOM)
                                || !right.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        if (front != null)
                        {
                            //Only if back wall is placed we can place the floor
                            if (!front.IsAvailable(BuildingPosition.BACK)
                                || !front.IsAvailable(BuildingPosition.BOTTOM)
                                || !front.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }

                        if (back != null)
                        {
                            //Only if front wall is placed we can place the floor
                            if (!back.IsAvailable(BuildingPosition.FRONT)
                                || !back.IsAvailable(BuildingPosition.BOTTOM)
                                || !back.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }
                        #endregion
                        
                        #region Above cells
                        if (above != null)
                        {
                            //If any of the walls are placed we can place the floor
                            if (
                                !above.IsAvailable(BuildingPosition.LEFT)
                                || !above.IsAvailable(BuildingPosition.RIGHT)
                                || !above.IsAvailable(BuildingPosition.FRONT)
                                || !above.IsAvailable(BuildingPosition.BACK)
                                || !above.IsAvailable(BuildingPosition.INSIDE)
                                )
                            {
                                aux = true;
                            }
                        }
                        
                        if (aboveBack != null)
                        {
                            //Only if front wall is placed we can place the floor
                            if (!aboveBack.IsAvailable(BuildingPosition.FRONT)
                                || !aboveBack.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }
                        
                        if (aboveFront != null)
                        {
                            //Only if back wall is placed we can place the floor
                            if (!aboveFront.IsAvailable(BuildingPosition.BACK)
                                || !aboveFront.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }
                        
                        if (aboveLeft != null)
                        {
                            //Only if right wall is placed we can place the floor
                            if (!aboveLeft.IsAvailable(BuildingPosition.RIGHT)
                                || !aboveLeft.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }
                        
                        if (aboveRight != null)
                        {
                            //Only if left wall is placed we can place the floor
                            if (!aboveRight.IsAvailable(BuildingPosition.LEFT)
                                || !aboveRight.IsAvailable(BuildingPosition.INSIDE))
                            {
                                aux = true;
                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region Wall

                        #region right

                        if (BuildingPosition.RIGHT == buildingPosition)
                        {
                            #region Bellow cells

                            if (below != null)
                            {
                                //If any of the walls are placed we can place the floor
                                if (
                                    !below.IsAvailable(BuildingPosition.RIGHT)
                                    || !below.IsAvailable(BuildingPosition.INSIDE)
                                    )
                                {
                                    aux = true;
                                }
                            }

                            if (belowRight != null)
                            {
                                //Only if left wall is placed we can place the floor
                                if (!belowRight.IsAvailable(BuildingPosition.LEFT)
                                    || !belowRight.IsAvailable(BuildingPosition.INSIDE))
                                {
                                    aux = true;
                                }
                            }
                            
                            #endregion
                            
                            #region Same level cells
                                if (right != null)
                                {
                                    //Only if left wall is placed we can place the floor
                                    if (!right.IsAvailable(BuildingPosition.LEFT)
                                        || !right.IsAvailable(BuildingPosition.BOTTOM)
                                        || !right.IsAvailable(BuildingPosition.FRONT)
                                        || !right.IsAvailable(BuildingPosition.BACK)
                                        || !right.IsAvailable(BuildingPosition.INSIDE))
                                    {
                                        aux = true;
                                    }
                                }

                                if (front != null)
                                {
                                    //Only if back wall is placed we can place the floor
                                    if (!front.IsAvailable(BuildingPosition.BACK)
                                        || !front.IsAvailable(BuildingPosition.RIGHT))
                                    {
                                        aux = true;
                                    }
                                }

                                if (back != null)
                                {
                                    //Only if front wall is placed we can place the floor
                                    if (!front.IsAvailable(BuildingPosition.FRONT)
                                        || !front.IsAvailable(BuildingPosition.RIGHT))
                                    {
                                        aux = true;
                                    }
                                }
                            #endregion
                            
                            #region Above cells
                                if (above != null)
                                {
                                    //If any of the walls are placed we can place the floor
                                    if (
                                        !above.IsAvailable(BuildingPosition.BOTTOM)
                                        || !above.IsAvailable(BuildingPosition.RIGHT)
                                        || !above.IsAvailable(BuildingPosition.INSIDE)
                                        )
                                    {
                                        aux = true;
                                    }
                                }
                                
                                if (aboveRight != null)
                                {
                                    //Only if left wall is placed we can place the floor
                                    if (!aboveRight.IsAvailable(BuildingPosition.LEFT)
                                        || !aboveRight.IsAvailable(BuildingPosition.INSIDE)
                                        || !aboveRight.IsAvailable(BuildingPosition.BOTTOM)
                                        )
                                    {
                                        aux = true;
                                    }
                                }
                            #endregion
                        }

                        #endregion
                        
                        #region left

                        if (BuildingPosition.LEFT == buildingPosition)
                        {
                            #region Bellow cells
                            if (below != null)
                            {
                                //If any of the walls are placed we can place the floor
                                if (
                                    !below.IsAvailable(BuildingPosition.LEFT)
                                    || !below.IsAvailable(BuildingPosition.INSIDE)
                                )
                                {
                                    aux = true;
                                }
                            }

                            if (belowLeft != null)
                            {
                                //Only if right wall is placed we can place the floor
                                if (!belowLeft.IsAvailable(BuildingPosition.RIGHT)
                                    || !belowLeft.IsAvailable(BuildingPosition.INSIDE))
                                {
                                    aux = true;
                                }
                            }
                            #endregion

                            #region Same level cells

                            if (left != null)
                            {
                                //Only if right wall is placed we can place the floor
                                if (!left.IsAvailable(BuildingPosition.RIGHT)
                                    || !left.IsAvailable(BuildingPosition.BOTTOM)
                                    || !left.IsAvailable(BuildingPosition.INSIDE)
                                    || !left.IsAvailable(BuildingPosition.FRONT)
                                    || !left.IsAvailable(BuildingPosition.BACK)
                                    )
                                {
                                    aux = true;
                                }
                            }

                            if (front != null)
                            {
                                //Only if back wall is placed we can place the floor
                                if (!front.IsAvailable(BuildingPosition.BACK)
                                    || !front.IsAvailable(BuildingPosition.LEFT))
                                {
                                    aux = true;
                                }
                            }

                            if (back != null)
                            {
                                //Only if front wall is placed we can place the floor
                                if (!back.IsAvailable(BuildingPosition.FRONT)
                                    || !back.IsAvailable(BuildingPosition.LEFT)
                                   )
                                {
                                    aux = true;
                                }
                            }

                            #endregion

                            #region Above cells

                            if (above != null)
                            {
                                //If any of the walls are placed we can place the floor
                                if (
                                    !above.IsAvailable(BuildingPosition.BOTTOM)
                                    || !above.IsAvailable(BuildingPosition.LEFT)
                                    || !above.IsAvailable(BuildingPosition.INSIDE)
                                )
                                {
                                    aux = true;
                                }
                            }

                            if (aboveLeft != null)
                            {
                                //Only if front wall is placed we can place the floor
                                if (!aboveBack.IsAvailable(BuildingPosition.RIGHT)
                                    || !aboveBack.IsAvailable(BuildingPosition.INSIDE)
                                    || !aboveBack.IsAvailable(BuildingPosition.BOTTOM)
                                    )
                                {
                                    aux = true;
                                }
                            }
                            #endregion
                        }

                        #endregion

                        #region front

                        if (BuildingPosition.FRONT == buildingPosition)
                        {
                            #region Bellow cells

                            if (below != null)
                            {
                                //If any of the walls are placed we can place the floor
                                if (
                                    !below.IsAvailable(BuildingPosition.FRONT)
                                    || !below.IsAvailable(BuildingPosition.INSIDE)
                                )
                                {
                                    aux = true;
                                }
                            }

                            if (belowFront != null)
                            {
                                //Only if back wall is placed we can place the floor
                                if (!belowFront.IsAvailable(BuildingPosition.BACK)
                                    || !belowFront.IsAvailable(BuildingPosition.INSIDE))
                                {
                                    aux = true;
                                }
                            }
                            #endregion

                            #region Same level cells

                            if (left != null)
                            {
                                //Only if right wall is placed we can place the floor
                                if (!left.IsAvailable(BuildingPosition.RIGHT)
                                    || !left.IsAvailable(BuildingPosition.FRONT)
                                    )
                                {
                                    aux = true;
                                }
                            }

                            if (right != null)
                            {
                                //Only if left wall is placed we can place the floor
                                if (!right.IsAvailable(BuildingPosition.LEFT)
                                    || !right.IsAvailable(BuildingPosition.FRONT)
                                    )
                                {
                                    aux = true;
                                }
                            }

                            if (front != null)
                            {
                                //Only if back wall is placed we can place the floor
                                if (!front.IsAvailable(BuildingPosition.BACK)
                                    || !front.IsAvailable(BuildingPosition.BOTTOM)
                                    || !front.IsAvailable(BuildingPosition.INSIDE)
                                    || !front.IsAvailable(BuildingPosition.RIGHT)
                                    || !front.IsAvailable(BuildingPosition.LEFT)
                                    )
                                {
                                    aux = true;
                                }
                            }
                            #endregion
                            
                            #region Above cells
                            
                                if (above != null)
                                {
                                    //If any of the walls are placed we can place the floor
                                    if (
                                        !above.IsAvailable(BuildingPosition.BOTTOM)
                                        || !above.IsAvailable(BuildingPosition.FRONT)
                                        || !above.IsAvailable(BuildingPosition.INSIDE)
                                    )
                                    {
                                        aux = true;
                                    }
                                }
                                
                                if (aboveFront != null)
                                {
                                    //Only if back wall is placed we can place the floor
                                    if (!aboveFront.IsAvailable(BuildingPosition.BACK)
                                        || !aboveFront.IsAvailable(BuildingPosition.INSIDE)
                                        || !aboveFront.IsAvailable(BuildingPosition.BOTTOM)
                                    )
                                    {
                                        aux = true;
                                    }
                                }
                            #endregion
                        }

                        #endregion
                        
                        #region back
                        
                            #region Bellow cells
                            
                                if (below != null)
                                {
                                    //If any of the walls are placed we can place the floor
                                    if (
                                        !below.IsAvailable(BuildingPosition.BACK)
                                        || !below.IsAvailable(BuildingPosition.INSIDE)
                                    )
                                    {
                                        aux = true;
                                    }
                                }
                                
                                if (belowBack != null)
                                {
                                    //Only if front wall is placed we can place the floor
                                    if (!belowBack.IsAvailable(BuildingPosition.FRONT)
                                        || !belowBack.IsAvailable(BuildingPosition.INSIDE))
                                    {
                                        aux = true;
                                    }
                                }
                                
                            #endregion
                            
                            #region Same level cells
                            
                                if (left != null)
                                {
                                    //Only if right wall is placed we can place the floor
                                    if (!left.IsAvailable(BuildingPosition.RIGHT)
                                        || !left.IsAvailable(BuildingPosition.BACK)
                                    )
                                    {
                                        aux = true;
                                    }
                                }
                                
                                if (right != null)
                                {
                                    //Only if left wall is placed we can place the floor
                                    if (!right.IsAvailable(BuildingPosition.LEFT)
                                        || !right.IsAvailable(BuildingPosition.BACK)
                                    )
                                    {
                                        aux = true;
                                    }
                                }
                                
                                if (back != null)
                                {
                                    //Only if front wall is placed we can place the floor
                                    if (!back.IsAvailable(BuildingPosition.FRONT)
                                        || !back.IsAvailable(BuildingPosition.BOTTOM)
                                        || !back.IsAvailable(BuildingPosition.INSIDE)
                                        || !back.IsAvailable(BuildingPosition.RIGHT)
                                        || !back.IsAvailable(BuildingPosition.LEFT)
                                    )
                                    {
                                        aux = true;
                                    }
                                }
                                
                            #endregion
                            
                            #region Above cells
                            
                                if (above != null)
                                {
                                    //If any of the walls are placed we can place the floor
                                    if (
                                        !above.IsAvailable(BuildingPosition.BOTTOM)
                                        || !above.IsAvailable(BuildingPosition.BACK)
                                        || !above.IsAvailable(BuildingPosition.INSIDE)
                                    )
                                    {
                                        aux = true;
                                    }
                                }
                                
                                if (aboveBack != null)
                                {
                                    //Only if front wall is placed we can place the floor
                                    if (!aboveBack.IsAvailable(BuildingPosition.FRONT)
                                        || !aboveBack.IsAvailable(BuildingPosition.INSIDE)
                                        || !aboveBack.IsAvailable(BuildingPosition.BOTTOM)
                                    )
                                    {
                                        aux = true;
                                    }
                                }
                                
                            #endregion
                        
                        #endregion
                        
                    #endregion
                }
            }
            
        }
        return aux;
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
                if (horizontalAngle > 45 && horizontalAngle < 135 && IsAvailable(BuildingPosition.RIGHT))
                {
                    buildingPosition = BuildingPosition.RIGHT;
                }
                else if (horizontalAngle > 135 && horizontalAngle < 225 && IsAvailable(BuildingPosition.BACK))
                {
                    buildingPosition = BuildingPosition.BACK;
                }
                else if (horizontalAngle > 225 && horizontalAngle < 315 && IsAvailable(BuildingPosition.LEFT))
                {
                    buildingPosition = BuildingPosition.LEFT;
                }
                else if (IsAvailable(BuildingPosition.FRONT))
                {
                    buildingPosition = BuildingPosition.FRONT;
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

    public override string ToString()
    {
        string aux = "";
        
        foreach (KeyValuePair<BuildingPosition, bool> availablePosition in availablePositions)
        {
            aux += availablePosition.Key + ": " + availablePosition.Value + "\n";
        }
        
        aux += "Position: " + position;
        
        return aux;
    }
}