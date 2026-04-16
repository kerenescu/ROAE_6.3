using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TarotCard : MonoBehaviour
{
    [SerializeField] private GameObject frontImage;
    [SerializeField] private GameObject backImage;
    [SerializeField] private string interpretationText;

    private bool isRevealed = false;

    public void OnClick()
    {
        if (isRevealed) return;

        isRevealed = true;
        frontImage.SetActive(true);
        backImage.SetActive(false);

        TarotReadingManager.Instance.SetMadameText(interpretationText);

        TarotReadingManager.Instance.NotifyCardRevealed();
    }

    public void SetInterpretation(string text)
    {
        interpretationText = text;
    }
}
