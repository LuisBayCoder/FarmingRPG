using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NoteReaderUI : MonoBehaviour
{
    public GameObject notePanel;
    public Image diagramImage;

    private static NoteReaderUI instance;
    public static NoteReaderUI Instance => instance;

    public bool IsNoteOpen { get; private set; }

    private void Update()
    {
        if (notePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            HideNote();
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        notePanel.SetActive(false);
        IsNoteOpen = false;
    }

    public void ShowNote(Sprite diagram)
    {
        diagramImage.sprite = diagram;
        notePanel.SetActive(true);
        IsNoteOpen = true;
        // Pause the game when the note is open
        Time.timeScale = 0f; // Pause the game

    }

    public void HideNote()
    {
        notePanel.SetActive(false);
        StartCoroutine(DelayedClose());
        // Resume the game when the note is closed
        Time.timeScale = 1f; // Resume the game

    }
    
    private IEnumerator DelayedClose()
    {
        yield return null; // Wait 1 frame
        IsNoteOpen = false;
        Time.timeScale = 1f;
    }
}

