using UnityEngine;
using PixelCrushers.DialogueSystem;

public class NPCConversationHandler : MonoBehaviour
{
    public NPCMovement npcMovement;
    private bool inConversation = false;

    private bool IsThisNpcConversation(Transform actor)
    {
        // Dialogue System can pass either this transform or a child transform as the actor.
        if (actor != null && (actor == transform || actor.IsChildOf(transform)))
        {
            return true;
        }

        // Fallback to current conversant for cases where the actor event arg isn't this NPC root.
        Transform conversant = DialogueManager.currentConversant;
        return conversant != null && (conversant == transform || conversant.IsChildOf(transform));
    }

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
        if (!IsThisNpcConversation(actor))
        {
            return;
        }

        Debug.Log($"Conversation Started for {gameObject.name}");
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
        if (!IsThisNpcConversation(actor))
        {
            return;
        }

        Debug.Log($"Conversation Ended for {gameObject.name}");
        if (npcMovement != null)
        {
            npcMovement.Unpause();
        }

        inConversation = false;
    }

    public void PauseOnMouseOver()
    {
        if (npcMovement != null)
        {
            npcMovement.Pause();
        }
    }

    public void UnPauseOnMouseOver()
    {
        if (npcMovement != null && inConversation == false)
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



