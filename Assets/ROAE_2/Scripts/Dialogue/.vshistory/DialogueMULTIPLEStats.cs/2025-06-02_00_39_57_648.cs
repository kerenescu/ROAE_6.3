using UnityEngine;

public class DialogueTrigger2 : MonoBehaviour
{
    [Header("Flags")]
    public DialogueFlag baristaFirstTimeFlag;     // Flag marcat după prima conversație cu Barista
    public DialogueFlag flagylDefault;            // Flag setat de Madame Lichenia

    [Header("Dialogues")]
    public Dialogue dialogueFirstTime;            // Dialogul inițial al Baristei
    public Dialogue dialogueDefaultAfterBarista;  // Dialogul fallback al Baristei
    public Dialogue dialogueAfterMadame;          // Dialogul special după ce ai vorbit cu Madame

    public void TriggerDialogue()
    {
        if (DialogueManager.Instance.IsDialoguePlaying())
            return;

        // 🧙‍♀️ Prioritar: ai vorbit cu Madame
        if (flagylDefault != null && flagylDefault.IsTriggered())
        {
            if (dialogueAfterMadame != null)
            {
                DialogueManager.Instance.StartDialogue(dialogueAfterMadame);
                return;
            }
        }

        // 🎬 Prima dată când vorbești cu Barista
        if (baristaFirstTimeFlag != null && !baristaFirstTimeFlag.IsTriggered())
        {
            baristaFirstTimeFlag.SetFlag(); // Marcheză că ai vorbit deja
            if (dialogueFirstTime != null)
            {
                DialogueManager.Instance.StartDialogue(dialogueFirstTime);
                return;
            }
        }

        // 🧃 Fallback: după ce ai vorbit prima oară
        if (dialogueDefaultAfterBarista != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueDefaultAfterBarista);
        }
    }
}
