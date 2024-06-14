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
}



