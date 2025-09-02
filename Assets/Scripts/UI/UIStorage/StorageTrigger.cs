using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageTrigger : MonoBehaviour
{
   private void OnTriggerEnter2D(Collider2D collision)
   {
       if (collision.CompareTag("Player"))
       {
           //find StorageUIManager instance and open storage menu
           StorageUIManager.Instance.EnableStorageMenu();
       }
   }
}
