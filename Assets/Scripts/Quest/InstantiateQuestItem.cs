using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateQuestItem : MonoBehaviour
{
  [SerializeField] private GameObject itemPrefab;

  public void InstantiateObject()
  {
    Instantiate(itemPrefab, transform.position, Quaternion.identity);
  }
}
