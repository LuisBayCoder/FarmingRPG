using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("Quest Objects")]
    public List<QuestObject> questObjects = new List<QuestObject>();

    [Header("Door Object")]
    public GameObject doorToOpen;

    private int correctPlacements = 0;
    public int requiredCorrectPlacements = 3; // how many are needed to complete the puzzle

    public void RegisterCorrectPlacement(int amount)
    {
        correctPlacements += amount;

        Debug.Log("Correct placements: " + correctPlacements);

        if (correctPlacements >= requiredCorrectPlacements)
        {
            CompleteQuest();
        }
    }

    private void CompleteQuest()
    {
        Debug.Log("Puzzle complete! Opening door.");
        if (doorToOpen != null)
        {
            doorToOpen.SetActive(false); // could also play animation or sound
        }
    }
}

