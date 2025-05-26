using System.Collections;
using System.Collections.Generic;
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
        Debug.Log("Spiral light intensity reached maximum.");
    }
}

