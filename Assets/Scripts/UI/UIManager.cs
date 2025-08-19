using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonMonobehaviour<UIManager>
{

    private bool _pauseMenuOn = false;
    private bool _sleepMenuOn = false;
    [SerializeField] private UIInventoryBar uiInventoryBar = null;
    [SerializeField] private PauseMenuInventoryManagement pauseMenuInventoryManagement = null;
    [SerializeField] private GameObject pauseMenu = null;
    [SerializeField] private GameObject sleepMenu = null;
    [SerializeField] private GameObject[] menuTabs = null;
    [SerializeField] private Button[] menuButtons = null;
    

    public bool PauseMenuOn { get => _pauseMenuOn; set => _pauseMenuOn = value; }

    protected override void Awake()
    {
        base.Awake();

        pauseMenu.SetActive(false);
        sleepMenu.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        PauseMenu();
    }

    private void PauseMenu()
    {
        // Toggle pause menu if escape is pressed

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Don't respond to Escape if the note is open
            // This is to prevent the pause menu from opening when the note is open
            // and the player presses Escape
            // This is to prevent the pause menu from opening when the note is open
            if (NoteReaderUI.Instance != null && NoteReaderUI.Instance.IsNoteOpen)
            {
                Debug.Log("Note is open, not responding to Escape key.");
                return;
            }

            if (PauseMenuOn)
            {
                DisablePauseMenu();
            }
            else if (!_sleepMenuOn)
            {
                EnablePauseMenu();
            }
            //if the sleep menu is open, close it
            if (_sleepMenuOn)
            {
                DisableSleepMenu();
            }
        }
    }

    private void EnablePauseMenu()
    {
        // Destroy any currently dragged items
        uiInventoryBar.DestroyCurrentlyDraggedItems();

        // Clear currently selected items
        uiInventoryBar.ClearCurrentlySelectedItems();

        PauseMenuOn = true;
        Player.Instance.PlayerInputIsDisabled = true;
        Time.timeScale = 0;
        pauseMenu.SetActive(true);

        // Trigger garbage collector
        System.GC.Collect();

        // Highlight selected button
        HighlightButtonForSelectedTab();
    }

    public void DisablePauseMenu()
    {
        // Destroy any currently dragged items
        pauseMenuInventoryManagement.DestroyCurrentlyDraggedItems();

        PauseMenuOn = false;
        Player.Instance.PlayerInputIsDisabled = false;
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }

    public void EnableSleepMenu()
    {
        _sleepMenuOn = true;
        Player.Instance.PlayerInputIsDisabled = true;
        Time.timeScale = 0;
        sleepMenu.SetActive(true);

        // Trigger garbage collector
        System.GC.Collect();
    }

    public void DisableSleepMenu()
    {
        _sleepMenuOn = false;
        Player.Instance.PlayerInputIsDisabled = false;
        Time.timeScale = 1;
        sleepMenu.SetActive(false);
    }

    private void HighlightButtonForSelectedTab()
    {
        for (int i = 0; i < menuTabs.Length; i++)
        {
            if (menuTabs[i].activeSelf)
            {
                SetButtonColorToActive(menuButtons[i]);
            }

            else
            {
                SetButtonColorToInactive(menuButtons[i]);
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
        for (int i = 0; i < menuTabs.Length; i++)
        {
            if (i != tabNum)
            {
                menuTabs[i].SetActive(false);
            }
            else
            {
                menuTabs[i].SetActive(true);

            }
        }

        HighlightButtonForSelectedTab();

    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
