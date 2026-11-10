using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

public static class DialogueDatabaseActorFixup
{
    private static bool hasAppliedFix;
    private const string LegacyDefaultNpcName = "Butch";
    private const string NewConversationPrefix = "New Conversation";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        hasAppliedFix = false;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryApplyFix();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryApplyFix();
    }

    private static void TryApplyFix()
    {
        if (hasAppliedFix)
        {
            return;
        }

        DialogueDatabase database = DialogueManager.masterDatabase;
        if (database == null || database.actors == null || database.conversations == null)
        {
            return;
        }

        Dictionary<string, Actor> actorByName = new Dictionary<string, Actor>(StringComparer.OrdinalIgnoreCase);
        foreach (Actor actor in database.actors)
        {
            if (actor == null || string.IsNullOrEmpty(actor.Name))
            {
                continue;
            }

            if (!actorByName.ContainsKey(actor.Name))
            {
                actorByName.Add(actor.Name, actor);
            }
        }

        Actor legacyDefaultNpcActor;
        actorByName.TryGetValue(LegacyDefaultNpcName, out legacyDefaultNpcActor);
        int legacyDefaultNpcActorId = legacyDefaultNpcActor != null ? legacyDefaultNpcActor.id : -1;

        int updatedConversations = 0;
        foreach (Conversation conversation in database.conversations)
        {
            if (conversation == null || string.IsNullOrEmpty(conversation.Title))
            {
                continue;
            }

            Actor actor;
            if (!actorByName.TryGetValue(conversation.Title, out actor))
            {
                if (legacyDefaultNpcActorId < 0 ||
                    conversation.ConversantID != legacyDefaultNpcActorId ||
                    conversation.Title.StartsWith(NewConversationPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                actor = CreateActor(database, conversation.Title);
                actorByName[conversation.Title] = actor;
            }

            int incorrectActorId = conversation.ConversantID;
            int expectedActorId = actor.id;
            if (incorrectActorId == expectedActorId)
            {
                continue;
            }

            conversation.ConversantID = expectedActorId;
            if (conversation.ActorID == incorrectActorId)
            {
                conversation.ActorID = expectedActorId;
            }

            if (conversation.dialogueEntries != null)
            {
                foreach (DialogueEntry entry in conversation.dialogueEntries)
                {
                    if (entry == null)
                    {
                        continue;
                    }

                    if (entry.ActorID == incorrectActorId)
                    {
                        entry.ActorID = expectedActorId;
                    }

                    if (entry.ConversantID == incorrectActorId)
                    {
                        entry.ConversantID = expectedActorId;
                    }
                }
            }

            updatedConversations++;
        }

        hasAppliedFix = true;
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (updatedConversations > 0)
        {
            Debug.Log("[DialogueDatabaseActorFixup] Updated actor references for " + updatedConversations + " conversation(s).");
        }
    }

    private static Actor CreateActor(DialogueDatabase database, string actorName)
    {
        Actor actor = new Actor();
        actor.id = GetNextActorId(database);
        actor.fields = new List<Field>();
        actor.Name = actorName;
        actor.IsPlayer = false;
        actor.Description = string.Empty;
        database.actors.Add(actor);
        return actor;
    }

    private static int GetNextActorId(DialogueDatabase database)
    {
        int maxId = 0;
        foreach (Actor actor in database.actors)
        {
            if (actor != null && actor.id > maxId)
            {
                maxId = actor.id;
            }
        }

        return maxId + 1;
    }
}
