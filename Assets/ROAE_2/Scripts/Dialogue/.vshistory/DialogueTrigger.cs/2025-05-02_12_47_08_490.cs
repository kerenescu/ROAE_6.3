using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private DialogueData defaultAfterFirstUse;

    private bool hasBeenTriggered = false;

    public void TriggerDialogue()
    {
        Debug.Log("🎯 TriggerDialogue() a fost apelată!");

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
        }
        else
        {
            Debug.LogWarning("❌ DialogueData principal lipsește.");
        }
    }
}

