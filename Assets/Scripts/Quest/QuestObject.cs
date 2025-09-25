using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObject : MonoBehaviour
{
    [Header("Skull Pedestal")]
    public Transform itemPosition;
    //[SerializeField] private GameObject lightObject;

    [Header("Quest Item")]
    public string itemNameRequired;

    public void Interact(string itemName)
    {
        Debug.Log("Interacting with the pedestal " + itemName);
        //this is called when the player interacts with the pedestal
    }
}


