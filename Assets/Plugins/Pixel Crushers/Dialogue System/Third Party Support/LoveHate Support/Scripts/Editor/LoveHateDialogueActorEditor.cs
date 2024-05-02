// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using PixelCrushers.LoveHate;

namespace PixelCrushers.DialogueSystem.LoveHate

{

    [CustomEditor(typeof(LoveHateDialogueActor), true)]
    [CanEditMultipleObjects]
    public class LoveHateDialogueActorEditor : DialogueActorEditor
    {

        public override void OnInspectorGUI()
        {
            var dialogueActor = target as LoveHateDialogueActor;
            var factionMember = dialogueActor.GetComponent<FactionMember>();
            var actorName = dialogueActor.actor;
            if (factionMember == null)
            {
                EditorGUILayout.HelpBox("FactionMember component is missing.", MessageType.Warning);
                if (GUILayout.Button("Add FactionMember"))
                {
                    dialogueActor.gameObject.AddComponent(TypeUtility.GetWrapperType(typeof(FactionMember)));
                }
            }
            else
            {
                var dialogueDatabase = EditorTools.FindInitialDatabase();
                var factionManager = FindObjectOfType<FactionManager>();
                if (factionManager == null)
                {
                    EditorGUILayout.HelpBox("No Faction Manager found in scene. Can't check that faction and dialogue actor match.", MessageType.Info);
                }
                else if (factionManager.factionDatabase == null)
                {
                    EditorGUILayout.HelpBox("Scene's Faction Manager doesn't have a Faction Database. Can't check that faction and dialogue actor match.", MessageType.Info);
                }
                else if (dialogueDatabase == null)
                {
                    EditorGUILayout.HelpBox("Can't identify the dialogue database. Does the scene have a Dialogue Manager? Can't check that faction and dialogue actor match.", MessageType.Info);
                }
                else
                {
                    var factionDatabase = factionManager.factionDatabase;
                    var faction = factionDatabase.GetFaction(factionMember.factionID);
                    var factionName = (faction != null) ? faction.name : string.Empty;
                    if (string.IsNullOrEmpty(actorName) && string.IsNullOrEmpty(factionName))
                    {
                        EditorGUILayout.HelpBox("Select an actor or a FactionMember faction.", MessageType.Warning);
                    }
                    else if (string.IsNullOrEmpty(actorName))
                    {
                        // If dialogue database has an actor that matches faction, use it:
                        if (dialogueDatabase.actors.Find(x => x.Name == factionName) != null)
                        {
                            serializedObject.Update();
                            serializedObject.FindProperty("actor").stringValue = factionName;
                            serializedObject.ApplyModifiedProperties();
                        }
                        // Otherwise offer to add actor:
                        else
                        {
                            if (GUILayout.Button("Add Actor '" + factionName + "'"))
                            {
                                Undo.RecordObject(dialogueDatabase, "Add Actor");
                                var template = TemplateTools.LoadFromEditorPrefs();
                                var newActor = template.CreateActor(template.GetNextActorID(dialogueDatabase), factionName, false);
                                dialogueDatabase.actors.Add(newActor);
                            }
                        }
                    }
                    else if (string.IsNullOrEmpty(factionName))
                    {
                        // If faction database has faction that matches actor, use it:
                        faction = factionDatabase.GetFaction(actorName);
                        if (faction != null)
                        {
                            Undo.RecordObject(factionMember, "Set Faction");
                            factionMember.factionID = faction.id;
                        }
                        // Otherwise offer to add faction:
                        else
                        {
                            if (GUILayout.Button("Add Faction '" + actorName + "'"))
                            {
                                Undo.RecordObject(factionDatabase, "Add Faction");
                                factionDatabase.CreateNewFaction(actorName, string.Empty);
                            }
                        }
                    }
                    else if (actorName != factionName)
                    {
                        EditorGUILayout.HelpBox("Actor and FactionMember faction do not match.", MessageType.Warning);
                        if (GUILayout.Button("Set Actor to " + factionName))
                        {
                            serializedObject.Update();
                            serializedObject.FindProperty("actor").stringValue = factionName;
                            serializedObject.ApplyModifiedProperties();
                            if (dialogueDatabase.actors.Find(x => x.Name == factionName) == null)
                            {
                                Undo.RecordObject(dialogueDatabase, "Add Actor");
                                var template = TemplateTools.LoadFromEditorPrefs();
                                var newActor = template.CreateActor(template.GetNextActorID(dialogueDatabase), factionName, false);
                                dialogueDatabase.actors.Add(newActor);
                            }
                        }
                        if (GUILayout.Button("Set Faction to " + actorName))
                        {
                            faction = factionDatabase.GetFaction(actorName);
                            if (faction == null)
                            {
                                Undo.RecordObject(factionDatabase, "Add Faction");
                                factionDatabase.CreateNewFaction(actorName, string.Empty);
                                faction = factionDatabase.GetFaction(actorName);
                            }
                            Undo.RecordObject(factionMember, "Set Faction");
                            factionMember.factionID = faction.id;
                        }
                    }
                }
            }

            base.OnInspectorGUI();
        }

    }
}
