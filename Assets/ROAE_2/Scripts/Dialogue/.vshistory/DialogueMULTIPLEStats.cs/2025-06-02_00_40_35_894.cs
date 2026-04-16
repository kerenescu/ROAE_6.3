using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Flags")]
    [SerializeField] private DialogueFlag baristaFirstTimeFlag;    // setat după primul dialog
    [SerializeField] private DialogueFlag madameFlag;              // flag setat de Madame

    [Header("Dialogues")]
    [SerializeField] private DialogueData dialogueFirstTime;       // prima oară cu Barista
    [SerializeField] private DialogueData dialogueAfterBarista;    // fallback după prima dată
    [SerializeField] private DialogueData dialogueAfterMadame;     // dacă ai vorbit cu Madame

    [Header("Optional")]
    [SerializeField] private GameObject deckToActivate;

    public void TriggerDialogue()
    {
        if (dialogueManager == null)
        {
            Debug.LogWarning("❌ DialogueManager lipsește!");
            return;
        }

        Debug.Log("☕ Barista: Verificăm care dialog se aplică...");

        // 1️⃣ Ai vorbit cu Madame Lichenia?
        if (madameFlag != null && madameFlag.IsTriggered())
        {
            if (dialogueAfterMadame != null)
            {
                Debug.Log("🔮 Ai vorbit cu Madame — rulăm dialogul special.");
                dialogueManager.StartDialogue(dialogueAfterMadame);
                return;
            }
        }

        // 2️⃣ Prima discuție cu Barista?
        if (baristaFirstTimeFlag != null && !baristaFirstTimeFlag.IsTriggered())
        {
            if (dialogueFirstTime != null)
            {
                Debug.Log("🎬 Primul dialog cu Barista.");
                baristaFirstTimeFlag.MarkAsTriggered();
                dialogueManager.StartDialogue(dialogueFirstTime);
                return;
            }
        }

        // 3️⃣ Altfel: fallback
        if (dialogueAfterBarista != null)
        {
            Debug.Log("🧃 Rulăm fallback Barista.");
            dialogueManager.StartDialogue(dialogueAfterBarista);
        }

        // 🃏 Activează deck dacă este setat
        if (deckToActivate != null)
        {
            deckToActivate.SetActive(true);
        }
    }
}
