using UnityEngine;

public class TarotDeck : MonoBehaviour
{
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private DialogueFlag tarotFlag;
    [SerializeField] private DialogueFlag alreadyReadTarotFlag; // ← NOU

    private bool isInteractable = false;

    private void Start()
    {
        if (tarotFlag != null && tarotFlag.WasTriggered)
        {
            isInteractable = true;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        if (!isInteractable) return;

        Debug.Log("🎴 Click pe deck. Începem citirea.");

        if (alreadyReadTarotFlag != null && alreadyReadTarotFlag.WasTriggered)
        {
            TarotReadingManager.Instance.StartRepeatReading(); // ← doar cărți întoarse
        }
        else
        {
            TarotReadingManager.Instance.StartReading(); // ← prima citire completă
            if (alreadyReadTarotFlag != null)
                alreadyReadTarotFlag.MarkAsTriggered(); // ← salvăm că s-a făcut
        }
    }
}
