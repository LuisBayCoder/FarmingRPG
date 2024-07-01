using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OnScreenMessage
{
    public GameObject go;
    public float timeToLive;
    public float levitationSpeed;

    public OnScreenMessage(GameObject go, float levitationSpeed)
    {
        this.go = go;
        this.levitationSpeed = levitationSpeed;
    }
}

public class OnScreenMessageSystem : MonoBehaviour
{
    [SerializeField] GameObject textPrefab;
    [SerializeField] float levitationSpeed = 0.5f; // Speed at which the text will move upwards
    [SerializeField] float messageDuration = 2f; // Duration the message will stay on screen

    List<OnScreenMessage> onScreenMessageList;

    private void Awake()
    {
        onScreenMessageList = new List<OnScreenMessage>();
    }

    private void Update()
    {
        for (int i = 0; i < onScreenMessageList.Count; i++)
        {
            OnScreenMessage message = onScreenMessageList[i];
            message.timeToLive -= Time.deltaTime;
            if (message.timeToLive < 0)
            {
                Destroy(message.go);
                onScreenMessageList.RemoveAt(i);
                i--; // Adjust the index since we've removed an element
            }
            else
            {
                // Move the message upwards
                message.go.transform.position += Vector3.up * message.levitationSpeed * Time.deltaTime;
            }
        }
    }

    public void PostMessage(Vector3 worldPosition, string message)
    {
        worldPosition.z = -1f;

        GameObject textGo = Instantiate(textPrefab, transform);
        textGo.transform.position = worldPosition;

        TextMeshPro tmp = textGo.GetComponent<TextMeshPro>();
        tmp.text = message;

        OnScreenMessage onScreenMessage = new OnScreenMessage(textGo, levitationSpeed);
        onScreenMessage.timeToLive = messageDuration;
        onScreenMessageList.Add(onScreenMessage);
    }
}

