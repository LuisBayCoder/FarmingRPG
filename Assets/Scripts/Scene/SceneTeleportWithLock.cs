using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SceneTeleportWithLock : MonoBehaviour
{
    [SerializeField] private SceneName sceneNameGoto = SceneName.Scene1_Farm;
    [SerializeField] private Vector3 scenePositionGoto = new Vector3();
    [SerializeField] private int requiredKeyItemCode = 0; // Set this to the item code of the required key
    private Player player;

    private void OnTriggerStay2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        
        if (player != null)
        {
            // Check if the player has the required key in keyring
            bool playerHasKey = CheckPlayerHasKey();

            if (playerHasKey)
            {
                //  Calculate players new position
                float xPosition = Mathf.Approximately(scenePositionGoto.x, 0f) ? player.transform.position.x : scenePositionGoto.x;
                float yPosition = Mathf.Approximately(scenePositionGoto.y, 0f) ? player.transform.position.y : scenePositionGoto.y;
                float zPosition = 0f;

                // Teleport to new scene
                SceneControllerManager.Instance.FadeAndLoadScene(sceneNameGoto.ToString(), new Vector3(xPosition, yPosition, zPosition));
            }
            else
            {
                DoorIsLockedUI();
            }
        }
    }

    private bool CheckPlayerHasKey()
    {
        // Check if InventoryManager exists
        if (InventoryManager.Instance == null)
            return false;

        // Get the keyring inventory list
        List<InventoryItem> keyringInventory = InventoryManager.Instance.inventoryLists[(int)InventoryLocation.keyring];

        // Check if the required key exists in the keyring
        foreach (InventoryItem item in keyringInventory)
        {
            if (item.itemCode == requiredKeyItemCode && item.itemQuantity > 0)
            {
                return true;
            }
        }

        return false;
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
