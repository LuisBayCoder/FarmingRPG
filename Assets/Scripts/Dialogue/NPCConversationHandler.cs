using UnityEngine;
using PixelCrushers.DialogueSystem;

public class NPCConversationHandler : MonoBehaviour
{
    public NPCMovement npcMovement;
    private bool inConverstion = false;

    private void OnEnable()
    {
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.conversationStarted += OnConversationStart;
            DialogueManager.instance.conversationEnded += OnConversationEnd;
        }
        else
        {
            Debug.LogError("DialogueManager.instance is null. Ensure DialogueManager is in the scene.");
        }
    }

    private void OnDisable()
    {
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.conversationStarted -= OnConversationStart;
            DialogueManager.instance.conversationEnded -= OnConversationEnd;
        }
    }

    private void OnConversationStart(Transform actor)
    {
        Debug.Log("Conversation Started");
        if (npcMovement != null)
        {
            npcMovement.Pause();
            inConverstion = true;
            // Add coins to dialogue system variable based on player's inventory
            int coinCount = InventoryManager.Instance.GetItemQuantityByName("Gold Coin");
            AddCoins(coinCount);
        }
    }

    private void OnConversationEnd(Transform actor)
    {
        Debug.Log("Conversation Ended");
        if (npcMovement != null)
        {
            npcMovement.Unpause();
            inConverstion = false;
        }
    }

    public void PauseOnMouseOver()
    {
        npcMovement.Pause();
    }

    public void UnPauseOnMouseOver()
    {
        if (inConverstion == false)
        {
            npcMovement.Unpause();
        }
    }
    // Method to add coins to the dialogue system variable
    public void AddCoins(int amount)
{
    int coins = DialogueLua.GetVariable("Coins").asInt;
    DialogueLua.SetVariable("Coins", coins + amount);
}
}



