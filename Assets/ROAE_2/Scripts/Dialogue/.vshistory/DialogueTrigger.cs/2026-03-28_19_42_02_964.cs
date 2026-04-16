using UnityEngine;

public class BaristaDialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private NpcReactionProfile reactionProfile;

    [Header("Temporary manual selection")]
    [SerializeField] private NpcActionType currentAction = NpcActionType.Neutral;

    public void TriggerDialogue()
    {
        Debug.Log("🗨️ Barista clicked → selecting dialogue from reaction profile");

        if (dialogueManager == null)
        {
            Debug.LogWarning("❌ DialogueManager lipsește!");
            return;
        }

        if (reactionProfile == null)
        {
            Debug.LogWarning("❌ NpcReactionProfile lipsește!");
            return;
        }

        DialogueData selectedDialogue = reactionProfile.GetDialogueForAction(currentAction);

        if (selectedDialogue == null)
        {
            Debug.LogWarning($"⚠️ Nu există DialogueData pentru acțiunea {currentAction}.");
            return;
        }

        dialogueManager.StartDialogue(selectedDialogue);
    }
}