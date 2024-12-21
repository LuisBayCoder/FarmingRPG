using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.QuestMachine;
using UnityEngine;

public class Damageable : MonoBehaviour
{   
    // Add a max health variable. Save the current health on save and load it back on load
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int _currentHealth;
    public string Message;
    public string Parameter;
    public int currentHealth 
    {
        get { return _currentHealth; } 
        set { _currentHealth = value; } 
    }
    public bool isDead { get { return _currentHealth <= 0; } }

    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private GameObject objectDrop;

    private Flash flash;

    private void Awake()
    {
        flash = GetComponent<Flash>();
        _currentHealth = maxHealth; // Initialize current health to max health on Awake
    }

    public void SetHealth(int health)
    {
        _currentHealth = health;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        StartCoroutine(flash.FlashRoutine());
        if (_currentHealth <= 0)
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

        // Send message to quest if Message and Parameter are not empty
        if (!string.IsNullOrEmpty(Message) && !string.IsNullOrEmpty(Parameter))
        {
            SendMessageToQuest();
        }
    }

    void SendMessageToQuest()
    {
        MessageSystem.SendMessage(this.gameObject, Message, Parameter);  
    }
}