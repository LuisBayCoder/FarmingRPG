using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private int _enemyCode;
    
    public int EnemyCode { get { return _enemyCode; } set { _enemyCode = value; } }

    /*
    private void Start()
    {
        
        if (EnemyCode != 0)
        {
           Init(EnemyCode);
        }

        // Ensure the Animator component is initialized
        Animator animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on the enemy.");
        }
        else
        {
            Debug.Log("Animator component found and initialized on the enemy.");
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError("Animator Controller is not assigned to the enemy.");
            }
            else
            {
                Debug.Log("Animator Controller is assigned to the enemy.");
            }
        }
    }
    public void Init(int enemyCodeParam)//maybe use this to get the enemy and check if it's dead or alive
    {
        
        if (enemyCodeParam != 0) 
        {
            // Initialization logic
        }
    }
    */
}
