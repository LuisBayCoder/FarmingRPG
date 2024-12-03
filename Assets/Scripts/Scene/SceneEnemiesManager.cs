using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//need to fix step by step

[RequireComponent(typeof(GenerateGUID))]
public class SceneEnemiesManager //: SingletonMonobehaviour<SceneEnemiesManager>, ISaveable
{
    /*
    private Transform parentEnemy;
    [SerializeField] private GameObject enemyPrefab = null;

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }

    private void AfterSceneLoad()
    {
        parentEnemy = GameObject.FindGameObjectWithTag(Tags.EnemiesParentTransform).transform;
    }

    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    /// <summary>
    /// Destroy all enemies currently in the scene
    /// </summary>
    private void DestroySceneEnemies()
    {
        Enemy[] enemiesInScene = GameObject.FindObjectsOfType<Enemy>();

        for (int i = enemiesInScene.Length - 1; i > -1; i--)
        {
            Destroy(enemiesInScene[i].gameObject);
        }
    }

    public void InstantiateSceneEnemy(int enemyType, Vector3 enemyPosition)
    {
        GameObject enemyGameObject = Instantiate(enemyPrefab, enemyPosition, Quaternion.identity, parentEnemy);
        Enemy enemy = enemyGameObject.GetComponent<Enemy>();
        enemy.Init(enemyType);
    }

    private void InstantiateSceneEnemies(List<SceneEnemy> sceneEnemyList)
    {
        GameObject enemyGameObject;

        foreach (SceneEnemy sceneEnemy in sceneEnemyList)
        {
            enemyGameObject = Instantiate(enemyPrefab, new Vector3(sceneEnemy.position.x, sceneEnemy.position.y, sceneEnemy.position.z), Quaternion.identity, parentEnemy);

            Enemy enemy = enemyGameObject.GetComponent<Enemy>();
            enemy.EnemyType = sceneEnemy.enemyType;
            enemy.name = sceneEnemy.enemyName;
        }
    }

    private void OnDisable()
    {
        ISaveableDeregister();
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    private void OnEnable()
    {
        ISaveableRegister();
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
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
                InstantiateSceneEnemies(sceneSave.listSceneEnemy);
            }
        }
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
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
            sceneEnemy.enemyType = enemy.EnemyType;
            sceneEnemy.position = new Vector3Serializable(enemy.transform.position.x, enemy.transform.position.y, enemy.transform.position.z);
            sceneEnemy.enemyName = enemy.name;

            // Add scene enemy to list
            sceneEnemyList.Add(sceneEnemy);
        }

        // Create and store scene data
        SceneSave sceneSave = new SceneSave();
        sceneSave.listSceneEnemy = sceneEnemyList;

        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }*/
}
