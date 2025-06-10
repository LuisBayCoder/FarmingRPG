using System.Collections;
using UnityEngine;

public class GivePlayerQuestItem : MonoBehaviour
{

    private int pendingItemCode = -1;

    // Call this instead of GiveQuestItem directly when paused
    public void QueueQuestItem(int itemCode)
    {
        pendingItemCode = itemCode;
    }

    public void OnGameUnpaused()
    {
        if (pendingItemCode != -1)
        {
            GiveQuestItem(pendingItemCode);
            pendingItemCode = -1;
        }
    }

    //method to give player quest item when called from another script
    //The game is paused when talking to npcs, so this method needs to be called after the game is unpaused
    //Is there a way to call this method after the game is unpaused?
    public void GiveQuestItem(int itemCode)
    {
        InventoryManager.Instance.AddItem(InventoryLocation.player, itemCode);
        Debug.Log($"Quest item with code {itemCode} has been given to the player.");
    }
}
