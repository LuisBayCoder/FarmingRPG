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

        // Get the Enemy script
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null && enemy.enemyList != null)
        {
            // Find the corresponding EnemyDetails in the SO_EnemyList
            EnemyDetails enemyDetails = enemy.enemyList.enemyDetails.Find(e => e.enemyCode == enemy.EnemyCode);
            if (enemyDetails != null)
            {
                // Set the isDead property to true
                enemyDetails.isDead = true;
            }
        }

        // Destroy NPCMovement first if it exists
        NPCMovement npcMovement = GetComponent<NPCMovement>();
        if (npcMovement != null)
        {
            Destroy(npcMovement);
        }

        // Collect all components to be destroyed except Enemy, Damageable, and Transform
        List<Component> componentsToDestroy = new List<Component>();
        Component[] components = GetComponents<Component>();
        foreach (Component component in components)
        {
            if (component != null && !(component is Enemy) && !(component is Damageable) && !(component is Transform))
            {
                componentsToDestroy.Add(component);
            }
        }

        // Destroy collected components
        foreach (Component component in componentsToDestroy)
        {
            Destroy(component);
        }

        // Destroy this component last
        Destroy(this);
        // Handle death (destroy the game object, play animation, etc.)
        //Destroy(gameObject);
    }
}