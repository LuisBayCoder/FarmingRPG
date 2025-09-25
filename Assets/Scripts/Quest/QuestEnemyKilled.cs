using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestEnemyKilled : MonoBehaviour
{
    [SerializeField] private int killCount; // Set the kill count for the quest 

    public void Start()
    {
        // Initialize the kill count and quest complete status
        killCount = 0;
    }

    void KilledEnemy()
    {
        // Increment the kill count and check if the quest is complete
        killCount++;
        if (killCount >= 1) // Check if the kill count is greater than or equal to 1
        {
            //need to save the kill count amount
            // Save the quest status and complete the quest

        }
    }

}
