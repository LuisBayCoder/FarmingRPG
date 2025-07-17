using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers;


//This will only work with "Get" message used in Quest Machine.
//This script is used for checking the inventory and sending the item count to the Quest Machine when a quest starts.
//Checks item descrition from scriptabe object and sends the item count to the Quest Machine.
public class CheckActionsBeforeQuest : MonoBehaviour
{
    public void InventoryCheck(string messageArgs)
    {
        // I need to add a delay before sending the message to ensure the inventory is updated
        StartCoroutine(InventoryCheckWithDelay(messageArgs));
        // Retrieve the item name from the message parameter
       
    }

    private IEnumerator InventoryCheckWithDelay(string messageArgs)
    {
        // Wait for a short duration to ensure the inventory is updated
        yield return new WaitForSeconds(2f);
         string questItemName = messageArgs;
        Debug.Log($"Quest started! Item to pick up: {questItemName}");

        // Check how many of this item the player already has
        int currentCount = InventoryManager.Instance.GetItemQuantityByName(questItemName);

        //if the count is more than 1 need to send message for each item
        if (currentCount > 1)
        {
            Debug.Log($"Player has {currentCount} of {questItemName}. Sending each item count to Quest Machine.");
            // Send a message for each item in the inventory
            for (int i = 0; i < currentCount; i++)
            {
                MessageSystem.SendMessage(null, "Get", questItemName, 1); // Send one item at a time
            }
        }
        //if the count is 0 then send the message with 0 count
        else if (currentCount == 1)
        {
            Debug.Log($"Player has 1 of {questItemName}. Sending 1 to Quest Machine.");
            MessageSystem.SendMessage(null, "Get", questItemName, 1); // Send one item
        }
        else
        {
            Debug.Log($"Player has no {questItemName}. Sending 0 to Quest Machine.");
            MessageSystem.SendMessage(null, "Get", questItemName, 0); // Send zero count
        }
    }

    //this method will only work with "Killed" message used in Quest Machine.
    //this method is used for checking the enemy kill count and sending the kill count to the Quest Machine when a quest starts.
    //this method takes the scene name and the enemy name as parameters.
    public int CheckEnemyKillCount(string sceneName, string parameter)
    {
        if (SceneEnemiesManager.Instance.GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            if (sceneSave.stringIntDictionary != null && sceneSave.stringIntDictionary.TryGetValue(parameter, out int killCount))
            {
                Debug.Log($"Kill Count for parameter '{parameter}' in scene '{sceneName}': {killCount}");

                for (int i = 0; i < killCount; i++)
                {
                    MessageSystem.SendMessage(null, "Killed", parameter); // Send one message per kill
                }

                return killCount;
            }
        }

        return 0;
    }


    // 👇 UnityEvent-friendly wrapper method
    public void CheckEnemyKillCount_EventWrapper(string combinedArgs)
    {
        string[] parts = combinedArgs.Split('|');
        if (parts.Length == 2)
        {
            string sceneName = parts[0];
            string parameter = parts[1];
            CheckEnemyKillCount(sceneName, parameter);
        }
        else
        {
            Debug.LogWarning("Invalid input format. Use 'SceneName|Parameter'.");
        }
    }

    //the paramater is the scene name that is the name where the dictionary is saved. Like Scene4_Barn. out SceneSave returns true or false if the scene name is found in the dictionary.
    //this works for "Checked" message used in Quest Machine.
    //the parameter is the name of the scene
    public void CheckCompletedQuest(string parameter)
    {
        StartCoroutine(CheckCompletedQuestWithDelay(parameter));
    }

    private IEnumerator CheckCompletedQuestWithDelay(string parameter)
    {
        float delay = 2.0f; // Set the delay duration in seconds
        yield return new WaitForSeconds(delay);

        Debug.Log($"Quest Count for parameter '{parameter}' in scene '{QuestSaveManager.Instance.GameObjectSave.sceneData.TryGetValue(parameter, out SceneSave sceneSave1)}'");
        if (QuestSaveManager.Instance.GameObjectSave.sceneData.TryGetValue(parameter, out SceneSave sceneSave))
        {
            Debug.Log($"Check completedQuestDictionary is true {sceneSave.completedQuestsDictionary} in scene '{sceneSave.completedQuestsDictionary.TryGetValue("Checked", out string questStatus1)}'.");
            // Check if the completed quests dictionary exists
            if (sceneSave.completedQuestsDictionary != null && sceneSave.completedQuestsDictionary.TryGetValue("Checked", out string questStatus))
            {
                Debug.Log($"Check Quest '{questStatus}' is completed in scene '{parameter}'.");
                //questStatus is the barn in this case. Parameter is the scene name.
                MessageSystem.SendMessage(null, "Checked", questStatus); // Send the quest status to the Quest Machine
            }
            else
            {
                Debug.LogWarning($"Quest '{parameter}' not found in completed quests.");
            }
        }
        else
        {
            Debug.LogWarning($"No data found for scene '{parameter}'.");
        }
    }

    void CheckCompletedQuestForSpecificSceneAndParameter()
    {
        // Replace with the name of the scene you want to check
        string targetParameter = "Scene4_Barn"; // Replace with the parameter you want to check

        CheckCompletedQuest(targetParameter);
    }

    void CheckKillCountForSpecificSceneAndParameter()
    {
        string targetSceneName = "Scene4_Barn"; // Replace with the name of the scene you want to check
        string targetParameter = "Slimer"; // Replace with the parameter you want to check

        int killCount = CheckEnemyKillCount(targetSceneName, targetParameter);
        Debug.Log($"Kill Count for parameter '{targetParameter}' in scene '{targetSceneName}': {killCount}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            // Simulate enemy kill count check by parameter
            //CheckKillCountForSpecificSceneAndParameter();
            //CheckCompletedQuestForSpecificSceneAndParameter();
            // Simulate inventory check by item name
            InventoryCheck("Cow Skulls"); // Replace with the actual item name you want to check
        }
    }
}
