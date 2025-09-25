using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeTextArray : MonoBehaviour
{
    public Text[] texts; // Array of Text components
    public float fadeDuration = 2f; // Duration for each fade

    // Method to fade all text elements in
    public void FadeTextIn()
    {
        foreach (Text text in texts)
        {
            StartCoroutine(FadeText(text, 0f, 1f, fadeDuration)); // Fade from alpha 0 to 1
        }
    }

    // Method to fade all text elements out
    public void FadeTextOut()
    {
        foreach (Text text in texts)
        {
            StartCoroutine(FadeText(text, 1f, 0f, fadeDuration)); // Fade from alpha 1 to 0
        }
    }

    // Method to fade a single text element
    private IEnumerator FadeText(Text text, float startAlpha, float endAlpha, float duration)
    {
        float timeElapsed = 0f;
        Color startColor = text.color;
        startColor.a = startAlpha;

        Color endColor = text.color;
        endColor.a = endAlpha;

        text.color = startColor;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            text.color = Color.Lerp(startColor, endColor, timeElapsed / duration);
            yield return null;
        }

        // Ensure the final color is set correctly
        text.color = endColor;
    }
}

