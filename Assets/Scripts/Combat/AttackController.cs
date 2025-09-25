using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    [SerializeField] private OnScreenMessageSystem onScreenMessageSystem; // Reference to OnScreenMessageSystem
    [SerializeField] float offsetDistance = 1f;
    [SerializeField] Vector2 attackAreaSize = new Vector2(1f, 1f);

    Rigidbody2D rgbd2d;

    private void Awake()
    {
        rgbd2d = GetComponent<Rigidbody2D>();
    }

    public void Attack(int damage, Vector2 attackDirection)
    {
        // Calculate the attack position based on the attack direction
        Vector2 position = rgbd2d.position + attackDirection * offsetDistance;
        Debug.Log($"Attack position: {position}, Direction: {attackDirection}");

        // Check for targets in the attack area
        Collider2D[] targets = Physics2D.OverlapBoxAll(position, attackAreaSize, 0f);
        Debug.Log($"Number of targets found: {targets.Length}");

        foreach (Collider2D c in targets)
        {
            Damageable damageable = c.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);

                // Show damage message
                Vector3 messagePosition = c.transform.position;
                string damageMessage = damage.ToString();
                onScreenMessageSystem.PostMessage(messagePosition, damageMessage, 2f);
            }
            else
            {
                Debug.Log("No Damageable component found on target");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (rgbd2d != null)
        {
            Gizmos.color = Color.red;

            // Visualize attack areas in all directions
            Vector2[] directions = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
            foreach (Vector2 dir in directions)
            {
                Vector2 position = rgbd2d.position + dir * offsetDistance;
                Gizmos.DrawWireCube(position, attackAreaSize);
            }
        }
    }
}