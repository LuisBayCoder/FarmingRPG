using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingletonMonobehaviour<AudioManager>
{
    [SerializeField] private GameObject soundPrefab = null;

    [Header("Other")]
    // Sound list and dictionary
    [SerializeField] private SO_SoundList so_soundList = null;

    private Dictionary<SoundName, SoundItem> soundDictionary;

    protected override void Awake()
    {
        base.Awake();

        // Initialise sound dictionary
        soundDictionary = new Dictionary<SoundName, SoundItem>();

        // Load sound dictionary with sounds
        foreach (SoundItem soundItem in so_soundList.soundDetails)
        {
            soundDictionary.Add(soundItem.soundName, soundItem);
        }
    }

    public void PlaySound(SoundName soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out SoundItem soundItem) && soundPrefab != null)
        {
            GameObject soundGameObject = PoolManager.Instance.ReuseObject(soundPrefab, Vector3.zero, Quaternion.identity);

            Sound sound = soundGameObject.GetComponent<Sound>();

            sound.SetSound(soundItem);
            soundGameObject.SetActive(true);
            StartCoroutine(DisableSound(soundGameObject, soundItem.soundClip.length));
        }
    }

    private IEnumerator DisableSound(GameObject soundGameObject, float soundDuration)
    {
        yield return new WaitForSeconds(soundDuration);
        soundGameObject.SetActive(false);
    }
}