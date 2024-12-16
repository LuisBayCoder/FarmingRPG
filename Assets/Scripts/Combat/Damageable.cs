using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{   
    //fix this add a max health variable. Save the current health on save and load it back on load
    [SerializeField] private int maxHealth = 100;
    public int currentHealth {get{return currentHealth;} set{currentHealth = value;}}
    public bool isDead { get { return currentHealth <= 0; } }

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
    }
}