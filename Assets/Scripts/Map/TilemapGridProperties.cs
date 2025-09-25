using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class TilemapGridProperties : MonoBehaviour
{
#if UNITY_EDITOR
    private Tilemap tilemap;
    [SerializeField] private SO_GridProperties gridProperties = null;
    [SerializeField] private GridBoolProperty gridBoolProperty = GridBoolProperty.diggable;

    private void OnEnable()
    {
        // Only populate in the editor
        if (!Application.IsPlaying(gameObject))
        {
            tilemap = GetComponent<Tilemap>();

            if (gridProperties != null)
            {
                gridProperties.gridPropertyList.Clear();
                UpdateObstacleColliders(); 
            }
        }
    }

    private void OnDisable()
    {        // Only populate in the editor
        if (!Application.IsPlaying(gameObject))
        {
            UpdateGridProperties();

            if (gridProperties != null)
            {
                // This is required to ensure that the updated gridproperties gameobject gets saved when the game is saved - otherwise they are not saved.
                EditorUtility.SetDirty(gridProperties);
            }
        }

        if (!Application.IsPlaying(gameObject))
        {
            RemoveObstacleColliders();
        }
    }

    private void UpdateGridProperties()
    {
        // Compress timemap bounds
        tilemap.CompressBounds();

        // Only populate in the editor
        if (!Application.IsPlaying(gameObject))
        {
            if (gridProperties != null)
            {
                Vector3Int startCell = tilemap.cellBounds.min;
                Vector3Int endCell = tilemap.cellBounds.max;

                for (int x = startCell.x; x < endCell.x; x++)
                {
                    for (int y = startCell.y; y < endCell.y; y++)
                    {
                        TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));

                        if (tile != null)
                        {
                            gridProperties.gridPropertyList.Add(new GridProperty(new GridCoordinate(x, y), gridBoolProperty, true));
                        }
                    }
                }
            }
        }
    }
    private void UpdateObstacleColliders()
    {
        // Compress tilemap bounds
        tilemap.CompressBounds();

        Vector3Int startCell = tilemap.cellBounds.min;
        Vector3Int endCell = tilemap.cellBounds.max;

        for (int x = startCell.x; x < endCell.x; x++)
        {
            for (int y = startCell.y; y < endCell.y; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                if (gridProperties.GetGridProperty(x, y, gridBoolProperty))
                {
                    // Add a BoxCollider2D to the tile
                    GameObject colliderObject = new GameObject("ObstacleCollider");
                    colliderObject.transform.position = tilemap.CellToWorld(cellPosition) + tilemap.cellSize / 2f; // Center the collider
                    colliderObject.transform.parent = this.transform;

                    BoxCollider2D collider = colliderObject.AddComponent<BoxCollider2D>();
                    collider.size = tilemap.cellSize;
                    collider.isTrigger = true; // Set to true if you don't want physical collisions
                    colliderObject.tag = "Obstacle";
                }
            }
        }
    }

    private void Update()
    {        // Only populate in the editor
        if (!Application.IsPlaying(gameObject))
        {
            Debug.Log("DISABLE PROPERTY TILEMAPS");
        }
    }
    private void RemoveObstacleColliders()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "ObstacleCollider")
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
#endif
}
