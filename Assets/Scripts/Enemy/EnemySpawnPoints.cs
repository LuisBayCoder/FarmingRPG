using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPoints : MonoBehaviour
{
    // List to hold all spawn points
    // Each child of this GameObject is considered a spawn point
    public List<Transform> spawnPoints = new List<Transform>();
    private void Awake()
    {
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }
    }
}
