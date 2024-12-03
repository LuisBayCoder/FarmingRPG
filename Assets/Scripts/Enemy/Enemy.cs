using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [ItemCodeDescription]
    [SerializeField]
    private int _enemyCode;

    //private SpriteRenderer spriteRenderer; don't need?

    public int EnemyCode { get { return _enemyCode; } set { _enemyCode = value; } }

    private void Awake()
    {
        //spriteRenderer = GetComponentInChildren<SpriteRenderer>(); don't need?
    }

    private void Start()
    {
        if (EnemyCode != 0)
        {
           Init(EnemyCode);
        }
    }

   
    public void Init(int itemCodeParam)
    {
        /*
        if (itemCodeParam != 0)
        {
            EnemyCode = itemCodeParam;

            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(EnemyCode);

            spriteRenderer.sprite = itemDetails.itemSprite;

            // If item type is reapable then add nudgeable component
            if (itemDetails.itemType == ItemType.Reapable_scenary)
            {
                gameObject.AddComponent<ItemNudge>();
            }
        }
        */
    }
}
