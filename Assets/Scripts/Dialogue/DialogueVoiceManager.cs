using UnityEngine;
using PixelCrushers.DialogueSystem;

public class DialogueVoiceManager : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayVoice(Subtitle subtitle)
    {
        if (subtitle == null || string.IsNullOrEmpty(subtitle.formattedText.text)) return;

        // Assuming voice files are named after the subtitle line's ID
        string voiceFileName = $"NPC_Line{subtitle.dialogueEntry.id}.wav";
        AudioClip clip = Resources.Load<AudioClip>($"Audio/{voiceFileName}");
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}

