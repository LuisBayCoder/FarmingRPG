using UnityEngine;
using PixelCrushers.DialogueSystem;

public class NPCConversationHandler : MonoBehaviour
{
    public NPCMovement npcMovement;
    private bool inConversation = false;

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
            inConversation = true;

            // Get coin count from CoinManager instead of inventory
            int coinCount = CoinManager.Instance != null ? CoinManager.Instance.Coins : 0;
            Debug.Log($"[DEBUG] Coin count from CoinManager: {coinCount}");
            AddCoins(coinCount);
        }
    }

    private void OnConversationEnd(Transform actor)
    {
        Debug.Log("Conversation Ended");
        if (npcMovement != null)
        {
            npcMovement.Unpause();
            inConversation = false;
        }
    }

    public void PauseOnMouseOver()
    {
        npcMovement.Pause();
    }

    public void UnPauseOnMouseOver()
    {
        if (inConversation == false)
        {
            npcMovement.Unpause();
        }
    }
    // Method to add coins to the dialogue system variable
    public void AddCoins(int amount)
{
    // Set Lua PlayerCoins variable to zero before adding
    DialogueLua.SetVariable("PlayerCoins", 0);
    DialogueLua.SetVariable("PlayerCoins", amount);
    Debug.Log("Set PlayerCoins to zero and added " + amount + " coins to dialogue system variable. Total now: " + DialogueLua.GetVariable("PlayerCoins").asInt);
}
}



