using UnityEngine;
using TMPro;
using System.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug; // Explicitly specify UnityEngine.Debug to avoid conflicts with System.Diagnostics.Debug

public class CoinUI : MonoBehaviour
{
    public TextMeshProUGUI coinText; // Reference to the UI text element
    public float animationDuration = 1f; // Duration of the count-up/count-down animation
    public AudioSource audioSource; // Single AudioSource for all sounds
    public AudioClip initialCoinSound; // Sound for the initial coin change
    public AudioClip countingCoinSound; // Sound for the continuous counting
    private int currentDisplayedCoins = 0; // The currently displayed coin value
    private Coroutine countCoroutine; // Reference to the running coroutine
    private Stopwatch stopwatch; // Custom timer

    private void Start()
    {
        // Initialize the stopwatch
        stopwatch = new Stopwatch();
        
        // Subscribe to the event
        CoinManager.Instance.OnCoinsChanged += UpdateCoinDisplay;
        // Initialize the display with the current coin balance
        UpdateCoinDisplay(CoinManager.Instance.Coins);
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        CoinManager.Instance.OnCoinsChanged -= UpdateCoinDisplay;
    }

    // Update the coin display
    private void UpdateCoinDisplay(int newCoins)
    {
        if (countCoroutine != null)
        {
            StopCoroutine(countCoroutine); // Stop the previous coroutine if it's running
        }
        countCoroutine = StartCoroutine(CountCoins(currentDisplayedCoins, newCoins));

        // Play the initial coin sound effect
        if (audioSource != null && initialCoinSound != null)
        {
            audioSource.clip = initialCoinSound; // Switch to the initial coin sound
            audioSource.loop = false; // Ensure looping is disabled for the initial sound
            audioSource.Play(); // Play the initial sound effect
            Debug.Log("Playing initial coin sound: " + initialCoinSound.name);
        }
        else
        {
            Debug.LogWarning("AudioSource or initialCoinSound is missing!");
        }
    }

    // Coroutine to animate the coin count
    private IEnumerator CountCoins(int from, int to)
    {
        stopwatch.Reset();
        stopwatch.Start();
        int step = (to > from) ? 1 : -1; // Determine if we are counting up or down
        float interval = animationDuration / Mathf.Abs(to - from); // Calculate interval for each step

        while (currentDisplayedCoins != to)
        {
            if (stopwatch.Elapsed.TotalSeconds >= interval)
            {
                Debug.Log("Elapsed time check passed: " + stopwatch.Elapsed.TotalSeconds); // Debug log to confirm elapsed time check
                stopwatch.Reset();
                stopwatch.Start();
                currentDisplayedCoins += step;
                coinText.text = currentDisplayedCoins.ToString();
                Debug.Log("Current displayed coins: " + currentDisplayedCoins); // Debug log to check coin amount
                   // Switch to the continuous counting sound
        if (audioSource != null && countingCoinSound != null && !audioSource.isPlaying)
        {
            audioSource.Stop(); // Stop the audio
            audioSource.clip = null; // Clear the clip
            audioSource.clip = countingCoinSound; // Switch to the counting sound
            audioSource.loop = true; // Enable looping for the continuous sound
            audioSource.Play(); // Start playing the counting sound
            Debug.Log("Playing counting coin sound: " + audioSource.clip.name);
        }
        else
        {
            Debug.LogWarning("AudioSource or countingCoinSound is missing!");
        }
            }
            yield return null;
        }

        // Ensure the final value is set
        currentDisplayedCoins = to;
        coinText.text = currentDisplayedCoins.ToString();
        Debug.Log("Final coin display: " + currentDisplayedCoins); // Debug log to confirm final value
        stopwatch.Stop();

        // Stop the continuous counting sound effect
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.loop = false; // Disable looping
            audioSource.Stop(); // Stop the continuous sound
            Debug.Log("Stopped counting coin sound.");
        }
    }
}
