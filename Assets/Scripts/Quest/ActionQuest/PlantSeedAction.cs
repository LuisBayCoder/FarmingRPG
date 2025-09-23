using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSeedAction : MonoBehaviour
{
    [SerializeField] private int seedItemCode;
    [SerializeField] private GameObject enemyPrefab; // Prefab of the enemy to spawn
    [SerializeField] private float spawnInterval = 5f; // Time interval between spawns

    public void PerformAction(Vector3 seedPosition, int seedItemCode)
    {
        this.seedItemCode = seedItemCode;

        if (seedItemCode != 0)
        {
            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(seedItemCode);
            if (itemDetails != null && itemDetails.itemType == ItemType.Seed)
            {
                Debug.Log($"Planting seed: {itemDetails.itemDescription}");
                // Add your planting logic here, using the position
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

    private IEnumerator SpawnEnemiesCoroutine(Vector3 seedPosition)
    {
        // Get grid info from GridPropertiesManager
        Grid grid = FindObjectOfType<Grid>();
        SO_GridProperties gridProperties = GridPropertiesManager.Instance.GetActiveSceneGridProperties();
        if (grid == null || gridProperties == null)
        {
            Debug.LogWarning("Grid or grid properties not found.");
            yield break;
        }

        int minX = gridProperties.originX;
        int minY = gridProperties.originY;
        int maxX = minX + gridProperties.gridWidth - 1;
        int maxY = minY + gridProperties.gridHeight - 1;

        int spawnCount = 4;
        float spawnRadius = 3f;

        for (int i = 0; i < spawnCount; i++)
        {
            // Random offset near seed
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(0.5f, spawnRadius);
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * distance;
            Vector3 rawSpawnPos = seedPosition + offset;

            // Convert to grid cell and clamp to map bounds
            Vector3Int cell = grid.WorldToCell(rawSpawnPos);
            cell.x = Mathf.Clamp(cell.x, minX, maxX);
            cell.y = Mathf.Clamp(cell.y, minY, maxY);

            Vector3 spawnPos = grid.GetCellCenterWorld(cell);

            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
