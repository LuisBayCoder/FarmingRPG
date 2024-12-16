
[System.Serializable]
public class SceneEnemy
{
    public int enemyCode;
    public Vector3Serializable position;
    public string enemyName;

    public bool isDead;

    public int currentHealth;

    public SceneEnemy()
    {
        position = new Vector3Serializable();
    }
}
