using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreInventoryManager : SingletonMonobehaviour<StoreInventoryManager>
{

    private bool _storeMenuOn = false;
    //[SerializeField] private UIInventoryBar uiInventoryBar = null;
   // [SerializeField] private PauseMenuInventoryManagement storeMenuInventoryManagement = null;
    [SerializeField] private GameObject storeMenu = null;
    [SerializeField] private GameObject[] storeTabs = null;
    [SerializeField] private Button[] storeButtons = null;

    public bool StoreMenuOn { get => _storeMenuOn; set => _storeMenuOn = value; }

    protected override void Awake()
    {
        base.Awake();

        storeMenu.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        StoreMenu();
    }

    private void StoreMenu()
    {
        // Toggle pause menu if escape is pressed

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (StoreMenuOn)
            {
                DisableStoreMenu();
            }
            else
            {
                EnableStoreMenu();
            }
        }
    }


    private void EnableStoreMenu()
    {
        // Destroy any currently dragged items
//        uiInventoryBar.DestroyCurrentlyDraggedItems();

        // Clear currently selected items
  //      uiInventoryBar.ClearCurrentlySelectedItems();

        StoreMenuOn = true;
        Player.Instance.PlayerInputIsDisabled = true;
        Time.timeScale = 0;
        storeMenu.SetActive(true);

        // Trigger garbage collector
        System.GC.Collect();

        // Highlight selected button
        HighlightButtonForSelectedTab();
    }

    public void DisableStoreMenu()
    {
        // Destroy any currently dragged items
       /// storeMenuInventoryManagement.DestroyCurrentlyDraggedItems();

        StoreMenuOn = false;
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
}
