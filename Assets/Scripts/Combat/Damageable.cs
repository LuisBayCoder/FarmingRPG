using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    internal void TakeDamage(int damage)
    {
        Destroy(gameObject);
    }
}
