using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovePlayerQuestItem : MonoBehaviour
{
    private string itemCodeString = "";
    private string itemToInstantiate = "";

    // New: support two items and spawn tags
    private string itemToInstantiateA = "";
    private string itemToInstantiateB = "";
    private string spawnTagA = "ItemSpawnPointA";
    private string spawnTagB = "ItemSpawnPointB";

    [SerializeField] private UIInventoryBar uiBar;

    // in the dialogue system, call this method and pass the item description as a parameter to remove the item from player inventory
    public void QueueItemForRemoval(string itemCodeString)
    {
        this.itemCodeString = itemCodeString;
        // Clear UI inventory slots so the UI reflects the removal
        if (uiBar != null)
        {
            // Clear carried item selection and any highlights/drags
            uiBar.ClearCurrentlySelectedItems();      // clears highlights and calls Player.ClearCarriedItem()
            uiBar.DestroyCurrentlyDraggedItems();     // optional: remove any dragged ghosts
        }
        else
        {
            Debug.LogWarning("UIInventoryBar not found in scene.");
        }
    }

    public void QueueItemToInstantiate(string itemToInstantiate)
    {
        this.itemToInstantiate = itemToInstantiate;
    }

    // New: queue individual items and spawn tags
    public void QueueItemToInstantiateA(string itemName)
    {
        this.itemToInstantiateA = itemName;
    }

    public void QueueItemToInstantiateB(string itemName)
    {
        this.itemToInstantiateB = itemName;
    }

    public void QueueSpawnTagA(string tag)
    {
        if (!string.IsNullOrEmpty(tag)) spawnTagA = tag;
    }

    public void QueueSpawnTagB(string tag)
    {
        if (!string.IsNullOrEmpty(tag)) spawnTagB = tag;
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
    // call this method at the end of the conversation to instantiate the item
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
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("ItemSpawnPointA");
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

    // New: instantiate two different items at two different spawn tags (locations)
    public void InstantiateTwoItemsAtLocations()
    {
        // Instantiate A
        if (!string.IsNullOrEmpty(itemToInstantiateA))
        {
            GameObject prefabA = Resources.Load<GameObject>($"Items/{itemToInstantiateA}");
            if (prefabA != null)
            {
                GameObject spawnA = FindSpawnForTag(spawnTagA);
                if (spawnA != null)
                {
                    Instantiate(prefabA, spawnA.transform.position, Quaternion.identity);
                    Debug.Log($"Instantiated '{itemToInstantiateA}' at {spawnA.transform.position} (tag '{spawnTagA}').");
                }
                else
                {
                    Debug.LogWarning($"No spawn point found with tag '{spawnTagA}' for item '{itemToInstantiateA}'.");
                }
            }
            else
            {
                Debug.LogError($"Item prefab '{itemToInstantiateA}' not found.");
            }

            itemToInstantiateA = "";
        }
        else
        {
            Debug.Log("No item A queued for instantiation.");
        }

        // Instantiate B
        if (!string.IsNullOrEmpty(itemToInstantiateB))
        {
            GameObject prefabB = Resources.Load<GameObject>($"Items/{itemToInstantiateB}");
            if (prefabB != null)
            {
                GameObject spawnB = FindSpawnForTag(spawnTagB);
                if (spawnB != null)
                {
                    Instantiate(prefabB, spawnB.transform.position, Quaternion.identity);
                    Debug.Log($"Instantiated '{itemToInstantiateB}' at {spawnB.transform.position} (tag '{spawnTagB}').");
                }
                else
                {
                    Debug.LogWarning($"No spawn point found with tag '{spawnTagB}' for item '{itemToInstantiateB}'.");
                }
            }
            else
            {
                Debug.LogError($"Item prefab '{itemToInstantiateB}' not found.");
            }

            itemToInstantiateB = "";
        }
        else
        {
            Debug.Log("No item B queued for instantiation.");
        }
    }

    // Helper to find a spawn point by tag: returns first found or null
    private GameObject FindSpawnForTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) tag = "ItemSpawnPoint";
        GameObject[] spawns = GameObject.FindGameObjectsWithTag(tag);
        if (spawns != null && spawns.Length > 0) return spawns[0];
        return null;
    }
}
