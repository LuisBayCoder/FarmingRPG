using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using PixelCrushers.LoveHate;

namespace PixelCrushers.DialogueSystem.LoveHate
{

    [RequireComponent(typeof(FactionMember))]
    public class LoveHateDialogueActor : DialogueActor
    {

        protected virtual void Start()
        {
            StartCoroutine(CheckAssignments());
        }

        protected IEnumerator CheckAssignments()
        {
            yield return new WaitForEndOfFrame();
            if (string.IsNullOrEmpty(actor))
            {
                Debug.LogWarning("Love/Hate: No actor set on " + name, this);
            }
            var factionMember = GetComponent<FactionMember>();
            if (factionMember == null)
            {
                Debug.LogWarning("Love/Hate: FactionMember component is missing on " + name, this);
            }
            else if (factionMember.faction == null)
            {
                Debug.LogWarning("Love/Hate: No faction set on " + name, this);
            }
            else if (!string.IsNullOrEmpty(actor) && factionMember.faction.name != actor)
            {
                Debug.LogWarning("Love/Hate: Dialogue actor and faction member do not match on " + name, this);
            }
        }
    }
}
