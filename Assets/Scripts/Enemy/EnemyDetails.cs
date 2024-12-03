using UnityEngine;

[System.Serializable]
public class EnemyDetails //new code for enemies
{
    public int enemyCode;
    public EnemyType enemyType;
    public string enemyDescription;
 
    public int damageAmount; // This will be used as the hit damage for enemies.
}
