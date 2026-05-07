using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LootChest : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private bool openOnTriggerEnter = false;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Display")]
    [SerializeField] private string chestDisplayName = "";
    [SerializeField] private bool includeChestPrefixInLabel = true;

    [Header("Initial Chest Loot")]
    [SerializeField] private bool addStarterItemsOnFirstOpen = true;
    [SerializeField] private string chestId = "";
    [SerializeField] private InventoryItem[] starterItems;

    private bool playerInRange;
    private bool starterItemsAdded;

    private void Awake()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        if (string.IsNullOrWhiteSpace(chestId))
        {
            chestId = BuildFallbackChestId();
        }

        if (string.IsNullOrWhiteSpace(chestDisplayName))
        {
            chestDisplayName = gameObject.name;
        }
    }

    private void Update()
    {
        if (!playerInRange || openOnTriggerEnter)
        {
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            OpenChest();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;

        if (openOnTriggerEnter)
        {
            OpenChest();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        playerInRange = false;
    }

    private void OpenChest()
    {
        if (StorageUIManager.Instance == null || StorageInventoryManager.Instance == null)
        {
            Debug.LogWarning("LootChest could not open because Storage managers are missing in the scene.");
            return;
        }

        StorageInventoryManager.Instance.SetActiveChest(chestId);
        StorageUIManager.Instance.SetActiveChestName(GetDisplayNameWithoutPrefix(chestDisplayName), includeChestPrefixInLabel);

        if (addStarterItemsOnFirstOpen)
        {
            starterItemsAdded = StorageInventoryManager.Instance.IsChestInitialized(chestId);
        }

        if (addStarterItemsOnFirstOpen && !starterItemsAdded)
        {
            AddStarterItemsToStorage();
            starterItemsAdded = true;
            StorageInventoryManager.Instance.MarkChestInitialized(chestId);
        }

        if (!StorageUIManager.Instance.StorageMenuOn)
        {
            StorageUIManager.Instance.EnableStorageMenu();
        }
    }

    private void AddStarterItemsToStorage()
    {
        foreach (InventoryItem item in starterItems)
        {
            if (item.itemCode <= 0 || item.itemQuantity <= 0)
            {
                continue;
            }

            for (int i = 0; i < item.itemQuantity; i++)
            {
                StorageInventoryManager.Instance.AddItem(InventoryLocation.storage, item.itemCode);
            }
        }
    }

    private string BuildFallbackChestId()
    {
        Vector3 position = transform.position;
        int px = Mathf.RoundToInt(position.x * 100f);
        int py = Mathf.RoundToInt(position.y * 100f);
        int pz = Mathf.RoundToInt(position.z * 100f);
        return gameObject.scene.name + "_" + gameObject.name + "_" + px + "_" + py + "_" + pz;
    }

    private string GetDisplayNameWithoutPrefix(string rawDisplayName)
    {
        if (string.IsNullOrWhiteSpace(rawDisplayName))
        {
            return "Storage";
        }

        string trimmedDisplayName = rawDisplayName.Trim();

        if (trimmedDisplayName.StartsWith("Chest:", System.StringComparison.OrdinalIgnoreCase))
        {
            return trimmedDisplayName.Substring("Chest:".Length).Trim();
        }

        if (trimmedDisplayName.StartsWith("Chest ", System.StringComparison.OrdinalIgnoreCase))
        {
            return trimmedDisplayName.Substring("Chest ".Length).Trim();
        }

        return trimmedDisplayName;
    }
}
