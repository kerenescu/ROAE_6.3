using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class JournalNotificationUI : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI messageText;
    public CanvasGroup canvasGroup;

    public float showDuration = 2f;
    public float fadeDuration = 0.5f;

    private Coroutine currentRoutine;
    private JournalPageData lastNotifiedPage;

    public static JournalNotificationUI Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        canvasGroup.alpha = 0f;
    }

    public void ShowMessage(string msg)
    {
        lastNotifiedPage = null;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(AnimateMessage(msg));
    }

    public void ShowPageNotification(JournalPageData page)
    {
        lastNotifiedPage = page;
        string pageName = page != null ? page.name : "Intrare noua";
        ShowMessage($"Pagină nouă în jurnal: {pageName}");
        lastNotifiedPage = page;
    }

    public void OnNotificationClick()
    {
        if (lastNotifiedPage != null && JournalUIFlow.Instance != null)
        {
            JournalUIFlow.Instance.OpenJournalToPage(lastNotifiedPage);
        }

        HideNotificationInstantly();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnNotificationClick();
    }

    private IEnumerator AnimateMessage(string msg)
    {
        messageText.text = msg;

        // Fade in
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        yield return new WaitForSeconds(showDuration);

        // Fade out
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0;
        messageText.text = "";
        currentRoutine = null;
        lastNotifiedPage = null;
    }

    private void HideNotificationInstantly()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        canvasGroup.alpha = 0f;
        messageText.text = "";
        lastNotifiedPage = null;
    }
}
