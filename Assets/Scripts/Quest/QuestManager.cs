using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.QuestMachine;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;


public class QuestManager : MonoBehaviour
{
    [Header("Quest Objects")]
    public List<QuestObject> questObjects = new List<QuestObject>();

    [Header("Door Object")]
    public GameObject doorToOpen;

    private int correctPlacements = 0;
    public int requiredCorrectPlacements = 3; // how many are needed to complete the puzzle
    public Light2D spiralLight; // reference to the light that will increase in intensity
    public float lightIncreaseRate = 0.1f; // rate at which the light intensity increases
                                           //need an array of Light2D objects to turn off  
    public Light2D[] lightsToTurnOff; // array of lights to turn off when the quest is complete
    private void Start()
    {
        if (spiralLight == null)
        {
            //I need to fidn the spiral by light by name of object
            spiralLight = GameObject.Find("SpiralLight").GetComponent<Light2D>();
            if (spiralLight == null)
            {
                Debug.LogError("Spiral light not found in the scene. Please assign it in the QuestManager.");
            }
        }

    }
    public void RegisterCorrectPlacement(int amount)
    {
        correctPlacements += amount;

        Debug.Log("Correct placements: " + correctPlacements);

        if (correctPlacements >= requiredCorrectPlacements)
        {
            CompleteQuest();
        }
    }

    private void CompleteQuest()
    {
        Debug.Log("Puzzle complete! Opening door.");
        //I need to fidn the spiral by light by name of object
        spiralLight = GameObject.Find("SpiralLight").GetComponent<Light2D>();
        if (spiralLight == null)
        {
            Debug.LogError("Spiral light not found in the scene. Please assign it in the QuestManager.");
        }

        if (spiralLight != null)
        {
            Debug.Log("Spiral light found, starting intensity increase.");
            StartCoroutine(IncreaseLightIntensity());
            //doorToOpen.SetActive(false); // could also play animation or sound
        }
        else
        {
            Debug.LogWarning("Spiral light not assigned in the QuestManager.");
        }
    }
    private IEnumerator IncreaseLightIntensity()
    {
        while (spiralLight.intensity < 5f)
        {
            spiralLight.intensity += lightIncreaseRate * Time.deltaTime;
            yield return null; // wait for the next frame
        }
        spiralLight.intensity = 5f; // ensure it doesn't exceed the maximum intensity

        // Fade to black and wait for it to finish
        yield return StartCoroutine(SceneControllerManager.Instance.FadeToBlackCoroutine());
        //after fading to black wait a second before continuing 
        yield return new WaitForSeconds(1f);
        spiralLight.intensity = 0f; // reset the light intensity
                                    // Find all Item components under the parent and destroy those with itemCode 10028
        Transform itemsParent = GameObject.FindGameObjectWithTag("ItemsParentTransform").transform;
        Item[] items = itemsParent.GetComponentsInChildren<Item>(true);
        foreach (Item item in items)
        {
            if (item.ItemCode == 10028)
            {
                Destroy(item.gameObject);
            }
        }
        //Make structure appear
        GameObject structure = GameObject.Find("InstantiateQuestItem");
        
        if (structure != null)
        {
            // Instantiate the quest item
            structure.GetComponent<InstantiateQuestItem>().InstantiateObject();
            QuestCompleted("Door", "Open"); // Send a message to the Quest Machine indicating the quest is completed
            // Find the lights to turn off by tag (including only active ones)
            GameObject[] lights = GameObject.FindGameObjectsWithTag("LightToTurnOff");
            lightsToTurnOff = new Light2D[lights.Length];
            for (int i = 0; i < lights.Length; i++)
            {
                lightsToTurnOff[i] = lights[i].GetComponent<Light2D>();
                if (lightsToTurnOff[i] == null)
                {
                    Debug.LogWarning("Light2D component not found on " + lights[i].name);
                }
            }

            foreach (Light2D light in lightsToTurnOff)
            {
                if (light != null)
                {
                    light.enabled = false; // Turn off the light
                }
                else
                {
                    Debug.LogWarning("Light2D component is null in lightsToTurnOff array.");
                }
            }
        }
        else
        {
            Debug.LogError("Structure not found in the scene. Please ensure it is present.");
        }
        // Fade from black and wait for it to finish
        yield return StartCoroutine(SceneControllerManager.Instance.FadeFromBlackCoroutine());
    }

    public void QuestCompleted(string message, string parameter)
    {
        // This method is called when a quest is completed
        // You can add additional logic here if needed
        Debug.Log($"Quest '{message}' completed with status: {parameter}");
        // Send a message to the Quest Machine indicating the quest is completed
        MessageSystem.SendMessage(null, message, parameter);
        //MessageSystem.SendMessage(null, parameter, questStatus, "Success");
        Debug.Log($"Quest '{message}' completed.");
        // You can add additional logic here if needed
    }

    public void StartQuest(string questName, string parameter)
    {
        // This method is called to start a quest
        Debug.Log($"Starting quest: {questName}");
        // You can add additional logic here if needed
        MessageSystem.SendMessage(null, "StartQuest", questName);
    }
}

