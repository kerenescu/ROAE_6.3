using UnityEngine;
using System.Collections;

public class TarotCard : MonoBehaviour
{
    [SerializeField] private GameObject front;
    [SerializeField] private GameObject back;
    [TextArea][SerializeField] private string interpretation;

    private bool isRevealed = false;
    private bool inputBlocked = false;

    public void OnClick()
    {
        if (isRevealed || inputBlocked || TarotReadingManager.IsReadingFrozen) return;

        StartCoroutine(RevealWithInterpretation());
    }

    private IEnumerator RevealWithInterpretation()
    {
        inputBlocked = true;
        TarotReadingManager.IsReadingFrozen = true;

        front.SetActive(true);
        back.SetActive(false);
        isRevealed = true;

        MushroomInteractionText.Instance.ShowText(interpretation);

        yield return new WaitForSeconds(2f); // timp în care playerul nu poate da click pe altă carte

        TarotReadingManager.IsReadingFrozen = false;
        TarotReadingManager.Instance.NotifyCardRevealed();
        inputBlocked = false;
    }

    public void ShowFrontInstant()
    {
        front.SetActive(true);
        back.SetActive(false);
        isRevealed = true;
    }
}
