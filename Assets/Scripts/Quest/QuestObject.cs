using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObject : MonoBehaviour
{
    private bool isCorrectlyPlaced = false;

    [Header("Skull Pedestal")]
    public Transform itemPosition;

    [Header("Quest Item")]
    public string itemNameRequired;

    public void Interact()
    {
        //this is called when the player drops the item on the pedestal

    }

    public bool IsCorrect() => isCorrectlyPlaced;
}


