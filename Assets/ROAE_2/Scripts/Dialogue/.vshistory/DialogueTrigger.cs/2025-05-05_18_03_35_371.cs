using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private DialogueData defaultAfterFirstUse;
    [SerializeField] private DialogueFlag dialogueFlag;

    [Header("Opțional")]
    [SerializeField] private GameObject deckToActivate; // ← ADĂUGAT

    private bool hasBeenTriggered = false;

    public void TriggerDialogue()
    {
        Debug.Log("🎯 TriggerDialogue() a fost apelată!");

        if (dialogueManager == null)
        {
            Debug.LogWarning("❌ DialogueManager lipsește!");
            return;
        }

        // 🟡 VERIFICĂM dacă flagul a fost deja setat
        if (dialogueFlag != null && dialogueFlag.IsTriggered())
        {
            Debug.Log("📌 Flag deja setat → trimitem dialogul default.");
            if (defaultAfterFirstUse != null)
            {
                dialogueManager.StartDialogue(defaultAfterFirstUse);
            }
            else
            {
                Debug.LogWarning("⚠️ defaultAfterFirstUse lipsește.");
            }
            return;
        }

        // 🟢 Dacă nu fusese setat încă, rulăm dialogul principal
        if (dialogue != null)
        {
            dialogueManager.StartDialogue(dialogue);
            Debug.Log("✅ Pornim dialogul principal.");

            // ✅ MARCĂM FLAGUL CA TRIGGERED
            if (dialogueFlag != null)
            {
                dialogueFlag.MarkAsTriggered();
                Debug.Log($"🟢 Flagul '{dialogueFlag.name}' a fost setat cu succes.");
            }

            if (deckToActivate != null)
            {
                deckToActivate.SetActive(true);
                Debug.Log("🎴 Deck activat după finalul dialogului.");
            }
        }
        else
        {
            Debug.LogWarning("❌ DialogueData principal lipsește.");
        }
    }

}
