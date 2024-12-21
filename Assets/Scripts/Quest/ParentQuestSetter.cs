using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentQuestSetter : MonoBehaviour
{
    [SerializeField] private string defaultMessage; // Set the parameter for the quest message
    [SerializeField] private string defaultParameter; // Set the parameter for the quest Parameter

    // Set the quest message and paramater on each child object with damageable component
    void Start()
    {
        SetMessagesForChildren();
    }

    private void SetMessagesForChildren()
    {
        foreach (Transform child in transform)
        {
            Damageable damageable = child.GetComponent<Damageable>();
            if (damageable != null)
            {
                if (string.IsNullOrEmpty(damageable.Message))
                {
                    damageable.Message = defaultMessage;
                }
                if (string.IsNullOrEmpty(damageable.Parameter))
                {
                    damageable.Parameter = defaultParameter;
                }
            }
        }
    }
}
