using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private DialogueData defaultAfterFirstUse;
    [SerializeField] private DialogueFlag dialogueFlag;

    [Header("Opțional")]
    [SerializeField] private GameObject deckToActivate;
    [SerializeField] private bool ignoreFlagCheck = false; // ✅ ADĂUGAT

    public void TriggerDialogue()
    {
        Debug.Log("🎯 TriggerDialogue() a fost apelată!");

        if (dialogueManager == null)
        {
            Debug.LogWarning("❌ DialogueManager lipsește!");
            return;
        }

        // 🟡 Dacă flagul e deja setat și nu ignorăm verificarea, redăm dialogul default
        if (!ignoreFlagCheck && dialogueFlag != null && dialogueFlag.IsTriggered())
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

        // 🟢 Rulăm dialogul principal
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
