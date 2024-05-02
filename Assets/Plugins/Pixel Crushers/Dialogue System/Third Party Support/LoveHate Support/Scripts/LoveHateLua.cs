using UnityEngine;
using PixelCrushers.LoveHate;

namespace PixelCrushers.DialogueSystem.LoveHate
{
	
	/// <summary>
	/// This static class provides Lua functions to access Love/Hate.
	/// You must call RegisterLuaFunctions() at least once before using
	/// the Lua functions.
	/// </summary>
	[AddComponentMenu("Pixel Crushers/Dialogue System/Third Party/Love\u2215Hate/LoveHateLua (on Dialogue Manager)")]
	public class LoveHateLua : MonoBehaviour 
	{

		private void Start()
		{
			RegisterLuaFunctions();
		}

		private static bool hasRegisteredLuaFunctions = false;

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
			hasRegisteredLuaFunctions = false;
		}
#endif

        /// <summary>
        /// Registers the Love/Hate Lua functions with the Dialogue System.
        /// </summary>
        public static void RegisterLuaFunctions() 
		{
			if (hasRegisteredLuaFunctions) return;
			hasRegisteredLuaFunctions = true;
			Lua.RegisterFunction("GetFactionName", null, SymbolExtensions.GetMethodInfo(() => GetFactionName(string.Empty)));

            Lua.RegisterFunction("GetPersonalityTrait", null, SymbolExtensions.GetMethodInfo(() => GetPersonalityTrait(string.Empty, string.Empty)));
            Lua.RegisterFunction("SetPersonalityTrait", null, SymbolExtensions.GetMethodInfo(() => SetPersonalityTrait(string.Empty, string.Empty, (double)0)));

            Lua.RegisterFunction("GetAffinity", null, SymbolExtensions.GetMethodInfo(() => GetAffinity(string.Empty, string.Empty)));
			Lua.RegisterFunction("SetAffinity", null, SymbolExtensions.GetMethodInfo(() => SetAffinity(string.Empty, string.Empty, (double) 0)));
			Lua.RegisterFunction("ModifyAffinity", null, SymbolExtensions.GetMethodInfo(() => ModifyAffinity(string.Empty, string.Empty, (double) 0)));

            Lua.RegisterFunction("GetRelationshipTrait", null, SymbolExtensions.GetMethodInfo(() => GetRelationshipTrait(string.Empty, string.Empty, string.Empty)));
			Lua.RegisterFunction("SetRelationshipTrait", null, SymbolExtensions.GetMethodInfo(() => SetRelationshipTrait(string.Empty, string.Empty, string.Empty, (double) 0)));
			Lua.RegisterFunction("ModifyRelationshipTrait", null, SymbolExtensions.GetMethodInfo(() => ModifyRelationshipTrait(string.Empty, string.Empty, string.Empty, (double) 0)));

            Lua.RegisterFunction("SetRelationshipInheritability", null, SymbolExtensions.GetMethodInfo(() => SetRelationshipInheritable(string.Empty, string.Empty, true)));

            Lua.RegisterFunction("GetHappiness", null, SymbolExtensions.GetMethodInfo(() => GetHappiness(string.Empty)));
			Lua.RegisterFunction("GetPleasure", null, SymbolExtensions.GetMethodInfo(() => GetPleasure(string.Empty)));
			Lua.RegisterFunction("GetArousal", null, SymbolExtensions.GetMethodInfo(() => GetArousal(string.Empty)));
			Lua.RegisterFunction("GetDominance", null, SymbolExtensions.GetMethodInfo(() => GetDominance(string.Empty)));
			Lua.RegisterFunction("ModifyPAD", null, SymbolExtensions.GetMethodInfo(() => ModifyPAD(string.Empty, (double) 0, (double) 0, (double) 0, (double) 0)));
            Lua.RegisterFunction("GetTemperament", null, SymbolExtensions.GetMethodInfo(() => GetTemperament(string.Empty)));
            Lua.RegisterFunction("GetEmotionalState", null, SymbolExtensions.GetMethodInfo(() => GetEmotionalState(string.Empty)));

            Lua.RegisterFunction("KnowsDeed", null, SymbolExtensions.GetMethodInfo(() => KnowsDeed(string.Empty, string.Empty, string.Empty, string.Empty)));
			Lua.RegisterFunction("ReportDeed", null, SymbolExtensions.GetMethodInfo(() => ReportDeed(string.Empty, string.Empty, string.Empty)));
			Lua.RegisterFunction("ShareRumors", null, SymbolExtensions.GetMethodInfo(() => ShareRumors(string.Empty, string.Empty)));
		}

		private static GameObject FindActor(string actorName)
		{
            // If actor name is blank, find player:
            if (string.IsNullOrEmpty(actorName)) return GameObject.FindGameObjectWithTag("Player");

            // Otherwise check if the actor has a DialogueActor registered with the Dialogue System:
            var registeredActor = CharacterInfo.GetRegisteredActorTransform(actorName);
            if (registeredActor != null) return registeredActor.gameObject;

            // Otherwise find GameObject whose name matches:
            var go = GameObject.Find(actorName);
            if (go != null) return go;

            // Otherwise check FactionMember components for a member whose faction matches the actor name:
            if (FactionManager.instance != null)
            {
                var faction = FactionManager.instance.GetFaction(actorName);
                if (FactionManager.instance.members.ContainsKey(faction) && FactionManager.instance.members[faction].Count > 0)
                {
                    return FactionManager.instance.members[faction][0].gameObject;
                }
            }

            return null;
		}

		private static FactionMember FindFactionMember(string actorName)
		{
			var actor = FindActor(actorName);
			return (actor == null) ? null : actor.GetComponentInChildren<FactionMember>();
		}
		
		private static FactionManager FindFactionManager(string actorName)
		{
			var factionMember = FindFactionMember(actorName);
			var factionManager = (factionMember == null) ? null : factionMember.factionManager;
			return factionManager ?? FindObjectOfType<FactionManager>();
		}

		public static string GetFactionName(string actorName)
		{
			var factionMember = FindFactionMember(actorName);
			return (factionMember == null || factionMember.faction == null) ? actorName : factionMember.faction.name;
		}

        public static double GetPersonalityTrait(string factionName, string traitName)
        {
            var factionManager = FindFactionManager(factionName);
            if ((factionManager == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction manager through " + factionName);
            return (factionManager == null) ? 0 : factionManager.factionDatabase.GetPersonalityTrait(GetFactionName(factionName), factionManager.factionDatabase.GetPersonalityTraitID(traitName));
        }

        public static void SetPersonalityTrait(string factionName, string traitName, double value)
        {
            var factionManager = FindFactionManager(factionName);
            if ((factionManager == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction manager through " + factionName);
            if (factionManager == null) return;
            factionManager.factionDatabase.SetPersonalityTrait(GetFactionName(factionName), factionManager.factionDatabase.GetPersonalityTraitID(traitName), (float)value);
        }

        public static double GetAffinity(string judgeName, string subjectName)
		{
			var factionManager = FindFactionManager(judgeName);
			if ((factionManager == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction manager through " + judgeName);
			return (factionManager == null) ? 0 : factionManager.GetAffinity(GetFactionName(judgeName), GetFactionName(subjectName));
		}
		
		public static void SetAffinity(string judgeName, string subjectName, double affinity) 
		{
			var factionManager = FindFactionManager(judgeName);
			if ((factionManager == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction manager through " + judgeName);
			if (factionManager == null) return;
			factionManager.SetPersonalAffinity(GetFactionName(judgeName), GetFactionName(subjectName), (float) affinity);
		}
		
		public static void ModifyAffinity(string judgeName, string subjectName, double affinity)
		{
			var factionManager = FindFactionManager(judgeName);
			if ((factionManager == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction manager through " + judgeName);
			if (factionManager == null) return;
			factionManager.ModifyPersonalAffinity(GetFactionName(judgeName), GetFactionName(subjectName), (float) affinity);
		}

		public static double GetRelationshipTrait(string judgeName, string subjectName, string traitName)
		{
			var factionManager = FindFactionManager(judgeName);
			if ((factionManager == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction manager through " + judgeName);
			return (factionManager == null) ? 0 : factionManager.factionDatabase.GetRelationshipTrait(GetFactionName(judgeName), GetFactionName(subjectName), factionManager.factionDatabase.GetRelationshipTraitID(traitName));
		}
		
		public static void SetRelationshipTrait(string judgeName, string subjectName, string traitName, double value) 
		{
			var factionManager = FindFactionManager(judgeName);
			if ((factionManager == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction manager through " + judgeName);
			if (factionManager == null) return;
			factionManager.factionDatabase.SetPersonalRelationshipTrait(GetFactionName(judgeName), GetFactionName(subjectName), factionManager.factionDatabase.GetRelationshipTraitID(traitName), (float) value);
		}
		
		public static void ModifyRelationshipTrait(string judgeName, string subjectName, string traitName, double value)
		{
			var factionManager = FindFactionManager(judgeName);
			if ((factionManager == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction manager through " + judgeName);
			if (factionManager == null) return;
			factionManager.factionDatabase.ModifyPersonalRelationshipTrait(GetFactionName(judgeName), GetFactionName(subjectName), factionManager.factionDatabase.GetRelationshipTraitID(traitName), (float) value);
		}

        public static void SetRelationshipInheritable(string judgeName, string subjectName, bool inheritable)
        {
            var factionManager = FindFactionManager(judgeName);
            if ((factionManager == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction manager through " + judgeName);
            if (factionManager == null) return;
            factionManager.factionDatabase.SetPersonalRelationshipInheritable(GetFactionName(judgeName), GetFactionName(subjectName), inheritable);
        }

        public static double GetHappiness(string actorName)
		{
			var factionMember = FindFactionMember(actorName);
			if ((factionMember == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction member on " + actorName);
			return (factionMember == null || factionMember.pad == null) ? 0 : factionMember.pad.happiness;
		}
		
		public static double GetPleasure(string actorName)
		{
			var factionMember = FindFactionMember(actorName);
			return (factionMember == null || factionMember.pad == null) ? 0 : factionMember.pad.pleasure;
		}
		
		public static double GetArousal(string actorName)
		{
			var factionMember = FindFactionMember(actorName);
			if ((factionMember == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction member on " + actorName);
			return (factionMember == null || factionMember.pad == null) ? 0 : factionMember.pad.arousal;
		}
		
		public static double GetDominance(string actorName)
		{
			var factionMember = FindFactionMember(actorName);
			if ((factionMember == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction member on " + actorName);
			return (factionMember == null || factionMember.pad == null) ? 0 : factionMember.pad.dominance;
		}
		
		public static void ModifyPAD(string actorName, double happinessChange, double pleasureChange,
		                             double arousalChange, double dominanceChange)
		{
			var factionMember = FindFactionMember(actorName);
			if (factionMember == null) 
			{
				if ((factionMember == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction member on " + actorName);
				return;
			}
			factionMember.ModifyPAD((float) happinessChange, (float) pleasureChange, (float) arousalChange, (float) dominanceChange);
		}

        public static string GetTemperament(string actorName)
        {
            var factionMember = FindFactionMember(actorName);
            if ((factionMember == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction member on " + actorName);
            return (factionMember == null || factionMember.pad == null) ? "unknown" : factionMember.pad.GetTemperament().ToString();
        }

        public static string GetEmotionalState(string actorName)
        {
            var factionMember = FindFactionMember(actorName);
            var emotionalState = (factionMember != null) ? factionMember.GetComponent<EmotionalState>() : null;
            if ((emotionalState == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find emotional state on " + actorName);
            return (emotionalState == null) ? "unknown" : emotionalState.GetCurrentEmotionName();
        }

        public static bool KnowsDeed(string checkName, string actorName, string targetName, string deedTag)
		{
			var factionMember = FindFactionMember(checkName);
			if ((factionMember == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction member on " + checkName);
			var actorFactionID = (factionMember == null || factionMember.factionManager == null) ? -1 : factionMember.factionManager.GetFactionID(GetFactionName(actorName));
			var targetFactionID = (factionMember == null || factionMember.factionManager == null) ? -1 : factionMember.factionManager.GetFactionID(GetFactionName(targetName));
			return (factionMember == null || factionMember.factionManager == null) ? false : factionMember.KnowsAboutDeed(actorFactionID, targetFactionID, deedTag);
		}
		
		public static void ReportDeed(string actorName, string targetName, string deedTag)
		{
			var factionMember = FindFactionMember(actorName);
			var deedReporter = (factionMember == null) ? null : factionMember.GetComponentInChildren<DeedReporter>();
			if ((deedReporter == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction member or deed reporter on " + actorName);
			if (deedReporter == null) return;
            if (!string.IsNullOrEmpty(targetName) && targetName.StartsWith("faction=", System.StringComparison.OrdinalIgnoreCase))
            {
                // Report to all faction members in specified faction:
                targetName = targetName.Substring("faction=".Length).Trim();
                var faction = FactionManager.instance.GetFaction(targetName);
                if (faction != null && FactionManager.instance.members.ContainsKey(faction))
                {
                    foreach (var member in FactionManager.instance.members[faction])
                    {
                        deedReporter.ReportDeed(deedTag, member);
                    }
                }
            }
            else
            {
                // Report to one faction member:
                deedReporter.ReportDeed(deedTag, FindFactionMember(targetName));
            }
		}
		
		public static void ShareRumors(string actorName, string targetName)
		{
			var factionMember = FindFactionMember(actorName);
			if ((factionMember == null) && DialogueDebug.LogWarnings) Debug.LogWarning("Dialogue System: Can't find faction member on " + actorName);
			if (factionMember == null) return;
			factionMember.ShareRumors(FindFactionMember(targetName));
		}
		
	}
	
}
