using UnityEngine;

[System.Serializable]
public class ItemDetails
{
    public int itemCode;
    public ItemType itemType;
    public string itemDescription;
    public Sprite itemSprite;
    public string itemLongDescription;
    public short itemUseGridRadius;
    public float itemUseRadius;
    public bool isStartingItem;
    public bool canBePickedUp;
    public bool canBeDropped;
    public bool canBeEaten;
    public bool canBeCarried;
    public bool isWeapon;
    public int damageAmount; // This will be used as the hit damage for tools
    public int staminaAmount;
    public int itemCost; // This will be used as the cost to buy the item
    public bool resizable; // This will be used to determine if the item can be resized
    public float scaleFactor; // This will be used to determine the scale factor of the item
    public bool isNotTrigger; // This will be used to determine if the item is a trigger or not
    public bool isQuestItem; // This will be used to determine if the item is a quest item
    public string questCountName; // This will be used to determine the quest item name
    public Sprite diagramImage;
}

