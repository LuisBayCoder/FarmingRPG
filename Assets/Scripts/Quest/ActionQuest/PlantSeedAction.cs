using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSeedAction : MonoBehaviour
{
    [SerializeField] private int seedItemCode;
    [SerializeField] private GameObject enemyPrefab; // Prefab of the enemy to spawn
    [SerializeField] private float spawnInterval = 5f; // Time interval between spawns

    public void PerformAction()
    {
        if (seedItemCode != 0)
        {
            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(seedItemCode);
            if (itemDetails != null && itemDetails.itemType == ItemType.Seed)
            {
                Debug.Log($"Planting seed: {itemDetails.itemDescription}");
                // Add your planting logic here, using the position
                StartCoroutine(SpawnEnemiesCoroutine());
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

    private IEnumerator SpawnEnemiesCoroutine()
    {
        yield return new WaitForSeconds(spawnInterval);

        EnemySpawnPoints spawnPoints = FindObjectOfType<EnemySpawnPoints>();
        if (spawnPoints == null || spawnPoints.spawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points set for enemies.");
            yield break;
        }

        List<int> usedIndices = new List<int>();
        int spawnCount = Mathf.Min(4, spawnPoints.spawnPoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, spawnPoints.spawnPoints.Count);
            } while (usedIndices.Contains(randomIndex) && usedIndices.Count < spawnPoints.spawnPoints.Count);

            usedIndices.Add(randomIndex);
            Transform spawnPoint = spawnPoints.spawnPoints[randomIndex];
            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
}
