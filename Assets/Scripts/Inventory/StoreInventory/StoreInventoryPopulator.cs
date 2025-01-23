using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreInventoryPopulator : MonoBehaviour
{
    [SerializeField] private InventoryItem[] itemsToAdd;
    [SerializeField] private InventoryLocation location = InventoryLocation.store;

    private void Start()
    {
        // Access the singleton instance of StoreInventoryManager
        StoreInventoryManager storeInventoryManager = StoreInventoryManager.Instance;

        // Check if the instance is not null
        if (storeInventoryManager != null)
        {
            // Add each item to the store inventory
            foreach (InventoryItem item in itemsToAdd)
            {
                storeInventoryManager.AddItem(location, item);
            }
        }
        else
        {
            Debug.LogError("StoreInventoryManager instance is not available.");
        }
    }
}
