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

    public void Interact(string itemName)
    {
        Debug.Log("Interacting with the pedestal " + itemName);
        //this is called when the player interacts with the pedestal
        if (itemName == itemNameRequired)
        {
            isCorrectlyPlaced = true;
            Debug.Log("Item placed correctly");
            
        }
        else
        {
            Debug.Log("Item not correct");
        }
    }
}


