using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "so_EnemyList", menuName = "Scriptable Objects/Enemy/Enemy List")]
public class SO_EnemyList : ScriptableObject
{
    [SerializeField]
    public List<EnemyDetails> enemyDetails;
}
