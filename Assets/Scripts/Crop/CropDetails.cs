using UnityEngine;

[System.Serializable]
public class CropDetails
{
    [ItemCodeDescription]
    public int seedItemCode;
    public int[] growthDays;
    public GameObject[] growthPrefab;
    public Sprite[] growthSprite;
    public Season[] seasons;
    public Sprite harvestedSprite;
    [ItemCodeDescription]
    public int harvestedTransformItemCode;
    public bool hideCropBeforeHarvestedAnimation;
    public bool disableCropCollidersBeforeHarvestedAnimation;
    public bool isHarvestedAnimation;
    public bool isHarvestActionEffect = false;
    public bool spawnCropProducedAtPlayerPosition;
    public HarvestActionEffect harvestActionEffect;
    public SoundName harvestSound;

    [ItemCodeDescription]
    public int[] harvestToolItemCode;
    public int[] requiredHarvestActions;
    [ItemCodeDescription]
    public int[] cropProducedItemCode;
    public int[] cropProducedMinQuantity;
    public int[] cropProducedMaxQuantity;
    public int daysToRegrow;

    private int totalHarvestActions = 0;

    public void ValidateArrays()
    {
        if (harvestToolItemCode.Length != requiredHarvestActions.Length)
        {
            Debug.LogError($"Mismatch in array lengths: harvestToolItemCode ({harvestToolItemCode.Length}), requiredHarvestActions ({requiredHarvestActions.Length}). Fixing...");

            int maxLength = Mathf.Max(harvestToolItemCode.Length, requiredHarvestActions.Length);
            System.Array.Resize(ref harvestToolItemCode, maxLength);
            System.Array.Resize(ref requiredHarvestActions, maxLength);
        }
    }

    public bool CanUseToolToHarvestCrop(int toolItemCode)
    {
        int requiredActions = RequiredHarvestActionsForTool(toolItemCode);
        Debug.Log($"Tool {toolItemCode} - Required Actions: {requiredActions}");
        return requiredActions != -1;
    }

    public int RequiredHarvestActionsForTool(int toolItemCode)
    {
        if (harvestToolItemCode.Length != requiredHarvestActions.Length)
        {
            Debug.LogError("harvestToolItemCode and requiredHarvestActions arrays are not of the same length!");
            return -1;
        }

        for (int i = 0; i < harvestToolItemCode.Length; i++)
        {
            if (harvestToolItemCode[i] == toolItemCode)
            {
                return requiredHarvestActions[i];
            }
        }
        return -1;
    }

    public bool PerformHarvestAction(int toolItemCode)
    {
        if (!CanUseToolToHarvestCrop(toolItemCode)) return false;

        totalHarvestActions++;
        int requiredActions = GetMaxRequiredHarvestActions();
        Debug.Log($"Total Actions: {totalHarvestActions}, Required: {requiredActions}");
        if (totalHarvestActions >= requiredActions)
        {
            HarvestCrop();
            return true;
        }

        return false;
    }

    private int GetMaxRequiredHarvestActions()
    {
        int maxRequiredActions = 0;
        for (int i = 0; i < requiredHarvestActions.Length; i++)
        {
            if (requiredHarvestActions[i] > maxRequiredActions)
            {
                maxRequiredActions = requiredHarvestActions[i];
            }
        }
        return maxRequiredActions;
    }

    private void HarvestCrop()
    {
        Debug.Log("Crop harvested!");
        totalHarvestActions = 0;
    }
}




