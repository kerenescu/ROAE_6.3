using UnityEngine;
using TMPro;
using System.Collections;

public class JournalNotificationUI : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public CanvasGroup canvasGroup;

    public float showDuration = 2f;
    public float fadeDuration = 0.5f;

    private Coroutine currentRoutine;

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
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(AnimateMessage(msg));
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
    }
}
