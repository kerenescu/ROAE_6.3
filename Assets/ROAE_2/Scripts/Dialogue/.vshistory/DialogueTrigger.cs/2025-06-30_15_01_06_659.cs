using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData dialogue; // primul dialog
    [SerializeField] private DialogueData defaultAfterFirstUse; // al doilea dialog (default)
    [SerializeField] private DialogueData thirdDialogue; // al treilea dialog (ex: după tarot)

    [SerializeField] private DialogueFlag dialogueFlag; // flag pentru primul dialog
    [SerializeField] private DialogueFlag dialogueFlagDEFAULT; // flag pentru al doilea dialog
    [SerializeField] private DialogueFlag flagForThirdDialogue; // flag pentru al treilea dialog

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

        // 🔵 Caz 3: dacă flagul pentru al treilea dialog este activ → redăm thirdDialogue
        if (!ignoreFlagCheck && flagForThirdDialogue != null && flagForThirdDialogue.IsTriggered())
        {
            Debug.Log("📌 Flag pentru al treilea dialog este setat → redăm thirdDialogue.");
            if (thirdDialogue != null)
            {
                dialogueManager.StartDialogue(thirdDialogue);
            }
            else
            {
                Debug.LogWarning("⚠️ thirdDialogue lipsește.");
            }
            return;
        }

        // 🟡 Caz 2: dacă flagul principal este setat → redăm defaultAfterFirstUse
        if (!ignoreFlagCheck && dialogueFlag != null && dialogueFlag.IsTriggered())
        {
            Debug.Log("📌 Flag deja setat → redăm defaultAfterFirstUse.");
            if (defaultAfterFirstUse != null)
            {
                dialogueManager.StartDialogue(defaultAfterFirstUse);
                dialogueFlagDEFAULT?.MarkAsTriggered();
            }
            else
            {
                Debug.LogWarning("⚠️ defaultAfterFirstUse lipsește.");
            }
            return;
        }

        // 🟢 Caz 1: rulăm dialogul principal
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
