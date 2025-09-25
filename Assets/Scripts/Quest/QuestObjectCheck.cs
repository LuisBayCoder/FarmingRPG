using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObjectCheck : MonoBehaviour
{
    [SerializeField] private string itemNameRequired; // The name of the item required for this quest object
    [SerializeField] private GameObject lightObject;
    private QuestManager questManager;
    public bool itemPlaced = false; // Flag to check if the item is placed

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
                collision.gameObject.GetComponent<Collider2D>().enabled = false;
                // Enable the light object to indicate correct placement

                //I need a delay before the light object is enabled only if the item is still there after the delay
                // Start a coroutine to enable the light object after a delay   
                StartCoroutine(EnableLightObjectWithDelay(0.5f, collision.gameObject));
            }

        }

        else
        {
            Debug.Log("Item does not match required item: " + itemDetails.itemDescription);
        }
    }

    private IEnumerator EnableLightObjectWithDelay(float delay, GameObject item)
    {
        yield return new WaitForSeconds(delay);
        // Check if the item is still in the trigger area   
        if (item != null && item.GetComponent<Collider2D>().enabled == false)
        {
            lightObject.SetActive(true);
            questManager.RegisterCorrectPlacement(1);
        }
        else
        {
            Debug.Log("Item was removed before the delay ended.");
        }      
    }
    /*
    private void OnTriggerExit2D(Collider2D collision)
    {
        // Disable the light object when the item is removed
        lightObject.SetActive(false);
        Debug.Log("Item removed from pedestal");
        questManager.RegisterCorrectPlacement(-1);
        itemPlaced = false; // Reset the flag when the item is removed  
    }
    */
}

