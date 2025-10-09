using UnityEngine;
using System.Collections;
using System.Linq;
using PixelCrushers.DialogueSystem;

public class LumenfruitSetterDebug : MonoBehaviour
{
    // Try setting Lumenfruit by a few common field-name variants and by running Lua
    [ContextMenu("Try Set Lumenfruit (debug)")]
    public void TrySetLumenfruit()
    {
        // Try SetItemField with both a spaced name and an underscored name
        DialogueLua.SetItemField("Lumenfruit", "Is Item", true);
        DialogueLua.SetItemField("Lumenfruit", "Is_Item", true);

        // Also set the Lua variable forms (the condition uses Item["Lumenfruit"].Is_Item)
        DialogueLua.SetVariable("Item[\"Lumenfruit\"].Is_Item", true);
        DialogueLua.SetVariable("Item.Lumenfruit.Is_Item", true);

        // Try running raw Lua as a last resort
        Lua.Run("Item['Lumenfruit'].Is_Item = true");

        // Print readbacks
        DebugCheck();
    }

    // If the Dialogue Manager isn't ready yet, wait for it to exist before trying
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => DialogueManager.instance != null);
        // Optional: auto-try at start once Dialogue Manager exists
        TrySetLumenfruit();
    }

    // Print every item in loaded databases so you can confirm the exact item name and its field titles
    [ContextMenu("Print Item DB")]
    public void PrintItemDatabase()
    {
        // Use DialogueManager.masterDatabase for older versions of Dialogue System
        var db = DialogueManager.masterDatabase;
        if (db == null)
        {
            Debug.LogWarning("No Dialogue Database found in DialogueManager.masterDatabase!");
            return;
        }

        Debug.Log($"DB: {db.name}");
        foreach (var it in db.items)
        {
            string fieldNames = string.Join(", ", it.fields.Select(f => f.title));
            Debug.Log($" - Item: '{it.Name}' fields: [{fieldNames}]");
        }
    }


    // Read back and log the core values to console
    private void DebugCheck()
    {
        var v1 = DialogueLua.GetItemField("Lumenfruit", "Is Item");
        var v2 = DialogueLua.GetItemField("Lumenfruit", "Is_Item");
        var v3 = DialogueLua.GetVariable("Item[\"Lumenfruit\"].Is_Item");
        var v4 = DialogueLua.GetVariable("Item.Lumenfruit.Is_Item");

        // Use .AsBool (property) not .AsBool()
        Debug.Log(
            $"GetItemField('Is Item') = {v1.AsBool} | " +
            $"GetItemField('Is_Item') = {v2.AsBool} | " +
            $"GetVariable(Item[\"Lumenfruit\"].Is_Item) = {v3.AsBool} | " +
            $"GetVariable(Item.Lumenfruit.Is_Item) = {v4.AsBool}"
        );
    }


    // Example pickup snippet — attach to fruit collider if needed
    public void OnPlayerPickup()
    {
        DialogueLua.SetItemField("Lumenfruit", "Is Item", true);
        Debug.Log("Picked up Lumenfruit; check Dialogue variables now.");
    }
}

