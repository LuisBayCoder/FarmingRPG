using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers;


//This will only work with "Get" message used in Quest Machine.
//This script is used for checking the inventory and sending the item count to the Quest Machine when a quest starts.
public class CheckActionsBeforeQuest : MonoBehaviour
{
    public void InventoryCheck(string messageArgs)
{
    // Retrieve the item name from the message parameter
    string questItemName = messageArgs;
    Debug.Log($"Quest started! Item to pick up: {questItemName}");

    // Check how many of this item the player already has
    int currentCount = InventoryManager.Instance.GetItemQuantityByName(questItemName);

    if (currentCount > 0)
    {
        Debug.Log($"Re-sending {currentCount} of {questItemName} to Quest Machine.");
        // Re-send the message globally
        MessageSystem.SendMessage(null, "Get", questItemName, currentCount);
    }
}

//this method will only work with "Killed" message used in Quest Machine.
//this method is used for checking the enemy kill count and sending the kill count to the Quest Machine when a quest starts.
public int CheckEnemyKillCount(string sceneName, string parameter)
{
    // Access the SceneEnemiesManager's GameObjectSave data
    if (SceneEnemiesManager.Instance.GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
    {
        // Check if the enemy kill counts exist for the scene
        if (sceneSave.stringIntDictionary != null && sceneSave.stringIntDictionary.TryGetValue(parameter, out int killCount))
        {
            Debug.Log($"Kill Count for parameter '{parameter}' in scene '{sceneName}': {killCount}");
            MessageSystem.SendMessage(null, "Killed", parameter, killCount); // Send the kill count to the Quest Machine
            return killCount;
        }
    }

    Debug.LogWarning($"No data found for scene '{sceneName}' or parameter '{parameter}'.");
    return 0; // Default value if not found
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
        CheckKillCountForSpecificSceneAndParameter();
    }
}

public void SceneChecked(string sceneName)
{
    // I need to add a if the scene was visited before the quest started. 
    // Check if the scene name is valid and exists in the SceneEnemiesManager
    if (SceneEnemiesManager.Instance.GameObjectSave.sceneData.ContainsKey(sceneName))
    {
        Debug.Log($"Scene '{sceneName}' exists in SceneEnemiesManager.");
    }
    else
    {
        Debug.LogWarning($"Scene '{sceneName}' does not exist in SceneEnemiesManager.");
    }
}

}
