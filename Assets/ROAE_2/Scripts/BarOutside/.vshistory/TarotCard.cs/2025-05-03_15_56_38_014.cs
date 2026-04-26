using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TarotCard : MonoBehaviour
{
    [Header("Card Faces")]
    [SerializeField] private GameObject front;
    [SerializeField] private GameObject back;

    [Header("Interpretation")]
    [TextArea]
    [SerializeField] private string interpretation;

    private bool isRevealed = false;
    private bool inputBlocked = false;

    private void Start()
    {
        front.SetActive(false);
        back.SetActive(true);
    }

    public void OnCardClick()
    {
        if (isRevealed || inputBlocked || TarotReadingManager.Instance.IsReadingFrozen)
            return;

        StartCoroutine(RevealWithInterpretation());
    }

    private IEnumerator RevealWithInterpretation()
    {
        inputBlocked = true;
        TarotReadingManager.Instance.IsReadingFrozen = true;

        // Show front and hide back
        front.SetActive(true);
        back.SetActive(false);
        isRevealed = true;

        // Show interpretation
        TarotReadingManager.Instance.ShowInterpretation(interpretation);

        // Wait for space key
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        // Hide interpretation and card
        TarotReadingManager.Instance.HideInterpretation();
        gameObject.SetActive(false);

        TarotReadingManager.Instance.NotifyCardRevealed();
        TarotReadingManager.Instance.IsReadingFrozen = false;
        inputBlocked = false;
    }
}
