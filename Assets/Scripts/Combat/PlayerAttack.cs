using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public int attackDamage = 10;
    public float attackRange = 2f;
    public float attackWidth = 2f;
    public LayerMask enemyLayer;
    public float attackDuration = 0.1f; // Duration the box is visible
    public Vector2 attackOffset = Vector2.zero; // Offset for attack position

    private bool isAttacking = false;
    private float attackEndTime;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1")) // Change to your attack input
        {
            if (IsCursorOverEnemy()) // Check if the cursor is over an enemy
            {
                Attack();
            }
        }

        // Check if the attack visualization should end
        if (isAttacking && Time.time >= attackEndTime)
        {
            isAttacking = false;
        }
    }

    void Attack()
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
            }
        }

        // Set the attack visualization
        isAttacking = true;
        attackEndTime = Time.time + attackDuration;
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
