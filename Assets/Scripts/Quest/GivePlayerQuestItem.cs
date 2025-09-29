using System.Collections;
using UnityEngine;

public class GivePlayerQuestItem : MonoBehaviour
{

    private int pendingItemCode = -1;
    private InventoryLocation pendingInventoryLocation;
    
    // call the two methods below from another script to queue the item and inventory location
    public void QueueQuestInventoryLocation(string inventoryLocation)
    {
        // Convert string to InventoryLocation enum
        InventoryLocation inventoryLocationEnum;
        if (!System.Enum.TryParse(inventoryLocation, out inventoryLocationEnum))
        {
            Debug.LogError($"Invalid inventory location: {inventoryLocation}");
            return;
        }
        pendingInventoryLocation = inventoryLocationEnum;
    }
    public void QueueQuestItem(int itemCode)
        {
            pendingItemCode = itemCode;
        }

    // Call this method when the conversation ends
    public void OnConversationEnd()
    {
        if (pendingItemCode != -1 && InventoryManager.Instance != null)
        {
            GiveQuestItem(pendingInventoryLocation, pendingItemCode);
            pendingItemCode = -1;
        }
    }

    //method to give player quest item when called from another script
    //The game is paused when talking to npcs, so this method needs to be called after the game is unpaused
    public void GiveQuestItem(InventoryLocation inventoryLocation, int itemCode)
    {
        InventoryManager.Instance.AddItem(inventoryLocation, itemCode);
        Debug.Log($"Quest item with code {itemCode} has been given to the {inventoryLocation}.");
    }
}
