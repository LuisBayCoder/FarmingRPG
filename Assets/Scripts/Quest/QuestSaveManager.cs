using System.Collections.Generic;
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

    private Dictionary<string, bool> completedQuests = new Dictionary<string, bool>();

    // This method is called to save a completed quest
    // It takes the quest name and location as parameters
    public void SaveCompletedQuest(string questName, string location)
    {
        if (string.IsNullOrEmpty(questName)) return;

        // Add the quest to the dictionary to track it as completed
        if (!completedQuests.ContainsKey(questName))
        {
            completedQuests[questName] = true; // Default to true since we're removing the explicit bool
        }

        // Persist to save system
        ISaveableStoreScene(SceneManager.GetActiveScene().name);
        Debug.Log($"Quest '{questName}' saved as completed.");
    }

    public bool IsQuestCompleted(string questName)
    {
        return completedQuests.ContainsKey(questName);
    }

    public void ISaveableStoreScene(string sceneName)
    {
     GameObjectSave.sceneData.Remove(sceneName);

        SceneSave sceneSave = new SceneSave();
        sceneSave.stringBoolDictionary = new Dictionary<string, string>();

        foreach (var quest in completedQuests.Keys)
        {
         sceneSave.stringBoolDictionary[quest] = "true"; // Use "true" as a placeholder value
        }

        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }

   public void ISaveableRestoreScene(string sceneName)
    {
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            if (sceneSave.stringBoolDictionary != null)
            {
                completedQuests.Clear();
                foreach (var quest in sceneSave.stringBoolDictionary.Keys)
                {
                    completedQuests[quest] = true; // Default to true since we're removing the explicit bool
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

