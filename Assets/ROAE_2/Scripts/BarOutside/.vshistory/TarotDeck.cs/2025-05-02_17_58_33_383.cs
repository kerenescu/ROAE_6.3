using UnityEngine;

public class TarotDeck : MonoBehaviour
{
    public static TarotDeck Instance { get; private set; }

    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private DialogueFlag tarotFlag; // nou!

    private bool interactionEnabled = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void EnableInteraction()
    {
        if (tarotFlag != null && tarotFlag.WasTriggered)
        {
            Debug.Log("[TAROT] Tarotul a fost deja citit. Interacțiunea nu va fi activată.");
            return;
        }

        interactionEnabled = true;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);

        Debug.Log("[TAROT] Interacțiunea a fost activată.");
    }

    public void OnInteract()
    {
        if (!interactionEnabled)
        {
            Debug.LogWarning("[TAROT] Încerci să interacționezi cu un tarot blocat.");
            return;
        }

        Debug.Log("[TAROT] Jucătorul interacționează cu tarotul.");

        if (tarotFlag != null)
            tarotFlag.MarkAsTriggered();

        interactionEnabled = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Aici declanșezi efectiv citirea tarotului sau dialogul
        // ex: DialogueTrigger.Instance.TriggerDialogue();
    }

    public bool HasBeenRead()
    {
        return tarotFlag != null && tarotFlag.WasTriggered;
    }
}
