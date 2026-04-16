using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

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

        captionPanel.SetActive(false); // ascunde captionul la început
    }

    private void Update()
    {
        if (captionPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverCaption())
            {
                ForceHideCaption();
            }
        }
    }

    private bool IsPointerOverCaption()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject == captionPanel || result.gameObject.transform.IsChildOf(captionPanel.transform))
            {
                return true;
            }
        }

        return false;
    }

    public void ShowCaption(string text)
    {
        captionText.text = text;
        captionText.gameObject.SetActive(true);
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

    public void ForceHideCaption()
    {
        captionText.gameObject.SetActive(false);
        captionPanel.SetActive(false);
        StopAllCoroutines();
    }

    public bool IsCaptionVisible()
    {
        return captionPanel.activeSelf;
    }
}
