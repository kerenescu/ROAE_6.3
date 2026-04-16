using UnityEngine;

public class TarotDeckInteract2D : MonoBehaviour
{
    [SerializeField] private GameObject tarotUIRoot;
    private bool hasStarted = false;

    private void OnMouseDown()
    {
        if (hasStarted)
        {
            Debug.Log("🛑 Deck-ul a fost deja activat.");
            return;
        }

        Debug.Log("🖱️ Click detectat pe deck-ul mic (2D).");

        hasStarted = true;

        if (tarotUIRoot != null)
        {
            tarotUIRoot.SetActive(true);
            Debug.Log("🎴 UI de tarot activat.");
        }
        else
        {
            Debug.LogWarning("⚠️ tarotUIRoot nu este setat în Inspector!");
        }

        if (TarotReadingManager.Instance != null)
        {
            TarotReadingManager.Instance.StartReading();
        }
        else
        {
            Debug.LogError("❌ TarotReadingManager.Instance e NULL!");
        }
    }
}
