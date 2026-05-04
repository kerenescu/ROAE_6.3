using UnityEngine;
using AC;

public class GlowEffectController : MonoBehaviour
{
    [Header("Vignette UI")]
    public CanvasGroup vignetteCanvasGroup;

    [Header("Config")]
    public float fadeDuration = 0.8f;
    public float maxAlpha = 0.6f;


    // Apelează asta când toate ciupercile sunt în starea lor magică
    public void TriggerVignette()
    {
        StopAllCoroutines();
        StartCoroutine(GlowFlash());
        Sound glowSound = GetComponent<Sound>();
        if (glowSound != null)
        {
            glowSound.Play();
        }

    }

    private System.Collections.IEnumerator GlowFlash()
    {
        // Fade IN
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            vignetteCanvasGroup.alpha = Mathf.Lerp(0, maxAlpha, t / fadeDuration);
            yield return null;
        }

        // Fade OUT
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            vignetteCanvasGroup.alpha = Mathf.Lerp(maxAlpha, 0, t / fadeDuration);
            yield return null;
        }

        vignetteCanvasGroup.alpha = 0f;
    }
}
