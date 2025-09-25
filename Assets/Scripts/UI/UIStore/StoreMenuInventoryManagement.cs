using System.Collections.Generic;
using UnityEngine;

public class StoreMenuInventoryManagement : MonoBehaviour
{

    [SerializeField] private StoreMenuInventoryManagementSlot[] inventoryManagementSlot = null;

    public GameObject inventoryManagementDraggedItemPrefab;

    [SerializeField] private Sprite transparent16x16 = null;

    [HideInInspector] public GameObject inventoryTextBoxGameobject;


    private void OnEnable()
    {
        EventHandler.InventoryUpdatedEvent += PopulateStoreInventory;
        // Populate player inventory
        if (StoreInventoryManager.Instance != null)
        {
            Debug.Log("StoreMenuInventoryManagement OnEnable");
            PopulateStoreInventory(InventoryLocation.store, StoreInventoryManager.Instance.inventoryLists[(int)InventoryLocation.store]);
        }
        else
        {
            Debug.Log("StoreMenuInventoryManagement StoreInventoryManager.Instance is null");
        }
    }

    private void OnDisable()
    {
        EventHandler.InventoryUpdatedEvent -= PopulateStoreInventory;

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
        for (int i = 0; i < StoreInventoryManager.Instance.inventoryLists[(int)InventoryLocation.store].Count; i++)
        {
            if (inventoryManagementSlot[i].draggedItem != null)
            {
                Destroy(inventoryManagementSlot[i].draggedItem);
            }

        }
    }

    private void PopulateStoreInventory(InventoryLocation inventoryLocation, List<InventoryItem> playerInventoryList)
    {
        if (inventoryLocation == InventoryLocation.store)
        {
            InitialiseInventoryManagementSlots();

            // loop through all player inventory items
            for (int i = 0; i < StoreInventoryManager.Instance.inventoryLists[(int)InventoryLocation.store].Count; i++)
            {
                // Get inventory item details
                inventoryManagementSlot[i].itemDetails = StoreInventoryManager.Instance.GetItemDetails(playerInventoryList[i].itemCode);
                inventoryManagementSlot[i].itemQuantity = playerInventoryList[i].itemQuantity;

                if (inventoryManagementSlot[i].itemDetails != null)
                {
                    // update inventory management slot with image and quantity
                    inventoryManagementSlot[i].storeInventoryManagementSlotImage.sprite = inventoryManagementSlot[i].itemDetails.itemSprite;
                    inventoryManagementSlot[i].textMeshProUGUI.text = inventoryManagementSlot[i].itemQuantity.ToString();
                }
            }
        }
    }

    private void InitialiseInventoryManagementSlots()
    {
        // Clear inventory slots
        for (int i = 0; i < 36 ; i++)
        {
            inventoryManagementSlot[i].itemDetails = null;
            inventoryManagementSlot[i].itemQuantity = 0;
            inventoryManagementSlot[i].storeInventoryManagementSlotImage.sprite = transparent16x16;
            inventoryManagementSlot[i].textMeshProUGUI.text = "";
        }
    }

}