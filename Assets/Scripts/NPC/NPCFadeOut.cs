using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFadeOut : MonoBehaviour
{
    
    public void TriggerFadeOut()
    {
        StartCoroutine(FadeOut());
    }

    public IEnumerator FadeOut()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float duration = 2f; // duration of the fade-out effect
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / duration));
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
            yield return null;
        }

        // ensure the sprite is fully invisible at the end
        Color finalColor = sr.color;
        finalColor.a = 0f;
        sr.color = finalColor;
        // remove the NPC from the scene
        Destroy(gameObject);
    }
}
