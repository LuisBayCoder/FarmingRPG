using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GenerateGUID))]
public class SceneObjectQuestManager : SingletonMonobehaviour<SceneObjectQuestManager>, ISaveable
{
    private Transform parentQuestObject; // Parent transform for quest objects
    [SerializeField] private SO_ObjectQuestList ObjectQuestListSO = null; // Reference to the ScriptableObject
    [SerializeField] private GameObject questObjectPrefab = null; // Default enemy prefab
    private string _iSaveableUniqueID;
    public string ISaveableUniqueID 
    { 
        get { return _iSaveableUniqueID; } 
        set { _iSaveableUniqueID = value; } 
    }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave 
    { 
        get { return _gameObjectSave; } 
        set { _gameObjectSave = value; } 
    }

    private GameObject currentQuestObject; // Current quest object being interacted with
    private Vector3 savedPosition;
    private int savedQuestObjectCode;


    private Dictionary<string, int> enemyKillCounts = new Dictionary<string, int>();

  
    private void AfterSceneLoad()
    {
        parentQuestObject = GameObject.FindGameObjectWithTag(Tags.QuestObjectParentTransform).transform;
    }

     /// <summary>
    /// Destroy all QuestObjects currently in the scene
    /// </summary>
    private void DestroySceneQuestObjects()
    {
       // Get all enemies in the scene
        ObjectQuest[] objectQuestsInScene = GameObject.FindObjectsOfType<ObjectQuest>();

        // Loop through all scene enemies and destroy them
        for (int i = objectQuestsInScene.Length - 1; i > -1; i--)
        {
            Destroy(objectQuestsInScene[i].gameObject);
        }
    }

    // This method is ideal for spawning a single quest Object on demand, such as during runtime events like triggers or player actions.
    public void InstantiateSceneObjectQuest(int objectQuestType, Vector3 Position)
    {
        GameObject questGameObject = Instantiate(questObjectPrefab, Position, Quaternion.identity, parentQuestObject);
        ObjectQuest objectQuest = questGameObject.GetComponent<ObjectQuest>();
    }

    private void InstantiateSceneObjectQuest()
    {
        if (GameObjectSave.sceneData.TryGetValue(SceneManager.GetActiveScene().name, out SceneSave sceneSave))
        {
            foreach (SceneObjectQuest sceneObjectQuest in sceneSave.listSceneObjectQuest)
            {
                if (!sceneObjectQuest.isActive)
                {
                    // Find the questObject with the saved questObjectCode
                    QuestObjectDetails objectQuestDetail = ObjectQuestListSO.objectDetails.Find(ed => ed.objectsCode == sceneObjectQuest.sceneObjectCode);
                    if (objectQuestDetail != null && objectQuestDetail.objectPrefab != null)
                    {
                        // Instantiate the QuestObject prefab at the saved position
                        GameObject newQuestObject = Instantiate(objectQuestDetail.objectPrefab, new Vector3(sceneObjectQuest.position.x, sceneObjectQuest.position.y, sceneObjectQuest.position.z), Quaternion.identity, parentQuestObject);

                        // Ensure the Animator component is initialized
                        Animator animator = newQuestObject.GetComponent<Animator>();
                        if (animator == null)
                        {
                            Debug.LogError("Animator component not found on the instantiated enemy.");
                        }
                        else
                        {
                            Debug.Log("Animator component found and initialized on the instantiated enemy.");
                            if (animator.runtimeAnimatorController == null)
                            {
                                Debug.LogError("Animator Controller is not assigned to the instantiated enemy.");
                            }
                            else
                            {
                                Debug.Log("Animator Controller is assigned to the instantiated enemy.");
                            }
                        }

                    }

                    else
                    {
                        Debug.LogError($"Enemy with the specified code {sceneObjectQuest.sceneObjectCode} not found or prefab is not assigned.");
                    }
                }
            }
        }
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
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable()
    {
        ISaveableDeregister();
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;

            // Restore data for current scene
            ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            if (sceneSave.listSceneEnemy != null)
            {
                // Destroy existing enemies in the scene
                DestroySceneQuestObjects();

                // Instantiate the list of enemies
                InstantiateSceneObjectQuest();
            }

        }
    }

    public GameObjectSave ISaveableSave()
    {
        // Store current scene data
        ISaveableStoreScene(SceneManager.GetActiveScene().name);

        return GameObjectSave;
    }

    public void ISaveableStoreScene(string sceneName)
    {
        
        // Remove old scene save if it exists
        GameObjectSave.sceneData.Remove(sceneName);

        // Get all enemies in the scene
        List<SceneEnemy> sceneEnemyList = new List<SceneEnemy>();
        
        Enemy[] enemiesInScene = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemiesInScene)
        {
            SceneEnemy sceneEnemy = new SceneEnemy();
            sceneEnemy.enemyCode = enemy.EnemyCode;
            sceneEnemy.position = new Vector3Serializable(enemy.transform.position.x, enemy.transform.position.y, enemy.transform.position.z);
            sceneEnemy.enemyName = enemy.name;
           
            Damageable damageable = enemy.GetComponent<Damageable>();
            if (damageable != null)
            {
                sceneEnemy.isDead = damageable.isDead;
                sceneEnemy.currentHealth = damageable.currentHealth;
            }

            // Add scene enemy to list
            sceneEnemyList.Add(sceneEnemy);
        }

        // Create and store scene data

        SceneSave sceneSave = new SceneSave();
        sceneSave.listSceneEnemy = sceneEnemyList;


        // Save the change thsi to save the quest objects state
       // sceneSave.stringIntDictionary = new Dictionary<string, int>(enemyKillCounts);

        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }
}
