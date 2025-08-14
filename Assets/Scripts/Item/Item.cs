using UnityEngine;

public class Item : MonoBehaviour
{
    [ItemCodeDescription]
    [SerializeField]
    private int _itemCode;

    public int priceOfItem;

    private SpriteRenderer spriteRenderer;

    public int ItemCode { get { return _itemCode; } set { _itemCode = value; } }

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if(ItemCode != 0)
        {
            Init(ItemCode);
        }
    }

    public void Init(int itemCodeParam)
    {
        if (itemCodeParam != 0)
        {
            ItemCode = itemCodeParam;

            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(ItemCode);

            spriteRenderer.sprite = itemDetails.itemSprite;

            // If item type is reapable then add nudgeable component
            if (itemDetails.itemType == ItemType.Reapable_scenary)
            {
                gameObject.AddComponent<ItemNudge>();
            }

            if (itemDetails.resizable == true)
            {
                ItemScale itemScale = gameObject.AddComponent<ItemScale>();

                // Pass the scale factor to the ItemScale component
                itemScale.SetScale(itemDetails.scaleFactor);
            }

            if (itemDetails.isNotTrigger == true)
            {
                BoxCollider2D boxCollider = gameObject.GetComponent<BoxCollider2D>();
                boxCollider.isTrigger = false;
            }
            if (itemDetails.orderInLayer != 0)
            {
                spriteRenderer.sortingOrder = itemDetails.orderInLayer;
            }

        }
    
    }
}
