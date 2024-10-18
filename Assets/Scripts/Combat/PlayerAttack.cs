using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public int attackDamage = 0;
    public float attackRange = 2f;
    public float attackWidth = 2f;
    public LayerMask enemyLayer;
    public float attackDuration = 0.1f; // Duration the box is visible
    public Vector2 attackOffset = Vector2.zero; // Offset for attack position
    public OnScreenMessageSystem messageSystem; // Reference to OnScreenMessageSystem
    private ItemDetails equippedItemDetails;
    private bool isAttacking = false;
    private float attackEndTime;
    private Camera mainCamera;
    private InventoryManager inventoryManager;

    private void Start()
    {
        mainCamera = Camera.main;
        messageSystem = FindObjectOfType<OnScreenMessageSystem>();
        inventoryManager = InventoryManager.Instance; // Assume singleton pattern
        UpdateEquippedTool();
    }

    private void Update()
    {
        // Check if the attack visualization should end
        if (isAttacking && Time.time >= attackEndTime)
        {
            isAttacking = false;
        }

        // Call this in an event or on input to update equipped tool
        UpdateEquippedTool();
    }

    private void UpdateEquippedTool()
    {
        equippedItemDetails = inventoryManager.GetSelectedInventoryItemDetails(InventoryLocation.player);
        if (equippedItemDetails != null && equippedItemDetails.isWeapon)
        {
            attackDamage = equippedItemDetails.damageAmount;
        }
        else
        {
            attackDamage = 0;
        }
    }

    public void Attack(Vector2 attackDirection)
    {
        AdjustAttackCollider(attackDirection); // Adjust the collider based on the direction

        Vector2 attackCenter = (Vector2)transform.position + attackOffset;
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackCenter, new Vector2(attackRange, attackWidth), 0, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            Damageable enemy = hit.GetComponent<Damageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                if (messageSystem != null)
                {
                    Vector3 messagePosition = hit.transform.position;
                    string messageText = $"-{attackDamage} HP";
                    messageSystem.PostMessage(messagePosition, messageText, 1f);
                }
            }
        }

        isAttacking = true;
        attackEndTime = Time.time + attackDuration;
    }

    private void AdjustAttackCollider(Vector2 attackDirection)
    {
        // Adjust the attack box based on the direction of the attack
        if (attackDirection == Vector2.up)
        {
            attackOffset = new Vector2(0, attackRange / 2);
            attackRange = 2f; // Long vertical attack
            attackWidth = 1f; // Narrow horizontal width
        }
        else if (attackDirection == Vector2.down)
        {
            attackOffset = new Vector2(0, -attackRange / 2);
            attackRange = 2f;
            attackWidth = 1f;
        }
        else if (attackDirection == Vector2.left)
        {
            attackOffset = new Vector2(-attackWidth / 2, 0);
            attackRange = 1f; // Short horizontal attack
            attackWidth = 2f; // Wide vertical width
        }
        else if (attackDirection == Vector2.right)
        {
            attackOffset = new Vector2(attackWidth / 2, 0);
            attackRange = 1f;
            attackWidth = 2f;
        }
    }

    private void OnDrawGizmos()
    {
        if (isAttacking)
        {
            Vector2 attackCenter = (Vector2)transform.position + attackOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackCenter, new Vector3(attackRange, attackWidth, 1));
        }
    }
}