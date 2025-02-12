using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Add this to use RawImage

public class PanelBlink : MonoBehaviour
{
    public Image cantBuy; // Reference to can't buy image
    private Coroutine blinkCoroutine; // Reference to the blinking coroutine
    public float blinkInterval = 0.1f; // Faster blinking interval (adjust as needed)
    public float blinkDuration = 2f; // Shorter blinking duration (adjust as needed)
    private float blinkTimer = 0f; // Custom timer for tracking blinking duration
    private bool isBlinking = false; // Track if blinking is active

    private void Update()
    {
        // Update the custom timer if blinking is active
        if (isBlinking)
        {
            blinkTimer += Time.unscaledDeltaTime; // Use unscaledDeltaTime to ignore Time.timeScale

            // Stop blinking after the specified duration
            if (blinkTimer >= blinkDuration)
            {
                StopBlinking(); // Stop blinking after the specified duration
            }
        }
    }

    public void StartBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine); // Stop any existing blinking coroutine
        }

        blinkTimer = 0f; // Reset the custom timer
        isBlinking = true; // Set blinking state to active
        blinkCoroutine = StartCoroutine(BlinkCantBuyImage());
    }

    public void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine); // Stop the blinking coroutine
            blinkCoroutine = null;
        }

        isBlinking = false; // Set blinking state to inactive
        cantBuy.enabled = false; // Ensure the image is off (hidden) when blinking stops
        Debug.Log("Blinking stopped. Total blinking duration: " + blinkTimer + " seconds.");
    }

    // Coroutine to handle the blinking effect
    private IEnumerator BlinkCantBuyImage()
    {
        while (true)
        {
            cantBuy.enabled = !cantBuy.enabled; // Toggle the visibility of the cantBuy image
            yield return new WaitForSecondsRealtime(blinkInterval); // Use WaitForSecondsRealtime to ignore Time.timeScale
        }
    }
}
