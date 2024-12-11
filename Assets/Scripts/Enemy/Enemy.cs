using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private int _enemyCode;
    
    public int EnemyCode { get { return _enemyCode; } set { _enemyCode = value; } }
    
    [SerializeField]
    public SO_EnemyList enemyList; // Reference to the SO_EnemyList scriptable object
}
