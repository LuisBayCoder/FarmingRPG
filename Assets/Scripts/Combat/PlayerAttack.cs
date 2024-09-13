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

    // Reference to InventoryManager or wherever equipped items are managed
    private InventoryManager inventoryManager;

    private void Start()
    {
        mainCamera = Camera.main;

        // Find the OnScreenMessageSystem in the scene
        messageSystem = FindObjectOfType<OnScreenMessageSystem>();

        inventoryManager = InventoryManager.Instance; // Assume singleton pattern
        UpdateEquippedTool();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1")) // Change to your attack input
        {
            
        }

        // Check if the attack visualization should end
        if (isAttacking && Time.time >= attackEndTime)
        {
            isAttacking = false;
        }

        // You can call this in an event or on input to update equipped tool
        UpdateEquippedTool();
    }
    private void UpdateEquippedTool()
    {
        // Get currently equipped item details from inventory
        equippedItemDetails = inventoryManager.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if (equippedItemDetails != null && equippedItemDetails.isWeapon)
        {
            // Update attack damage to match the equipped tool's damageAmount
            attackDamage = equippedItemDetails.damageAmount;
        }
        else
        {
            attackDamage = 0; // No weapon equipped or invalid item
        }
    }
    public void Attack()
    {
        if (IsCursorOverEnemy()) // Check if the cursor is over an enemy
        {
            // Calculate the attack box center position with the offset
        Vector2 attackCenter = (Vector2)transform.position + attackOffset;

        // Perform an overlap box to detect enemies in the attack range around the adjusted position
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackCenter, new Vector2(attackRange, attackWidth), 0, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            Damageable enemy = hit.GetComponent<Damageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);

                // Post a message on the screen when an enemy is hit
                if (messageSystem != null)
                {
                    Vector3 messagePosition = hit.transform.position;
                    string messageText = $"-{attackDamage} HP";
                    messageSystem.PostMessage(messagePosition, messageText, 1f);
                }
            }
        }

        // Set the attack visualization
        isAttacking = true;
        attackEndTime = Time.time + attackDuration;
        } 
    }
    public void PerformAttack(Vector2 direction)
    {
        if (attackDamage > 0)
        {
            // Perform attack with the current attackDamage
            Debug.Log($"Attacking with {attackDamage} damage in direction {direction}");
            // Call your attack logic here
        }
    }
    private bool IsCursorOverEnemy()
    {
        // Get the world position of the cursor
        Vector3 cursorWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));

        // Check for colliders at the cursor position
        Collider2D hit = Physics2D.OverlapPoint(cursorWorldPosition, enemyLayer);

        // Check if a collider was hit
        if (hit != null)
        {
            Debug.Log($"Hit object: {hit.gameObject.name}");

            // Check if the object has the tag "Enemy"
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log("Cursor is over an enemy");
                return true;
            }
            else
            {
                Debug.Log("Cursor is not over an enemy");
            }
        }
        else
        {
            Debug.Log("No collider hit");
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (isAttacking)
        {
            // Calculate the attack box center position with the offset for visualization
            Vector2 attackCenter = (Vector2)transform.position + attackOffset;

            // Draw a box in the editor to visualize the attack range around the adjusted position
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackCenter, new Vector3(attackRange, attackWidth, 1));
        }
    }
}

