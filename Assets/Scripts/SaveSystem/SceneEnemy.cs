
[System.Serializable]
public class SceneEnemy
{
    public int enemyCode;
    public Vector3Serializable position;
    public string enemyName;

    public SceneEnemy()
    {
        position = new Vector3Serializable();
    }
}
