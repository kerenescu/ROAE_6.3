using UnityEngine;

public class BaristaDialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private NpcReactionProfile reactionProfile;
    [SerializeField] private BaristaActionSelector actionSelector;

    [Header("Manual fallback")]
    [SerializeField] private NpcActionType currentAction = NpcActionType.Neutral;

    public void TriggerDialogue()
    {
        if (dialogueManager == null)
        {
            Debug.LogWarning("DialogueManager missing.");
            return;
        }

        if (reactionProfile == null)
        {
            Debug.LogWarning("NpcReactionProfile missing.");
            return;
        }

        NpcActionType action = currentAction;

        if (actionSelector != null)
            action = actionSelector.GetAction();

        DialogueData selectedDialogue = reactionProfile.GetDialogueForAction(action);

        if (selectedDialogue == null)
        {
            Debug.LogWarning("No dialogue for selected action.");
            return;
        }

        dialogueManager.StartDialogue(selectedDialogue);
    }
}