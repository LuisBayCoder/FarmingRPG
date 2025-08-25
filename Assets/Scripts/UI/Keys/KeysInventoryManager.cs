using System.Collections.Generic;
using UnityEngine;

public class KeysInventoryManager : MonoBehaviour
{

    [SerializeField] private KeysInventoryManagementSlot[] keysManagementSlot = null;

    public GameObject inventoryManagementDraggedItemPrefab;

    [SerializeField] private Sprite transparent16x16 = null;

    [HideInInspector] public GameObject inventoryTextBoxGameobject;


    private void OnEnable()
    {
        EventHandler.InventoryUpdatedEvent += PopulatePlayerInventory;

        // Populate player inventory
        if (InventoryManager.Instance != null)
        {
            Debug.Log("PauseMenuInventoryManagement OnEnable");
            PopulatePlayerInventory(InventoryLocation.player, InventoryManager.Instance.inventoryLists[(int)InventoryLocation.player]);
        }
    }

    private void OnDisable()
    {
        EventHandler.InventoryUpdatedEvent -= PopulatePlayerInventory;

        DestroyInventoryTextBoxGameobject();
    }

    public void DestroyInventoryTextBoxGameobject()
    {
        // Destroy inventory text box if created
        if (inventoryTextBoxGameobject != null)
        {
            Destroy(inventoryTextBoxGameobject);
        }
    }

    public void DestroyCurrentlyDraggedItems()
    {
        // loop through all player inventory items
        for (int i = 0; i < InventoryManager.Instance.inventoryLists[(int)InventoryLocation.player].Count; i++)
        {
            if (keysManagementSlot[i].draggedItem != null)
            {
                Destroy(keysManagementSlot[i].draggedItem);
            }

        }
    }

    private void PopulatePlayerInventory(InventoryLocation inventoryLocation, List<InventoryItem> playerInventoryList)
    {
        if (inventoryLocation == InventoryLocation.player)
        {
            InitialiseInventoryManagementSlots();

            // loop through all player inventory items
            for (int i = 0; i < InventoryManager.Instance.inventoryLists[(int)InventoryLocation.player].Count; i++)
            {
                // Get inventory item details
                keysManagementSlot[i].itemDetails = InventoryManager.Instance.GetItemDetails(playerInventoryList[i].itemCode);
                keysManagementSlot[i].itemQuantity = playerInventoryList[i].itemQuantity;

                if (keysManagementSlot[i].itemDetails != null)
                {
                    // update inventory management slot with image and quantity
                    keysManagementSlot[i].inventoryManagementSlotImage.sprite = keysManagementSlot[i].itemDetails.itemSprite;
                    keysManagementSlot[i].textMeshProUGUI.text = keysManagementSlot[i].itemQuantity.ToString();
                }
            }
        }
    }

    private void InitialiseInventoryManagementSlots()
    {
        // Clear inventory slots
        for (int i = 0; i < Settings.playerMaximumInventoryCapacity; i++)
        {
            keysManagementSlot[i].greyedOutImageGO.SetActive(false);
            keysManagementSlot[i].itemDetails = null;
            keysManagementSlot[i].itemQuantity = 0;
            keysManagementSlot[i].inventoryManagementSlotImage.sprite = transparent16x16;
            keysManagementSlot[i].textMeshProUGUI.text = "";
        }

        // Grey out unavailable slots
        for (int i = InventoryManager.Instance.inventoryListCapacityIntArray[(int)InventoryLocation.player]; i < Settings.playerMaximumInventoryCapacity; i++)
        {
            keysManagementSlot[i].greyedOutImageGO.SetActive(true);
        }
    }

}
