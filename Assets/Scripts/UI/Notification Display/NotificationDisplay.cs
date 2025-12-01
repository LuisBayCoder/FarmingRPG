using TMPro;
using UnityEngine;

public class NotificationDisplay : MonoBehaviour
{
    public TextMeshProUGUI textUI;
    public float fadeInTime = 0.3f;
    public float displayTime = 2f;
    public float fadeOutTime = 0.4f;

    private Coroutine routine;

    private void Awake()
    {
        // Start invisible
        Color c = textUI.color;
        c.a = 0;
        textUI.color = c;
    }

    public void Show(string message)
    {
        textUI.text = message;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FadeRoutine());
    }

    private System.Collections.IEnumerator FadeRoutine()
    {
        // Fade in
        yield return StartCoroutine(FadeAlpha(0f, 1f, fadeInTime));

        // Stay visible
        yield return new WaitForSeconds(displayTime);

        // Fade out
        yield return StartCoroutine(FadeAlpha(1f, 0f, fadeOutTime));
    }

    private System.Collections.IEnumerator FadeAlpha(float start, float end, float duration)
    {
        float t = 0;
        Color c = textUI.color;

        while (t < duration)
        {
            float a = Mathf.Lerp(start, end, t / duration);
            c.a = a;
            textUI.color = c;
            t += Time.deltaTime;
            yield return null;
        }

        c.a = end;
        textUI.color = c;
    }
}
