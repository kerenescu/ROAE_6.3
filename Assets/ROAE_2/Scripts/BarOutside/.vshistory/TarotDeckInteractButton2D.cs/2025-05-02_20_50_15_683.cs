using UnityEngine;

public class TarotDeckInteractButton : MonoBehaviour
{
    [SerializeField] private GameObject tarotUIRoot;

    private bool hasStarted = false;

    public void OnClick()
    {
        if (hasStarted) return;
        hasStarted = true;

        if (tarotUIRoot != null)
            tarotUIRoot.SetActive(true);

        TarotReadingManager.Instance.StartReading();
        Debug.Log("🎴 Tarot UI activat prin click pe mini-deck.");
    }
}
