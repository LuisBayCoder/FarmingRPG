using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovePlayerQuestItem : MonoBehaviour
{

    private string itemCodeString = "";
    private string itemToInstantiate = "";

    // in the dialogue system, call this method and pass the item description as a parameter to remove the item from player inventory
    public void QueueItemForRemoval(string itemCodeString)
    {
        this.itemCodeString = itemCodeString;
    }

    public void QueueItemToInstantiate(string itemToInstantiate)
    {
        this.itemToInstantiate = itemToInstantiate;
    }

    // remove item from player inventory by item description
    // at the end of the conversation, call this method to remove the item from player inventory
    public void RemoveItemFromInventory()
    {
        if (string.IsNullOrEmpty(itemCodeString))
        {
            Debug.LogWarning("No item code string queued for removal.");
            return;
        }
        // Find the item code by description
        foreach (var kvp in InventoryManager.Instance.GetAllItems())
        {
            if (kvp.Key == itemCodeString)
            {
                // Find the item code from itemDetailsDictionary
                foreach (var itemDetail in InventoryManager.Instance.GetType()
                    .GetField("itemDetailsDictionary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .GetValue(InventoryManager.Instance) as Dictionary<int, ItemDetails>)
                {
                    if (itemDetail.Value.itemDescription == itemCodeString)
                    {
                        int itemCode = itemDetail.Key;
                        InventoryManager.Instance.RemoveItem(InventoryLocation.player, itemCode);
                        Debug.Log($"Removed one '{itemCodeString}' from player inventory.");
                        itemCodeString = "";
                        return;
                    }
                }
            }
        }
        Debug.LogWarning($"Item '{itemCodeString}' not found in player inventory.");
    }

    // I need to instantiate an item at a specific location
    public void InstantiateItemAtLocation()
    {
        if (string.IsNullOrEmpty(itemToInstantiate))
        {
            Debug.LogWarning("No item queued for instantiation.");
            return;
        }

        // Instantiate the item at the specified location
        GameObject itemPrefab = Resources.Load<GameObject>($"Items/{itemToInstantiate}");
        if (itemPrefab != null)
        {
            // Find all spawn points and pick the first one
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("ItemSpawnPoint");
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                GameObject spawnPoint = spawnPoints[0];
                Instantiate(itemPrefab, spawnPoint.transform.position, Quaternion.identity);
                Debug.Log($"Instantiated '{itemToInstantiate}' at {spawnPoint.transform.position}.");
                itemToInstantiate = "";
            }
            else
            {
                Debug.LogWarning("No spawn point found for item instantiation.");
            }
        }
        else
        {
            Debug.LogError($"Item prefab '{itemToInstantiate}' not found.");
        }
    }
}
