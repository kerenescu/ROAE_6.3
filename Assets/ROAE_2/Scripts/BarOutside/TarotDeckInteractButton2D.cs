using UnityEngine;

public class TarotDeckInteract2D : MonoBehaviour
{
    [SerializeField] private GameObject tarotUIRoot;
    private bool hasStarted = false;

    private void OnMouseDown()
    {
        // 🛡️ NU permite click dacă deja e activă citirea
        if (hasStarted || (TarotReadingManager.Instance != null && TarotReadingManager.Instance.IsReadingActive()))
        {
            Debug.Log("🛑 Deck-ul NU poate fi clicat acum (citirea este deja activă).");
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
