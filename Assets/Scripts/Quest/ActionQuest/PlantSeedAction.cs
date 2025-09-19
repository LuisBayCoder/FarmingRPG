using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSeedAction : MonoBehaviour
{
    [SerializeField] private int seedItemCode;
    [SerializeField] private GameObject enemyPrefab; // Prefab of the enemy to spawn
    [SerializeField] private float spawnInterval = 5f; // Time interval between spawns
    [SerializeField] private float spawnRadius = 5f; // Radius around the seed to spawn enemies

    // Overload to accept position
    public void PerformAction(Vector3 seedPosition)
    {
        if (seedItemCode != 0)
        {
            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(seedItemCode);
            if (itemDetails != null && itemDetails.itemType == ItemType.Seed)
            {
                Debug.Log($"Planting seed: {itemDetails.itemDescription}");
                StartCoroutine(SpawnEnemiesCoroutine(seedPosition));
            }
            else
            {
                Debug.LogWarning("The item code does not correspond to a seed item.");
            }
        }
        else
        {
            Debug.LogWarning("Seed item code is not set.");
        }
        Debug.Log("Plant Seed Action performed");
    }

    // Old PerformAction for compatibility (optional)
    public void PerformAction()
    {
        PerformAction(transform.position);
    }

    private IEnumerator SpawnEnemiesCoroutine(Vector3 centerPosition)
    {
        yield return new WaitForSeconds(spawnInterval);

        // Get map size and origin from GridPropertiesManager
        Vector2Int gridDimensions, gridOrigin;
        SceneName currentScene = SceneControllerManager.Instance.GetCurrentScene();
        bool found = GridPropertiesManager.Instance.GetGridDimensions(currentScene, out gridDimensions, out gridOrigin);

        int spawnCount = 4;
        for (int i = 0; i < spawnCount; i++)
        {
            // Random angle and distance
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(0.5f * spawnRadius, spawnRadius);
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * distance;
            Vector3 spawnPos = centerPosition + offset;

            // Clamp to map boundaries if map info found
            if (found)
            {
                // Convert world position to grid coordinates
                Grid grid = FindObjectOfType<Grid>();
                Vector3Int gridPos = grid.WorldToCell(spawnPos);

                // Clamp grid coordinates to map boundaries
                int minX = gridOrigin.x;
                int minY = gridOrigin.y;
                int maxX = gridOrigin.x + gridDimensions.x - 1;
                int maxY = gridOrigin.y + gridDimensions.y - 1;

                gridPos.x = Mathf.Clamp(gridPos.x, minX, maxX);
                gridPos.y = Mathf.Clamp(gridPos.y, minY, maxY);

                // Convert back to world position (center of cell)
                spawnPos = grid.GetCellCenterWorld(gridPos);
            }

            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
    }
}
