using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StoreMenuInventoryManagementSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image storeInventoryManagementSlotImage;
    public TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private StoreMenuInventoryManagement inventoryManagement = null;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;

    [HideInInspector] public ItemDetails itemDetails;
    [HideInInspector] public int itemQuantity;
    [SerializeField] private int slotNumber = 0;

    // private Vector3 startingPosition;
    public GameObject draggedItem;
    private Canvas parentCanvas;
    private bool isPointerOver = false;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        // Check if the left mouse button is clicked while the pointer is over the slot
        if (isPointerOver && Input.GetMouseButtonDown(0))
        {
            MoveItemToInventory();
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
            draggedItemImage.sprite = storeInventoryManagementSlotImage.sprite;
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
            if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.GetComponent<StoreMenuInventoryManagementSlot>() != null)
            {
                // get the slot number where the drag ended
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<StoreMenuInventoryManagementSlot>().slotNumber;

                // Swap inventory items in inventory list
                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber);

                // Destroy inventory text box
                inventoryManagement.DestroyInventoryTextBoxGameobject();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        // Populate text box with item details
        if (itemQuantity != 0)
        {
            // Instantiate inventory text box
            inventoryManagement.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            inventoryManagement.inventoryTextBoxGameobject.transform.SetParent(parentCanvas.transform, false);

            StoreUIInventoryTextBox inventoryTextBox = inventoryManagement.inventoryTextBoxGameobject.GetComponent<StoreUIInventoryTextBox>();

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
            "Buy", // Example value for textBuySale
            "5" // Example price value
            );

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
        isPointerOver = false;
        inventoryManagement.DestroyInventoryTextBoxGameobject();
    }

    private void MoveItemToInventory()
    {
        // Get the item details and quantity
        int itemCode = itemDetails.itemCode;
        int itemQuantity = this.itemQuantity;
        
        // Access the singleton instance of StoreInventoryManager
        InventoryManager storeInventoryManager = InventoryManager.Instance;

        // Add the item to the player's inventory
        // Check if the instance is not null
        if (storeInventoryManager != null)
        {
            storeInventoryManager.AddItem(InventoryLocation.player, itemCode);    
        }

        // Remove the item from the store inventory
        StoreInventoryManager.Instance.RemoveItem(InventoryLocation.store, itemCode);

        // Update the UI
        //inventoryManagement.UpdateInventoryUI();
    }
}

