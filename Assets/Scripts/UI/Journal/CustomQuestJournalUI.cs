using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using PixelCrushers.QuestMachine; 

public class CustomQuestJournalUI : MonoBehaviour
{
    [Header("Quest Source")]
    [SerializeField] private QuestJournal playerJournal; 

    [Header("Your Custom UI Elements")]
    // This list maps directly to the elements you drag into the Inspector
    [SerializeField] private List<Image> tabImages; 
    private readonly List<Sprite> defaultTabSprites = new List<Sprite>();

    private void Awake()
    {
        CacheDefaultTabSprites();
    }

    void OnEnable()
    {
        // Automatically runs every single time the Tab4Journal panel is turned ON
        UpdateCustomTabs();
    }

    public void UpdateCustomTabs()
    {
        // If you haven't assigned the player journal manually, find it automatically
        if (playerJournal == null)
        {
            playerJournal = QuestMachine.GetQuestJournal(); 
        }

        if (playerJournal == null || tabImages == null || tabImages.Count == 0) return;
        CacheDefaultTabSprites();

        // Get all quests currently assigned to the player
        List<Quest> activeQuests = playerJournal.questList;

        // Loop through your UI slots
        for (int i = 0; i < tabImages.Count; i++)
        {
            if (i < activeQuests.Count)
            {
                Quest currentQuest = activeQuests[i];

                // Ensure the quest has a valid icon setup in Quest Machine
                if (currentQuest.icon != null)
                {
                    tabImages[i].sprite = currentQuest.icon;
                    tabImages[i].gameObject.SetActive(true); 
                }
                else
                {
                    tabImages[i].sprite = i < defaultTabSprites.Count ? defaultTabSprites[i] : null;
                    tabImages[i].gameObject.SetActive(true); 
                }
            }
            else
            {
                // Hide any extra UI buttons/slots if the player has fewer quests than slots
                tabImages[i].gameObject.SetActive(false); 
            }
        }
    }

    private void CacheDefaultTabSprites()
    {
        if (tabImages == null) return;
        while (defaultTabSprites.Count < tabImages.Count)
        {
            var image = tabImages[defaultTabSprites.Count];
            defaultTabSprites.Add(image != null ? image.sprite : null);
        }
    }
}


