using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SceneTeleportWithLock : MonoBehaviour
{
    [SerializeField] private SceneName sceneNameGoto = SceneName.Scene1_Farm;
    [SerializeField] private Vector3 scenePositionGoto = new Vector3();
    private Player player;
    private bool playerHasKey = false; // This should be set based on your game logic

    private void OnTriggerStay2D(Collider2D collision)
    {

        Player player = collision.GetComponent<Player>();

        if (player != null && playerHasKey)
        {
            //  Calculate players new position

            float xPosition = Mathf.Approximately(scenePositionGoto.x, 0f) ? player.transform.position.x : scenePositionGoto.x;

            float yPosition = Mathf.Approximately(scenePositionGoto.y, 0f) ? player.transform.position.y : scenePositionGoto.y;

            float zPosition = 0f;

            // Teleport to new scene
            SceneControllerManager.Instance.FadeAndLoadScene(sceneNameGoto.ToString(), new Vector3(xPosition, yPosition, zPosition));

        }

        else if (player != null && !playerHasKey)
        {
            DoorIsLockedUI();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Player exited teleport zone");
        // Optionally, you can disable the locked UI when the player exits
        UIManager.Instance.DisableDoorIsLockedUI();
    }
    
    private void DoorIsLockedUI()
    {
        // Implement the logic to show a UI message that the door is locked
        UIManager.Instance.EnableDoorIsLockedUI();
    }
}
