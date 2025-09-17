using System.Collections;
using UnityEngine;

public class Crop : MonoBehaviour
{
    private int cumulativeDamage = 0;

    [Tooltip("This should be populated from child transform gameobject showing harvest effect spawn point")]
    [SerializeField] private Transform harvestActionEffectTransform = null;

    [Tooltip("This should be populated from child gameobject")]
    [SerializeField] private SpriteRenderer cropHarvestedSpriteRenderer = null;

    [HideInInspector]
    public Vector2Int cropGridPosition;

    public void ProcessToolAction(ItemDetails equippedItemDetails, bool isToolRight, bool isToolLeft, bool isToolDown, bool isToolUp)
    {
        Debug.Log("ProcessToolAction called");

        // Get grid property details
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y);

        if (gridPropertyDetails == null)
            return;

        // Get seed item details
        ItemDetails seedItemDetails = InventoryManager.Instance.GetItemDetails(gridPropertyDetails.seedItemCode);
        if (seedItemDetails == null)
            return;

        // Get crop details
        CropDetails cropDetails = GridPropertiesManager.Instance.GetCropDetails(seedItemDetails.itemCode);
        if (cropDetails == null)
            return;

        // Get the required harvest actions for the tool
        int requiredHarvestActions = cropDetails.RequiredHarvestActionsForTool(equippedItemDetails.itemCode);
        if (requiredHarvestActions == -1)
            return; // this tool can't be used to harvest this crop

        // Add the tool's damage to the cumulative damage
        cumulativeDamage += equippedItemDetails.damageAmount;

        // Trigger tool animation
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            if (isToolRight || isToolUp)
            {
                animator.SetTrigger("usetoolright");
            }
            else if (isToolLeft || isToolDown)
            {
                animator.SetTrigger("usetoolleft");
            }
        }

        // Trigger tool particle effect on crop
        if (cropDetails.isHarvestActionEffect)
        {
            EventHandler.CallHarvestActionEffectEvent(harvestActionEffectTransform.position, cropDetails.harvestActionEffect);
        }

        // Check if cumulative damage meets or exceeds required harvest actions
        //always set the number at least to 1 so that even a tool with 0 damage can harvest
        if (cumulativeDamage < 1)
            cumulativeDamage = 1;
        if (cumulativeDamage >= requiredHarvestActions)
            HarvestCrop(isToolRight, isToolUp, cropDetails, gridPropertyDetails, animator);
    }

    private void HarvestCrop(bool isUsingToolRight, bool isUsingToolUp, CropDetails cropDetails, GridPropertyDetails gridPropertyDetails, Animator animator)
    {
        Debug.Log($"HarvestCrop called. Animator is {(animator == null ? "NULL" : "NOT NULL")}");
        // Is there a harvested animation
        if (cropDetails.isHarvestedAnimation && animator != null)
        {
            Debug.Log("Harvested animation should play.");
            // If harvest sprite then add to sprite renderer
            if (cropDetails.harvestedSprite != null)
            {
                if (cropHarvestedSpriteRenderer != null)
                {
                    cropHarvestedSpriteRenderer.sprite = cropDetails.harvestedSprite;
                }
            }

            if (isUsingToolRight || isUsingToolUp)
            {
                animator.SetTrigger("harvestright");
            }
            else
            {
                animator.SetTrigger("harvestleft");
            }
        }

        // Is there a harvested sound
        if (cropDetails.harvestSound != SoundName.none)
        {
            AudioManager.Instance.PlaySound(cropDetails.harvestSound);
        }

        // Delete crop from grid properties
        gridPropertyDetails.seedItemCode = -1;
        gridPropertyDetails.growthDays = -1;
        gridPropertyDetails.daysSinceLastHarvest = -1;
        gridPropertyDetails.daysSinceWatered = -1;

        // Should the crop be hidden before the harvested animation
        if (cropDetails.hideCropBeforeHarvestedAnimation)
        {
            GetComponentInChildren<SpriteRenderer>().enabled = false;
        }

        // Should box colliders be disabled before harvest
        if (cropDetails.disableCropCollidersBeforeHarvestedAnimation)
        {
            // Disable any box colliders
            Collider2D[] collider2Ds = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D collider2D in collider2Ds)
            {
                collider2D.enabled = false;
            }
        }

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // Is there a harvested animation - Destroy this crop game object after animation completed
        if (cropDetails.isHarvestedAnimation && animator != null)
        {
            Debug.Log("Starting coroutine to wait for harvested animation state.");
            StartCoroutine(ProcessHarvestActionsAfterAnimation(cropDetails, gridPropertyDetails, animator));
        }
        else
        {
            HarvestActions(cropDetails, gridPropertyDetails);
        }
    }

    private IEnumerator ProcessHarvestActionsAfterAnimation(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails, Animator animator)
    {
        Debug.Log("ProcessHarvestActionsAfterAnimation coroutine started.");
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Harvested"))
        {
            Debug.Log("Waiting for 'Harvested' animation state...");
            yield return null;
        }
        Debug.Log("'Harvested' animation state reached. Proceeding to harvest actions.");
        HarvestActions(cropDetails, gridPropertyDetails);

    }

    private void HarvestActions(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        Debug.Log("HarvestActions called");
        SpawnHarvestedItems(cropDetails);
        if (cropDetails.isActionItem && cropDetails.actionItemPrefab != null)
        {
            //instantiate action at random position around the crop position
            Vector3 spawnPosition = new Vector3(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f), 0f);
            Instantiate(cropDetails.actionItemPrefab, spawnPosition, Quaternion.identity);
        }

        // Does this crop transform into another crop
        if (cropDetails.harvestedTransformItemCode > 0)
        {
            CreateHarvestedTransformCrop(cropDetails, gridPropertyDetails);
        }

        Destroy(gameObject);
    }

    private void SpawnHarvestedItems(CropDetails cropDetails)
    {
        // Spawn the item(s) to be produced
        for (int i = 0; i < cropDetails.cropProducedItemCode.Length; i++)
        {
            int cropsToProduce;

            // Calculate how many crops to produce
            if (cropDetails.cropProducedMinQuantity[i] == cropDetails.cropProducedMaxQuantity[i] ||
                cropDetails.cropProducedMaxQuantity[i] < cropDetails.cropProducedMinQuantity[i])
            {
                cropsToProduce = cropDetails.cropProducedMinQuantity[i];
            }
            else
            {
                cropsToProduce = Random.Range(cropDetails.cropProducedMinQuantity[i], cropDetails.cropProducedMaxQuantity[i] + 1);
            }

            for (int j = 0; j < cropsToProduce; j++)
            {
                Vector3 spawnPosition;
                if (cropDetails.spawnCropProducedAtPlayerPosition)
                {
                    Debug.Log($"InventoryManager.Instance is {(InventoryManager.Instance == null ? "NULL" : "NOT NULL")}");
                    Debug.Log($"About to add item {cropDetails.cropProducedItemCode[i]} to player inventory");
                    InventoryManager.Instance.AddItem(InventoryLocation.player, cropDetails.cropProducedItemCode[i]);
                }
                else
                {
                    // Random position
                    spawnPosition = new Vector3(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f), 0f);
                    SceneItemsManager.Instance.InstantiateSceneItem(cropDetails.cropProducedItemCode[i], spawnPosition);
                }
            }
        }
    }

    private void CreateHarvestedTransformCrop(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        // Update crop in grid properties
        gridPropertyDetails.seedItemCode = cropDetails.harvestedTransformItemCode;
        gridPropertyDetails.growthDays = 0;
        gridPropertyDetails.daysSinceLastHarvest = -1;
        gridPropertyDetails.daysSinceWatered = -1;

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // Display planted crop
        GridPropertiesManager.Instance.DisplayPlantedCrop(gridPropertyDetails);
    }
}




