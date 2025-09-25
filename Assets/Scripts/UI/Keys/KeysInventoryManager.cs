using System.Collections.Generic;
using UnityEngine;

public class KeysInventoryManager : MonoBehaviour
{

    [SerializeField] private KeyInventoryManagementSlot[] keysManagementSlot = null;

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
            PopulatePlayerInventory(InventoryLocation.keyring, InventoryManager.Instance.inventoryLists[(int)InventoryLocation.keyring]);
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
        for (int i = 0; i < InventoryManager.Instance.inventoryLists[(int)InventoryLocation.keyring].Count; i++)
        {
            if (keysManagementSlot[i].draggedItem != null)
            {
                Destroy(keysManagementSlot[i].draggedItem);
            }

        }
    }

    private void PopulatePlayerInventory(InventoryLocation inventoryLocation, List<InventoryItem> playerInventoryList)
    {
        if (inventoryLocation == InventoryLocation.keyring)
        {
            InitialiseInventoryManagementSlots();

            // loop through all player inventory items
            for (int i = 0; i < InventoryManager.Instance.inventoryLists[(int)InventoryLocation.keyring].Count; i++)
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
        for (int i = 0; i < keysManagementSlot.Length; i++)
        {
            keysManagementSlot[i].itemDetails = null;
            keysManagementSlot[i].itemQuantity = 0;
            keysManagementSlot[i].inventoryManagementSlotImage.sprite = transparent16x16;
            keysManagementSlot[i].textMeshProUGUI.text = "";
        }

    }

}
