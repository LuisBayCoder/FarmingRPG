using UnityEngine;
using PixelCrushers.LoveHate;

namespace PixelCrushers.DialogueSystem.LoveHate
{

	/// <summary>
	/// Persistent faction member data, saved in the Actor table, under the actor's
	/// name, in a field "FactionMemberData".
	/// 
	/// This component saves the faction member's PAD and memories.
	/// </summary>
	[AddComponentMenu("Pixel Crushers/Dialogue System/Third Party/Love\u2215Hate/Save System/Persistent Faction Member")]
	public class PersistentFactionMember : MonoBehaviour
	{
		
		private const string FactionMemberFieldName = "FactionMemberData";
		
		private FactionMember m_factionMember;
		private string m_actorName;
		
		private void Start()
		{
			m_actorName = OverrideActorName.GetActorName(transform);
			m_factionMember = GetComponent<FactionMember>();
			if (m_factionMember == null)
			{
				enabled = false;
				Debug.LogError("Love/Hate; PersistentFactionManager can't find a FactionMember", this);
			}
		}
		
		public void OnRecordPersistentData()
		{
			if (!enabled || m_factionMember == null) return;
			var data = m_factionMember.SerializeToString();
			DialogueLua.SetActorField(m_actorName, FactionMemberFieldName, data);
		}
		
		public void OnApplyPersistentData()
		{
			if (!enabled || m_factionMember == null) return;
			var data = DialogueLua.GetActorField(m_actorName, FactionMemberFieldName).AsString;
			m_factionMember.DeserializeFromString(data);
		}

	}
}
