using UnityEngine;

public class TarotDeck : MonoBehaviour
{
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private DialogueFlag tarotFlag;

    private bool isInteractable = false;

    private void Start()
    {
        if (tarotFlag != null && tarotFlag.WasTriggered)
        {
            isInteractable = true;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true); // afișăm un hint
        }
        else
        {
            gameObject.SetActive(false); // ascundem complet deck-ul dacă flag-ul nu e activat
        }
    }

    private void OnMouseDown()
    {
        if (!isInteractable) return;

        Debug.Log("Deckul a fost activat. Începem citirea în tarot.");
        // TODO: Aici declanșezi deschiderea UI-ului sau un nou dialog
    }
}
