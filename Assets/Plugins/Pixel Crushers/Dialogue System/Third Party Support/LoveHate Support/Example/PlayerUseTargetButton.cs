using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.LoveHate.Example
{

	/// <summary>
	/// Provides a click handler that sends "OnUse" to the example deed toolbar's target.
	/// </summary>
	public class PlayerUseTargetButton : MonoBehaviour {

		private RudimentaryPlayerController2D m_player = null;

		private void Awake()
		{
			m_player = GetComponent<RudimentaryPlayerController2D>();
		}

		public void OnClick()
		{
			if (m_player == null) return;
			m_player.currentTarget.SendMessage("OnUse", transform, SendMessageOptions.DontRequireReceiver);
		}

	}

}