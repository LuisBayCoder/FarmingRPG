using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public int currentHealth = 100;

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        // Handle death (destroy the game object, play animation, etc.)
        Destroy(gameObject);
    }
}
