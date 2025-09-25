using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneObjectQuest : MonoBehaviour
{
    
    public int sceneObjectCode;
    public Vector3Serializable position;
    public string sceneObjectName;

    public bool isCompleted;
    public bool isActive;

    public SceneObjectQuest()
    {
        position = new Vector3Serializable();
    }
}

