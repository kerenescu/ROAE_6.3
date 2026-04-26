using System.Collections;
using UnityEngine;

public class MirrorFinalSequence : MonoBehaviour
{
    [Header("Referințe")]
    public GameObject oglinda;
    public GameObject jesterflor;

    [Header("Glow Settings")]
    public Color glowColor = Color.white;
    public float glowIntensity = 2f;

    [Header("Apariție Jesterflor")]
    public float fadeDuration = 2f;

    private Material oglindaMaterial;
    private SpriteRenderer jesterflorRenderer;

    public void Reveal()
    {
        if (oglinda != null)
        {
            var renderer = oglinda.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                oglindaMaterial = new Material(renderer.material);
                oglindaMaterial.SetColor("_Color", glowColor);
                oglindaMaterial.EnableKeyword("_EMISSION");
                oglindaMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
                renderer.material = oglindaMaterial;

                Debug.Log("💡 Glow-ul oglinzii a fost activat.");
            }
        }

        if (jesterflor != null)
        {
            jesterflor.SetActive(true);
            jesterflorRenderer = jesterflor.GetComponent<SpriteRenderer>();

            if (jesterflorRenderer != null)
            {
                Color c = jesterflorRenderer.color;
                c.a = 0f;
                jesterflorRenderer.color = c;

                StartCoroutine(FadeInJesterflor());
            }
        }
    }

    private IEnumerator FadeInJesterflor()
    {
        float elapsed = 0f;
        Color color = jesterflorRenderer.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            jesterflorRenderer.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        jesterflorRenderer.color = new Color(color.r, color.g, color.b, 1f);
        Debug.Log("🎭 Jesterflor a apărut cu fade-in.");
    }
}
