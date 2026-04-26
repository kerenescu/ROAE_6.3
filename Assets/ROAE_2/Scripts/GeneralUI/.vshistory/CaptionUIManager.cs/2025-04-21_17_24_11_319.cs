using System.Collections;
using UnityEngine;
using TMPro;

public class CaptionUIManager : MonoBehaviour
{
    public static CaptionUIManager Instance;

    [SerializeField] private GameObject captionPanel;
    [SerializeField] private TMP_Text captionText;
    [SerializeField] private float autoHideDelay = 4f;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        captionPanel.SetActive(false);
    }

    public void ShowCaption(string text)
    {
        captionText.text = text;
        captionPanel.SetActive(true);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(autoHideDelay);
        captionPanel.SetActive(false);
    }
}
