using UnityEngine;

public class HoverPauseDebug : MonoBehaviour
{
    void Update()
    {
        // Press 'P' to freeze/unfreeze the game without moving your mouse
        if (Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
            Debug.Log("TimeScale set to: " + Time.timeScale);
        }
    }
}
