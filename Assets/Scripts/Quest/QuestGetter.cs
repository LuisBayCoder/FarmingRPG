using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestGetter : MonoBehaviour
{
    private QuestSaveManager _questSaveManager;
    // Start is called before the first frame update
    void Start()
    {
        _questSaveManager = GameObject.Find("CheckActionsBeforeQuest").GetComponent<QuestSaveManager>();
       
    }

    void SendQuestMessage(string messageArgs, string locationName)
    {
       _questSaveManager.SaveCompletedQuest(messageArgs, locationName);
    }
    
    public void SendQuestMessage_EventWrapper(string combinedArgs)
{
    string[] parts = combinedArgs.Split('|');
    if (parts.Length == 2)
    {
        string messageArgs = parts[0];
        string locationName = parts[1];

        
        SendQuestMessage(messageArgs, locationName);
    }
   
}
}
