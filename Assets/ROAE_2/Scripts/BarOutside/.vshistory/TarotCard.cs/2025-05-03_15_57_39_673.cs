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
        if (isRevealed || inputBlocked) return;

        StartCoroutine(RevealWithInterpretation());
    }

    private IEnumerator RevealWithInterpretation()
    {
        inputBlocked = true;

        front.SetActive(true);
        back.SetActive(false);
        isRevealed = true;

        TarotReadingManager.Instance.SetMadameText(interpretation);

        yield return new WaitForSeconds(2f); // timp în care playerul nu poate da click pe altă carte

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
