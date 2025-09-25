using PixelCrushers.DialogueSystem.Articy.Articy_4_0;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuInventoryManagementSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image inventoryManagementSlotImage;
    public TextMeshProUGUI textMeshProUGUI;
    public GameObject greyedOutImageGO;
    [SerializeField] private PauseMenuInventoryManagement inventoryManagement = null;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;

    [HideInInspector] public ItemDetails itemDetails;
    [HideInInspector] public int itemQuantity;
    [SerializeField] private int slotNumber = 0;
    [SerializeField] private bool isStoreInventoryUI = false;
    [SerializeField] private bool isStorage = false;

    // private Vector3 startingPosition;
    public GameObject draggedItem;
    private Canvas parentCanvas;
    private bool isPointerOverStoreUI = false;
    private bool isPointerOverStorageUI = false;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        // Check if the left mouse button is clicked while the pointer is over the slot
        if (isPointerOverStoreUI && Input.GetMouseButtonDown(0))
        {
            MoveItemToStoreInventory();
        }
        else if (isPointerOverStorageUI && Input.GetMouseButtonDown(0))
        {
            MoveItemToStorageInventory();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemQuantity != 0)
        {
            // Instatiate gameobject as dragged item
            draggedItem = Instantiate(inventoryManagement.inventoryManagementDraggedItemPrefab, inventoryManagement.transform);

            // Get image for dragged item
            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = inventoryManagementSlotImage.sprite;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // move game object as dragged item
        if (draggedItem != null)
        {
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Destroy game object as dragged item
        if (draggedItem != null)
        {
            Destroy(draggedItem);

            // Get object drag is over
            if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlot>() != null)
            {
                // get the slot number where the drag ended
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlot>().slotNumber;

                // Swap inventory items in inventory list
                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber);

                // Destroy inventory text box
                inventoryManagement.DestroyInventoryTextBoxGameobject();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Destroy any existing inventory text box
        inventoryManagement.DestroyInventoryTextBoxGameobject();
        // Populate text box with item details
        if (itemQuantity != 0)
        {
            // Instantiate inventory text box
            inventoryManagement.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            inventoryManagement.inventoryTextBoxGameobject.transform.SetParent(parentCanvas.transform, false);

            if (!isStoreInventoryUI && !isStorage)
            {
                UIInventoryTextBox inventoryTextBox = inventoryManagement.inventoryTextBoxGameobject.GetComponent<UIInventoryTextBox>();

                // Set item type description
                string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

                // Populate text box
                inventoryTextBox.SetTextboxText(
                    itemDetails.itemDescription,
                    itemTypeDescription,
                    "",
                    itemDetails.itemLongDescription,
                    "",
                    "",
                    "", 
                    "" 
                );
            }
            else if (isStoreInventoryUI)
            {
                isPointerOverStoreUI = true;

                //this is the player ui inventory in the store so we need to use the store ui inventory text box
                StoreUIInventoryTextBox storeUIInventoryTextBox = inventoryManagement.inventoryTextBoxGameobject.GetComponent<StoreUIInventoryTextBox>();

                // Set item type description
                string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

                // Populate text box
                storeUIInventoryTextBox.SetTextboxText(
                    itemDetails.itemDescription,
                    itemTypeDescription,
                    "",
                    itemDetails.itemLongDescription,
                    "",
                    "",
                    "Sell", // Example value for textBuySale
                    itemDetails.itemCost.ToString() // Example price value
                );
            }

            else if (isStorage)
            {
                 isPointerOverStorageUI = true;

                //this is the player ui inventory in the store so we need to use the store ui inventory text box
                StoreUIInventoryTextBox storeUIInventoryTextBox = inventoryManagement.inventoryTextBoxGameobject.GetComponent<StoreUIInventoryTextBox>();

                // Set item type description
                string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

                // Populate text box
                storeUIInventoryTextBox.SetTextboxText(
                    itemDetails.itemDescription,
                    itemTypeDescription,
                    "",
                    itemDetails.itemLongDescription,
                    "",
                    "",
                    "Price:  Store item.",
                    itemDetails.itemCost.ToString()
                );
            }

            // Set text box position
            if (slotNumber > 23)
            {
                inventoryManagement.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
                inventoryManagement.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y + 50f, transform.position.z);
            }
            else
            {
                inventoryManagement.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                inventoryManagement.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y - 50f, transform.position.z);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOverStoreUI = false;
        isPointerOverStorageUI = false;
        inventoryManagement.DestroyInventoryTextBoxGameobject();
    }

    private void MoveItemToStoreInventory()
    {
        // Get the item details and quantity
        Debug.Log("MoveItemToStoreInventory" + itemDetails.itemCode);
        int itemCode = itemDetails.itemCode;
        int itemQuantity = this.itemQuantity;

        // Access the singleton instance of StoreInventoryManager
        StoreInventoryManager storeInventoryManager = StoreInventoryManager.Instance;

        // Add the item to the store's inventory
        // Check if the instance is not null
        if (storeInventoryManager != null)
        {
            storeInventoryManager.AddItem(InventoryLocation.store, itemCode);
        }
        SellItem(itemDetails.itemCost);    // Example: Selling items
        // Remove the item from the player's inventory
        InventoryManager.Instance.RemoveItem(InventoryLocation.player, itemCode);
        OnPointerEnter(null);
    }

    private void MoveItemToStorageInventory()
    {
        // Get the item details and quantity
        Debug.Log("MoveItemToStorageInventory" + itemDetails.itemCode);
        int itemCode = itemDetails.itemCode;
        int itemQuantity = this.itemQuantity;

        // Access the singleton instance of StorageInventoryManager
        StorageInventoryManager storageInventoryManager = StorageInventoryManager.Instance;

        // Add the item to the storage's inventory
        // Check if the instance is not null
        if (storageInventoryManager != null)
        {
            storageInventoryManager.AddItem(InventoryLocation.storage, itemCode);
        }

        // Remove the item from the player's inventory
        InventoryManager.Instance.RemoveItem(InventoryLocation.player, itemCode);
        OnPointerEnter(null);
    }

    // Example: Selling items
    public void SellItem(int cropValue)
    {
        CoinManager.Instance.AddCoins(cropValue);
    }
}
