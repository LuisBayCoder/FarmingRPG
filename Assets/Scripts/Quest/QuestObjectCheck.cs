using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObjectCheck : MonoBehaviour
{
    [SerializeField] private string itemNameRequired; // The name of the item required for this quest object
    [SerializeField] private GameObject lightObject;
    private QuestManager questManager;

    private void Start()
    {
        if (questManager == null)
        {
            questManager = FindObjectOfType<QuestManager>();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collided object has the Item script
        Item item = collision.gameObject.GetComponent<Item>();
        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(item.ItemCode);

        if (itemDetails != null)
        {
            if (itemDetails.itemDescription == itemNameRequired)
            {
                // Enable the light object to indicate correct placement
                lightObject.SetActive(true);
                Debug.Log("Item placed correctly: " + itemDetails.itemDescription);
                //I need to change registerCorrectPlacement to subtract 1 from the required correct placements
                // Register the correct placement with the quest manager    

                questManager.RegisterCorrectPlacement(1);
            }

        }

        else
        {
            Debug.Log("Item does not match required item: " + itemDetails.itemDescription);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Disable the light object when the item is removed
        lightObject.SetActive(false);
        Debug.Log("Item removed from pedestal");
        questManager.RegisterCorrectPlacement(-1);
    }
}

