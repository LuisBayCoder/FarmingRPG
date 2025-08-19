using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sleep : MonoBehaviour
{
    //if the player enters the collider trigger a sleep method. 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SleepPlayer();
        }
    }

    private void SleepPlayer()
    {
        //find the UIManager and call the EnableSleepMenu method
        UIManager.Instance.EnableSleepMenu();
    }
}
