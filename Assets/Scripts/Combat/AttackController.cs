using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    [SerializeField] private OnScreenMessageSystem onScreenMessageSystem; // Reference to OnScreenMessageSystem
    [SerializeField] float offsetDistance = 1.5f;
    [SerializeField] Vector2 attackAreaSize = new Vector2(1f, 1f);

    Rigidbody2D rgbd2d;

    private void Awake()
    {
        rgbd2d = GetComponent<Rigidbody2D>();
    }

    public void Attack(int damage, Vector2 lastMotionVector)
    {
        Vector2 position = rgbd2d.position + lastMotionVector * offsetDistance;

        Collider2D[] targets = Physics2D.OverlapBoxAll(position, attackAreaSize, 0f);

        foreach (Collider2D c in targets)
        {
            Damageable damageable = c.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);

                // Show damage message
                Vector3 messagePosition = c.transform.position;
                string damageMessage = damage.ToString();
                onScreenMessageSystem.PostMessage(messagePosition, damageMessage,2f);
            }
        }
    }
}
