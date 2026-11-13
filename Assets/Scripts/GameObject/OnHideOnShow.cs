using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnHideOnShow : MonoBehaviour
{
    public UnityEvent onShowEvent = new UnityEvent();
    public UnityEvent onHideEvent = new UnityEvent();
    [SerializeField] private CustomQuestJournalUI customQuestJournalUI;

    private void Awake()
    {
        if (customQuestJournalUI == null)
        {
            customQuestJournalUI = GetComponent<CustomQuestJournalUI>();
        }

        if (customQuestJournalUI == null)
        {
            customQuestJournalUI = GetComponentInChildren<CustomQuestJournalUI>(true);
        }
    }

  // This script can be used to handle actions when the GameObject is shown or hidden
  private void OnEnable()
  {
     onShowEvent.Invoke();
     if (customQuestJournalUI == null)
     {
        customQuestJournalUI = FindObjectOfType<CustomQuestJournalUI>(true);
     }
     if (customQuestJournalUI != null)
     {
        customQuestJournalUI.UpdateCustomTabs();
     }
  }

  private void OnDisable()
  {
     onHideEvent.Invoke();
  }
}
