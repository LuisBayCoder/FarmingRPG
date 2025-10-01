using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueOnConversationEnd : MonoBehaviour
{
    private bool fadeOutTrigger = false;

    //turn on using npc dialogue
    public void TurnOnFadeOut()
    {
        fadeOutTrigger = true;
    }
    
    //turn on conversation end
    public void TriggerFadeOut()
    {
        if (!fadeOutTrigger) return;
        // Find the GameObject with the NPCFadeOut script
        NPCFadeOut npcFadeOut = FindObjectOfType<NPCFadeOut>();
        StartCoroutine(npcFadeOut.FadeOut());
        fadeOutTrigger = false;
    }
}
