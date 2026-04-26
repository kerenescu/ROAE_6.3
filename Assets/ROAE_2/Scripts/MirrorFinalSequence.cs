using System.Collections;
using UnityEngine;

public class MirrorFinalSequence : MonoBehaviour
{
    [Header("Obiecte oglinzi")]
    public GameObject oglindaSparta;
    public GameObject oglindaIntacta;

    [Header("Glow oglinda intactă")]
    public Color glowColor = Color.white;
    public float glowIntensity = 2f;

    [Header("Jesterflor")]
    public GameObject jesterflor;
    public float fadeDuration = 2f;

    [Header("Delay general")]
    public float delayBeforeReveal = 1f;

    private SpriteRenderer oglindaRenderer;
    private SpriteRenderer jesterflorRenderer;

    public void Reveal()
    {
        // Faza 1: Schimb oglinda
        if (oglindaSparta != null)
            oglindaSparta.SetActive(false);

        if (oglindaIntacta != null)
        {
            oglindaIntacta.SetActive(true);
            oglindaRenderer = oglindaIntacta.GetComponent<SpriteRenderer>();
        }

        // Activăm Jesterflor în avans, dar invizibil
        if (jesterflor != null)
        {
            jesterflor.SetActive(true);
            jesterflorRenderer = jesterflor.GetComponent<SpriteRenderer>();

            if (jesterflorRenderer != null)
            {
                Color c = jesterflorRenderer.color;
                c.a = 0f;
                jesterflorRenderer.color = c;
            }
        }

        // Așteptăm puțin, apoi începem revelația
        StartCoroutine(RevealAfterDelay());
    }

    private IEnumerator RevealAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeReveal);

        // Glow pe oglindă
        if (oglindaRenderer != null)
        {
            Material m = new Material(oglindaRenderer.material);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", glowColor * glowIntensity);
            oglindaRenderer.material = m;

            Debug.Log("💡 Glow-ul oglinzii intacte activat.");
        }

        // Fade-in la Jesterflor
        if (jesterflorRenderer != null)
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
}
