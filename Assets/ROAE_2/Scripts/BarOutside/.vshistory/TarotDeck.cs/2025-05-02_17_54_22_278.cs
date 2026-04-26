using UnityEngine;

public class TarotDeck : MonoBehaviour
{
    public static TarotDeck Instance { get; private set; }

    [SerializeField] private GameObject interactionPrompt;
    private bool interactionEnabled = false;

    private bool hasBeenRead = false; // Poți înlocui cu salvare persistentă dacă vrei

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
        if (!hasBeenRead)
        {
            interactionEnabled = true;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);

            Debug.Log("[TAROT] Interacțiunea a fost activată.");
        }
    }

    public void OnInteract()
    {
        if (!interactionEnabled)
        {
            Debug.LogWarning("[TAROT] Încerci să interacționezi cu un tarot blocat.");
            return;
        }

        Debug.Log("[TAROT] Jucătorul interacționează cu tarotul.");
        hasBeenRead = true;
        interactionEnabled = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Aici declanșezi efectiv citirea tarotului sau dialogul
        // ex: DialogueTrigger.Instance.TriggerDialogue();
    }

    public bool HasBeenRead()
    {
        return hasBeenRead;
    }
}
