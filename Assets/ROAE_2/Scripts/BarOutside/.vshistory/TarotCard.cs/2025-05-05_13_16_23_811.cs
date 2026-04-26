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
    [SerializeField] private float freezeSeconds = 5f;

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
        TarotReadingManager.IsReadingFrozen = true;

        //// NU mai aștepți automat
        //yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));

        TarotReadingManager.Instance.HideInterpretation();
        TarotReadingManager.Instance.NotifyCardRevealed();
        TarotReadingManager.IsReadingFrozen = false;
    }



    public void ShowFrontInstant()
    {
        front.SetActive(true);
        back.SetActive(false);
        isRevealed = true;
    }
}
