using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorageUIManager : SingletonMonobehaviour<StorageUIManager>
{
    private bool _storeMenuOn = false;
    //[SerializeField] private UIInventoryBar uiInventoryBar = null;
    //[SerializeField] private PauseMenuInventoryManagement storeMenuInventoryManagement = null;
    [Tooltip("Typically leave unticked so temporary Dialogue Managers don't unregister your functions.")]
    public bool unregisterOnDisable = false;

    [SerializeField] private GameObject storeMenu = null;
    [SerializeField] private GameObject[] storeTabs = null;
    [SerializeField] private Button[] storeButtons = null;
    [SerializeField] private TMP_Text activeChestNameText = null;
    [SerializeField] private string chestNamePrefix = "Chest: ";
    [SerializeField] private bool forceTwoLineCenteredChestLabel = true;

    private bool currentLabelIncludesPrefix = true;

    public bool StorageMenuOn { get => _storeMenuOn; set => _storeMenuOn = value; }

    protected override void Awake()
    {
        base.Awake();

        storeMenu.SetActive(false);
        ConfigureChestNameText();
    }
    
    private void Update()
    {
        // Toggle pause menu if escape is pressed this a debug key
        if (Input.GetKeyDown(KeyCode.O))
        {
            ToggleStorageMenu();
        }
    }

    public void StoreMenu()
    {
        // Toggle pause menu if escape is pressed
        ToggleStorageMenu();
    }

    private void ToggleStorageMenu()
    {
        if (StorageMenuOn)
        {
            DisabelStorageMenu();
        }
        else
        {
            EnableStorageMenu();
        }
    }


    public void EnableStorageMenu()
    {
        // Destroy any currently dragged items
        //uiInventoryBar.DestroyCurrentlyDraggedItems();

        // Clear currently selected items
        //uiInventoryBar.ClearCurrentlySelectedItems();

        StorageMenuOn = true;
        Player.Instance.PlayerInputIsDisabled = true;
        Time.timeScale = 0;
        storeMenu.SetActive(true);

        // Trigger garbage collector
        System.GC.Collect();

        // Highlight selected button
        HighlightButtonForSelectedTab();

        RefreshActiveChestNameText();
    }

    public void DisabelStorageMenu()
    {
        // Destroy any currently dragged items
        //storeMenuInventoryManagement.DestroyCurrentlyDraggedItems();

        StorageMenuOn = false;
        Player.Instance.PlayerInputIsDisabled = false;
        Time.timeScale = 1;
        storeMenu.SetActive(false);
    }

    private void HighlightButtonForSelectedTab()
    {
        for (int i = 0; i < storeTabs.Length; i++)
        {
            if (storeTabs[i].activeSelf)
            {
                SetButtonColorToActive(storeButtons[i]);
            }

            else
            {
                SetButtonColorToInactive(storeButtons[i]);
            }
        }
    }

    private void SetButtonColorToActive(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = colors.pressedColor;
        button.colors = colors;
    }

    private void SetButtonColorToInactive(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = colors.disabledColor;
        button.colors = colors;
    }

    public void SwitchPauseMenuTab(int tabNum)
    {
        for (int i = 0; i < storeTabs.Length; i++)
        {
            if (i != tabNum)
            {
                storeTabs[i].SetActive(false);
            }
            else
            {
                storeTabs[i].SetActive(true);

            }
        }

        HighlightButtonForSelectedTab();
    }

    public void SetActiveChestName(string chestDisplayName)
    {
        SetActiveChestName(chestDisplayName, true);
    }

    public void SetActiveChestName(string chestDisplayName, bool includePrefix)
    {
        if (activeChestNameText == null)
        {
            return;
        }

        currentLabelIncludesPrefix = includePrefix;

        ConfigureChestNameText(includePrefix);

        if (string.IsNullOrWhiteSpace(chestDisplayName))
        {
            chestDisplayName = "Storage";
        }

        activeChestNameText.text = BuildChestLabel(chestDisplayName, includePrefix);
    }

    private void RefreshActiveChestNameText()
    {
        if (activeChestNameText == null)
        {
            return;
        }

        ConfigureChestNameText(currentLabelIncludesPrefix);

        if (StorageInventoryManager.Instance == null)
        {
            return;
        }

        string activeChestId = StorageInventoryManager.Instance.ActiveChestId;
        if (string.IsNullOrWhiteSpace(activeChestId))
        {
            activeChestNameText.text = BuildChestLabel("Storage", currentLabelIncludesPrefix);
        }
    }

    private string BuildChestLabel(string chestDisplayName, bool includePrefix)
    {
        if (!includePrefix)
        {
            return $"<align=\"center\">{chestDisplayName}</align>";
        }

        if (forceTwoLineCenteredChestLabel)
        {
            return $"<align=\"center\">{chestNamePrefix}\n{chestDisplayName}</align>";
        }

        return $"<align=\"center\">{chestNamePrefix}{chestDisplayName}</align>";
    }

    private void ConfigureChestNameText()
    {
        ConfigureChestNameText(true);
    }

    private void ConfigureChestNameText(bool includePrefix)
    {
        if (activeChestNameText == null)
        {
            return;
        }

        activeChestNameText.richText = true;
        activeChestNameText.alignment = includePrefix ? TextAlignmentOptions.Top : TextAlignmentOptions.Center;
        activeChestNameText.enableWordWrapping = includePrefix;
    }
}