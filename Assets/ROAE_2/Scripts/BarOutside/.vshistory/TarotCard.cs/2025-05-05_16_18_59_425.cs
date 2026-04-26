using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TarotCard : MonoBehaviour
{
    [Header("Card Faces")]
    [SerializeField] private GameObject front;
    [SerializeField] private GameObject back;

    [Header("Interpretation")]
    [TextArea(3, 5)]
    [SerializeField] private string interpretation;

    [Header("Freeze Duration")]
    [SerializeField] private float freezeSeconds = 7f;

    private bool isRevealed = false;

    public void OnClick()
    {
        if (isRevealed || TarotReadingManager.IsReadingFrozen)
            return;

        isRevealed = true;
        StartCoroutine(RevealWithInterpretation());
    }

    private IEnumerator RevealWithInterpretation()
    {
        front.SetActive(true);
        back.SetActive(false);

        TarotReadingManager.Instance.ShowInterpretation(interpretation);

        // NU blochezi alte acțiuni
        StartCoroutine(HideInterpretationAfterDelay(6f)); // ↩️ temporizator separat

        TarotReadingManager.Instance.NotifyCardRevealed();
        isRevealed = true;
        yield break;
    }

    private IEnumerator HideInterpretationAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        // Doar dacă această interpretare este încă vizibilă
        TarotReadingManager.Instance.HideInterpretation();
    }



    public void ShowFrontInstant()
    {
        front.SetActive(true);
        back.SetActive(false);
        isRevealed = true;
    }
}
