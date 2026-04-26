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

        captionPanel.SetActive(false); // ascunde captionul la inceput
    }

    // asta e pentru a afisa captionul
    public void ShowCaption(string text)
    {
        captionText.text = text;
        captionText.gameObject.SetActive(true); // asigură-te că textul e activ
        captionPanel.SetActive(true);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    // asta e pentru a ascunde captionul dupa un delay
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(autoHideDelay);
        captionPanel.SetActive(false);
    }

    // daca inchid telefonul/jurnalul etc fortez sa dispara captionul gandul
    public void ForceHideCaption()
    {
        captionText.gameObject.SetActive(false);
        captionPanel.SetActive(false); 
        StopAllCoroutines();                      
    }

}
