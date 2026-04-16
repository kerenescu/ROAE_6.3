using UnityEngine;

public class MirrorFinalSequence : MonoBehaviour
{
    [Header("Obiecte de activat")]
    public GameObject oglindaGlowEffect;
    public GameObject jesterflor;

    public void Reveal()
    {
        if (oglindaGlowEffect != null)
        {
            oglindaGlowEffect.SetActive(true);
            Debug.Log("💡 Glow-ul oglinzii a fost activat.");
        }

        if (jesterflor != null)
        {
            jesterflor.SetActive(true);
            Debug.Log("🎭 Jesterflor a apărut.");
        }
    }
}
