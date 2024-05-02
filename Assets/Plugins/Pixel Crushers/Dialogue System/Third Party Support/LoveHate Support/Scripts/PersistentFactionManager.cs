using UnityEngine;
using PixelCrushers.LoveHate;

namespace PixelCrushers.DialogueSystem.LoveHate
{

	/// <summary>
	/// Persistent faction manager data. Currently only one faction manager is saved
	/// since it uses a single Lua variable name, "FactionDatabase".
	/// 
	/// This component saves factions' parents and relationships.
	/// </summary>
	[AddComponentMenu("Pixel Crushers/Dialogue System/Third Party/Love\u2215Hate/Save System/Persistent Faction Manager")]
	public class PersistentFactionManager : MonoBehaviour
	{

		private const string FactionDatabaseVariableName = "FactionDatabaseData";

		private FactionManager m_factionManager;

		private void Awake()
		{
			m_factionManager = GetComponent<FactionManager>();
			if (m_factionManager == null)
			{
				enabled = false;
				Debug.LogError("Love/Hate; PersistentFactionManager can't find a FactionManager", this);
			}
		}

		public void OnRecordPersistentData()
		{
			if (!enabled || m_factionManager == null) return;
			var data = m_factionManager.SerializeToString();
			DialogueLua.SetVariable(FactionDatabaseVariableName, data);
		}
		
		public void OnApplyPersistentData()
		{
			if (!enabled || m_factionManager == null) return;
			var data = DialogueLua.GetVariable(FactionDatabaseVariableName).AsString;
			m_factionManager.DeserializeFromString(data);
		}

	}

}
