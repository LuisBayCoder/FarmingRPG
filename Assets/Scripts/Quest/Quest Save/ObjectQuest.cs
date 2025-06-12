using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectQuest : MonoBehaviour
{
    [SerializeField]
    private int _objectQuest;
    
    public int ObjectQuestCode { get { return _objectQuest; } set { _objectQuest = value; } }
    
    [SerializeField]
    public SO_ObjectQuestList objectQuestList; // Reference to the SO_EnemyList scriptable object
}
