using PixelCrushers.QuestMachine;
using UnityEngine;
using PixelCrushers;

public class ItemPickUp : MonoBehaviour
{
    private const int DefaultGoldCoinValue = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item = collision.GetComponent<Item>();

        if (item != null)
        {
            // Get item details
            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(item.ItemCode);

            // if item can be picked up
            if (itemDetails.canBePickedUp == true)
            {
                bool isGoldCoin = item.ItemCode == (int)ItemCode.GoldCoin || itemDetails.itemType == ItemType.GoldCoin;

                if (isGoldCoin)
                {
                    int coinAmount = Mathf.Max(DefaultGoldCoinValue, itemDetails.itemCost);

                    if (CoinManager.Instance != null)
                    {
                        CoinManager.Instance.AddCoins(coinAmount);
                    }
                    else
                    {
                        Debug.LogWarning("CoinManager not found. Gold Coin pickup could not be added.");
                    }

                    Destroy(collision.gameObject);
                }
                else if(itemDetails.itemType != ItemType.Key)
                {
                    // Add item to inventory
                    InventoryManager.Instance.AddItem(InventoryLocation.player, item, collision.gameObject);
                }
                else if(itemDetails.itemType == ItemType.Key)
                {
                    // Add item to keyring inventory
                    InventoryManager.Instance.AddItem(InventoryLocation.keyring, item, collision.gameObject);
                }

                // Play pick up sound
                AudioManager.Instance.PlaySound(SoundName.effectPickupSound);

                if (itemDetails.isQuestItem == true)
                {
                    MessageSystem.SendMessage(null, "Get", itemDetails.questCountName, 1);
                }

                Debug.Log("itemdetails.itemType: " + ItemType.Note);
                // If it's a note, open the note UI
                if (itemDetails.itemType == ItemType.Note && itemDetails.diagramImage != null)
                {
                    
                    NoteReaderUI.Instance.ShowNote(itemDetails.diagramImage);
                }
            }
        }
    }
}
