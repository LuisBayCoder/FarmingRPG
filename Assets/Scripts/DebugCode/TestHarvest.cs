using UnityEngine;

public class TestHarvest : MonoBehaviour
{
    public SO_CropDetailsList cropDetailsList;
    public SO_ItemList itemList;
    public int seedItemCode = 1; // Example seed item code
    public int toolItemCode = 10013; // Example tool item code

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Press Space to perform a harvest action
        {
            CropDetails crop = cropDetailsList.GetCropDetails(seedItemCode);
            if (crop != null)
            {
                Debug.Log($"Trying to use tool with item code {toolItemCode} on crop with seed item code {seedItemCode}");
                bool canUseTool = crop.CanUseToolToHarvestCrop(toolItemCode);
                Debug.Log(canUseTool ? "Tool can be used to harvest the crop." : "Tool cannot be used to harvest the crop.");

                if (canUseTool)
                {
                    bool harvested = crop.PerformHarvestAction(toolItemCode);
                    Debug.Log(harvested ? "Crop harvested!" : "Keep hitting!");
                }
            }
            else
            {
                Debug.Log("No crop found with the given seed item code.");
            }
        }

        if (Input.GetKeyDown(KeyCode.T)) // Press T to cycle through tools for testing
        {
            toolItemCode = (toolItemCode == 10013) ? 10017 : 10013;
            Debug.Log($"Switched to tool with item code {toolItemCode}");
        }
    }
}



