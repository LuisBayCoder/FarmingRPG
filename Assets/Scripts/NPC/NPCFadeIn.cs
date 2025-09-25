using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFadeIn : MonoBehaviour
{
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a = 0f; // start invisible
        sr.color = c;
    }
    // Start is called before the first frame update
    void Start()
    {
        // get the animator component
        Animator animator = GetComponent<Animator>();
        // set the "FadeIn" trigger to true
        animator.SetTrigger("FadeIn");
        StartCoroutine(FadeIn());
    }
    IEnumerator FadeIn()
    {
        float duration = 2f; // duration of the fade-in effect
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
            yield return null;
        }

        // ensure the sprite is fully visible at the end
        Color finalColor = sr.color;
        finalColor.a = 1f;
        sr.color = finalColor;
    }
}
