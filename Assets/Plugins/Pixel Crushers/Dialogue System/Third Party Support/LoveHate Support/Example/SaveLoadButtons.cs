using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.LoveHate.Example
{

	/// <summary>
	/// Provides click handlers for Dialogue System save and load buttons.
	/// </summary>
	public class SaveLoadButtons : MonoBehaviour {

		public string savedGameKey = "SavedGame";

		public void SaveGame()
		{
			DialogueManager.ShowAlert("Saving Love/Hate data...");
			var saveData = PersistentDataManager.GetSaveData();
			PlayerPrefs.SetString(savedGameKey, saveData);
			Debug.Log("SAVED DATA: " + saveData);
		}

		public void LoadGame()
		{
			if (PlayerPrefs.HasKey(savedGameKey)) 
			{
				var saveData = PlayerPrefs.GetString(savedGameKey);
				Debug.Log("LOADED DATA: " + saveData);
				PersistentDataManager.ApplySaveData(saveData);
				DialogueManager.ShowAlert("Loaded Love/Hate data...");
			}
			else
			{
				DialogueManager.ShowAlert("Save the data first.");
			}
		}

	}

}