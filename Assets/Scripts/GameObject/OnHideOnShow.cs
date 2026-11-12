using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnHideOnShow : MonoBehaviour
{
    public UnityEvent onShowEvent = new UnityEvent();
    public UnityEvent onHideEvent = new UnityEvent();
  // This script can be used to handle actions when the GameObject is shown or hidden
  private void OnEnable()
  {
     onShowEvent.Invoke();
  }

  private void OnDisable()
  {
     onHideEvent.Invoke();
  }
}
