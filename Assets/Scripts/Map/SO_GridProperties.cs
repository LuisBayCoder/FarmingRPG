using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "so_GridProperties", menuName = "Scriptable Objects/Grid Properties")]
public class SO_GridProperties : ScriptableObject
{
    public SceneName sceneName;
    public int gridWidth;
    public int gridHeight;
    public int originX;
    public int originY;


    [SerializeField]
    public List<GridProperty> gridPropertyList;
    //this only used on the enemy for chasing player 

    public bool GetGridProperty(int x, int y, GridBoolProperty propertyType)
    {
        GridProperty gridProperty = gridPropertyList.Find(g => g.gridCoordinate.x == x && g.gridCoordinate.y == y);

        if (gridProperty != null && gridProperty.gridBoolProperty == propertyType)
        {
            return gridProperty.gridBoolValue;
        }

        return false;
    }
}
