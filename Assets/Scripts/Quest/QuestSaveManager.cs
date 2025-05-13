using System.Collections.Generic;
using PixelCrushers;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GenerateGUID))]
public class QuestSaveManager : SingletonMonobehaviour<QuestSaveManager>, ISaveable
{
   private string _iSaveableUniqueID;
    public string ISaveableUniqueID 
    { 
        get => _iSaveableUniqueID; 
        set => _iSaveableUniqueID = value; 
    }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave 
    { 
        get => _gameObjectSave; 
        set => _gameObjectSave = value; 
    }

    // Dictionary to store completed quests and their locations
    // The key is the quest name, and the value is the location where it was completed
    // This allows for tracking multiple quests and their respective locations
    //I need to get this dictionary from CheckActionsBeforeQuest.cs and QuestUpdateByCollider.cs
    // This dictionary will be used to store the completed quests and their locations
    
    private Dictionary<string, string> completedQuests = new Dictionary<string, string>();

  // This method is called to save a completed quest
// It takes the quest name and location as parameters
    public void SaveCompletedQuest(string questName, string location)
    {
        if (string.IsNullOrEmpty(questName)) return;

        // Add the quest to the dictionary to track it as completed
        if (!completedQuests.ContainsKey(questName))
        {
            completedQuests[questName] = location; // Save the quest name and location
        }

        // Persist to save system
        ISaveableStoreScene(SceneManager.GetActiveScene().name);
        Debug.Log($"Quest '{questName}' saved as completed at location '{location}'.");
    }

    public bool IsQuestCompleted(string questName)
    {
        return completedQuests.ContainsKey(questName);
    }

    public void ISaveableStoreScene(string sceneName)
    {
        GameObjectSave.sceneData.Remove(sceneName);

        SceneSave sceneSave = new SceneSave();
        sceneSave.stringDictionary = new Dictionary<string, string>();

        // Save completed quests into the SceneSave object
        sceneSave.completedQuestsDictionary = new Dictionary<string, string>(completedQuests);

        GameObjectSave.sceneData.Add(sceneName, sceneSave);

        Debug.Log($"Saving scene data for scene: {sceneName}");
        foreach (var key in sceneSave.completedQuestsDictionary.Keys)
        {
            Debug.Log($"Saved quest: {key}");
        }
    }

   public void ISaveableRestoreScene(string sceneName)
    {
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
    {
        if (sceneSave.completedQuestsDictionary != null)
        {
            completedQuests.Clear();
            foreach (var quest in sceneSave.completedQuestsDictionary)
            {
                completedQuests[quest.Key] = quest.Value;
                Debug.Log($"Restored quest: {quest.Key} at location: {quest.Value}");
            }
        }
    }
    }

    public GameObjectSave ISaveableSave()
    {
        ISaveableStoreScene(SceneManager.GetActiveScene().name);
        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;
            ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    protected override void Awake()
    {
        base.Awake();
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void OnEnable()
    {
        ISaveableRegister();
    }

    private void OnDisable()
    {
        ISaveableDeregister();
    }
}

