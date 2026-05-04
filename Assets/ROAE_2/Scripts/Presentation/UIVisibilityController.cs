using UnityEngine;

public class UIVisibilityController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError("❌ UIVisibilityController: CanvasGroup component not found!");
            }
        }
    }

    public void ShowUI()
    {
        if (canvasGroup == null) return;

        Time.timeScale = 0f;
        // dezactivează toate hotspoturile din scenă
        foreach (AC.Hotspot h in FindObjectsOfType<AC.Hotspot>())
        {
            h.enabled = false;

        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        Debug.Log("✅ UIVisibilityController: ShowUI called.");
    }

    public void HideUI()
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        Debug.Log("🕶️ UIVisibilityController: HideUI called.");
        Time.timeScale = 1f;
        // reactivează toate hotspoturile
        foreach (AC.Hotspot h in FindObjectsOfType<AC.Hotspot>())
        {
            h.enabled = true;

        }
    }
}
