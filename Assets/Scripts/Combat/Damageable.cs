using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public int currentHealth = 100;
    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private GameObject objectDrop;

    private Flash flash;

    private void Awake()
    {
        flash = GetComponent<Flash>();
    }
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        StartCoroutine(flash.FlashRoutine());
        if (currentHealth <= 0)
        {
            StartCoroutine(CheckDetectDeathRoutine());
        }
    }

    private IEnumerator CheckDetectDeathRoutine()
    {
        yield return new WaitForSeconds(flash.GetRestoreMatTime());
        Die();
    }

    private void Die()
    {
        Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
        Instantiate(objectDrop, transform.position, Quaternion.identity);
        // Handle death (destroy the game object, play animation, etc.)
        Destroy(gameObject);
    }
}
