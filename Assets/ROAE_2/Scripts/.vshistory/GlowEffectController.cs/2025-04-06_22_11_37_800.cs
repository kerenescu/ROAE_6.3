using UnityEngine;

public class GlowEffectController : MonoBehaviour
{
    public CanvasGroup vignetteCanvasGroup;
    public float fadeDuration = 0.8f;
    public float maxAlpha = 0.6f;

    public void TriggerVignette()
    {
        StopAllCoroutines();
        StartCoroutine(GlowFlash());
    }

    private System.Collections.IEnumerator GlowFlash()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            vignetteCanvasGroup.alpha = Mathf.Lerp(0, maxAlpha, t / fadeDuration);
            yield return null;
        }

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            vignetteCanvasGroup.alpha = Mathf.Lerp(maxAlpha, 0, t / fadeDuration);
            yield return null;
        }

        vignetteCanvasGroup.alpha = 0;
    }
}
