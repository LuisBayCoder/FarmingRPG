
using PixelCrushers.QuestMachine;
using UnityEngine;
using PixelCrushers;

public class ItemPickUp : MonoBehaviour
{

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
                // Add item to inventory
                InventoryManager.Instance.AddItem(InventoryLocation.player, item, collision.gameObject);

                // Play pick up sound
                AudioManager.Instance.PlaySound(SoundName.effectPickupSound);

                if(itemDetails.isQuestItem == true)
                {
                    MessageSystem.SendMessage(null, "Get", itemDetails.questCountName, 1);
                } 
            }
        }
    }
}
