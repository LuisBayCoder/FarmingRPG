using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageInventoryPopulator : MonoBehaviour
{
    [SerializeField] private InventoryItem[] itemsToAdd;
    [SerializeField] private InventoryLocation location = InventoryLocation.storage;

    private void Start()
    {
        // Access the singleton instance of StorageInventoryManager
        StorageInventoryManager storageInventoryManager = StorageInventoryManager.Instance;

        // Check if the instance is not null
        if (storageInventoryManager != null)
        {
            // Add each item to the storage inventory
            foreach (InventoryItem item in itemsToAdd)
            {
                storageInventoryManager.AddItem(location, item);
            }
        }
        else
        {
            Debug.LogError("StorageInventoryManager instance is not available.");
        }
    }
}

