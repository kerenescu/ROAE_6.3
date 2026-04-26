using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TarotCard : MonoBehaviour
{
    [Header("Referințe vizuale")]
    [SerializeField] private GameObject front;
    [SerializeField] private GameObject back;

    [Header("Textul interpretării")]
    [TextArea(3, 5)]
    [SerializeField] private string interpretationText;

    [Header("Durata afișării interpretării")]
    [SerializeField] private float freezeDuration = 2.5f;

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
        // Afișăm fața cărții
        front.SetActive(true);
        back.SetActive(false);

        // Setăm textul Madamei
        TarotReadingManager.Instance.SetMadameText(interpretationText);

        // Blocăm interacțiunile
        TarotReadingManager.IsReadingFrozen = true;

        yield return new WaitForSecondsRealtime(freezeDuration);

        // Gata blocajul
        TarotReadingManager.IsReadingFrozen = false;

        // Ascundem textul după interpretare
        TarotReadingManager.Instance.SetMadameText("");

        // Informăm managerul că această carte a fost întoarsă
        TarotReadingManager.Instance.NotifyCardRevealed();
    }

    public void ShowFrontInstant()
    {
        front.SetActive(true);
        back.SetActive(false);
        isRevealed = true;
    }
}
