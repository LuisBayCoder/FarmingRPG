using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GenerateGUID))]
public class SceneEnemiesManager : SingletonMonobehaviour<SceneEnemiesManager>, ISaveable
{
    private Transform parentEnemy;
    [SerializeField] private SO_EnemyList enemyListSO = null; // Reference to the ScriptableObject
    [SerializeField] private GameObject enemyPrefab = null; // Default enemy prefab

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

    private GameObject currentEnemy;
    private Vector3 savedPosition;
    private int savedEnemyCode;

    private void AfterSceneLoad()
    {
        parentEnemy = GameObject.FindGameObjectWithTag(Tags.EnemiesParentTransform).transform;
        RemoveCurrentEnemy();
    }

    private void RemoveCurrentEnemy()
    {
        if (currentEnemy != null)
        {
            Destroy(currentEnemy);
        }
    }

    // This method is ideal for spawning a single enemy on demand, such as during runtime events like triggers or player actions.
    public void InstantiateSceneEnemy(int enemyType, Vector3 enemyPosition)
    {
        GameObject enemyGameObject = Instantiate(enemyPrefab, enemyPosition, Quaternion.identity, parentEnemy);
        Enemy enemy = enemyGameObject.GetComponent<Enemy>();
        //enemy.Init(enemyType);

        // Ensure the Animator component is initialized
        Animator animator = enemyGameObject.GetComponent<Animator>();
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

    private void InstantiateSceneEnemies()
    {
        // Get all enemies in the scene
        Enemy[] enemiesInScene = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemiesInScene)
        {
            // Save the current enemy's position and enemyCode
            savedPosition = enemy.transform.position;
            savedEnemyCode = enemy.EnemyCode;
            Debug.Log($"Found enemy in scene with code: {savedEnemyCode}");

            // Find the enemy with the saved enemyCode
            EnemyDetails enemyDetail = enemyListSO.enemyDetails.Find(ed => ed.enemyCode == savedEnemyCode);
            if (enemyDetail != null && enemyDetail.enemyPrefab != null)
            {
                // Instantiate the enemy prefab at the saved position
                GameObject newEnemy = Instantiate(enemyDetail.enemyPrefab, savedPosition, Quaternion.identity, parentEnemy);
                Debug.Log($"Instantiated enemy with code: {savedEnemyCode}");

                // Ensure the Animator component is initialized
                Animator animator = newEnemy.GetComponent<Animator>();
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
                Debug.LogError($"Enemy with the specified code {savedEnemyCode} not found or prefab is not assigned.");
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
                DestroySceneEnemies();

                // Instantiate the list of enemies
                InstantiateSceneEnemies();
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

            // Add scene enemy to list
            sceneEnemyList.Add(sceneEnemy);
        }

        // Create and store scene data
        SceneSave sceneSave = new SceneSave();
        sceneSave.listSceneEnemy = sceneEnemyList;

        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }

    /// <summary>
    /// Destroy all enemies currently in the scene
    /// </summary>
    private void DestroySceneEnemies()
    {
       // Get all enemies in the scene
        Enemy[] enemiesInScene = GameObject.FindObjectsOfType<Enemy>();

        // Loop through all scene enemies and destroy them
        for (int i = enemiesInScene.Length - 1; i > -1; i--)
        {
            Destroy(enemiesInScene[i].gameObject);
        }
    }
}
