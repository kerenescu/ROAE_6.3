using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private DialogueData defaultAfterFirstUse;
    [SerializeField] private DialogueFlag dialogueFlag;

    [Header("Opțional")]
    [SerializeField] private GameObject deckToActivate; // ← ADĂUGAT

    private bool hasBeenTriggered = true;

    public void TriggerDialogue()
    {
        Debug.Log("🎯 TriggerDialogue() a fost apelată!");
        deckToActivate.SetActive(true);
        if (dialogueManager == null)
        {
            Debug.LogWarning("❌ DialogueManager lipsește!");
            return;
        }

        if (hasBeenTriggered && defaultAfterFirstUse != null)
        {
            Debug.Log("▶️ Redeschidere — trimitem spre dialogul default.");
            dialogueManager.StartDialogue(defaultAfterFirstUse);
            return;
        }

        if (dialogue != null)
        {
            hasBeenTriggered = true;
            Debug.Log("✅ Pornim dialogul principal.");
            dialogueManager.StartDialogue(dialogue);

            if (dialogueFlag != null)
            {
                dialogueFlag.MarkAsTriggered();
                Debug.Log($"🟢 Flagul '{dialogueFlag.name}' a fost setat cu succes.");

                // 🔥 Dacă avem deck și e legat de acest flag, îl activăm
                if (deckToActivate != null)
                {
                    deckToActivate.SetActive(true);
                    Debug.Log("🎴 Deck activat după finalul dialogului.");
                }
            }
        }
        else
        {
            Debug.LogWarning("❌ DialogueData principal lipsește.");
        }
    }
}
