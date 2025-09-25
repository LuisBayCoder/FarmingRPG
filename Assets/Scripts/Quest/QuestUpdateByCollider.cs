using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestUpdateByCollider : MonoBehaviour
{
    [SerializeField] private string questName = "QuestName"; // Name of the quest to be updated
    [SerializeField] private string questDescription = "QuestDescription"; // Description of the quest to be updated
    // Start is called before the first frame update

    void SendQuestMessage(string messageArgs, string locationName)
    {
       // Use the Singleton instance to call the method
    QuestSaveManager.Instance.SaveCompletedQuest(messageArgs, locationName);

    // Debug if the quest was saved
    Debug.Log($"Quest '{messageArgs}' saved at location '{locationName}'.");
    }

    //Send a message if player collides with 2D collider quest object
    //
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            string messageArgs = questName;
            string locationName = questDescription; // Get the name of the GameObject that has this script attached
            SendQuestMessage(messageArgs, locationName);
        }
    } 
}
