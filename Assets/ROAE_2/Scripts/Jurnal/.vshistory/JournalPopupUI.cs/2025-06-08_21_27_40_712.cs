using UnityEngine;
using TMPro;
using System.Collections;

public class JournalPopupUI : MonoBehaviour
{
    public static JournalPopupUI Instance;

    public CanvasGroup canvasGroup;
    public TextMeshProUGUI textElement;
    public AudioSource audioSource;
    public AudioClip popupClip;

    public float showDuration = 2f;
    public float fadeDuration = 0.5f;

    private Coroutine currentRoutine;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void Show(string msg)
    {
        Debug.Log($"📢 Show() apelat cu mesaj: {msg}");

        if (currentRoutine != null)
        {
            Debug.Log("🛑 StopCoroutine() anterior");
            StopCoroutine(currentRoutine);
        }

        if (canvasGroup == null)
            Debug.LogError("❌ CanvasGroup este NULL!");
        if (textElement == null)
            Debug.LogError("❌ TextElement (TMP) este NULL!");
        if (audioSource == null)
            Debug.LogWarning("🔇 AudioSource este NULL!");
        if (popupClip == null)
            Debug.LogWarning("🔇 popupClip este NULL!");

        currentRoutine = StartCoroutine(PopupRoutine(msg));
    }


    private IEnumerator PopupRoutine(string msg)
    {
        gameObject.SetActive(true);
        textElement.text = msg;

        if (audioSource != null && popupClip != null)
            audioSource.PlayOneShot(popupClip);

        // Fade in
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(showDuration);

        // Fade out
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }
}
