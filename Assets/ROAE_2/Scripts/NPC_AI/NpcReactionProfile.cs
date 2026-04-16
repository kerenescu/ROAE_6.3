using UnityEngine;

[CreateAssetMenu(fileName = "NewNpcReactionProfile", menuName = "Dialogue System/NPC Reaction Profile")]
public class NpcReactionProfile : ScriptableObject
{
    [Header("Authored dialogues for each action")]
    [SerializeField] private DialogueData neutralDialogue;
    [SerializeField] private DialogueData warmDialogue;
    [SerializeField] private DialogueData guardedDialogue;
    [SerializeField] private DialogueData hintDialogue;
    [SerializeField] private DialogueData warmHintDialogue;
    [SerializeField] private DialogueData guardedHintDialogue;

    public DialogueData GetDialogueForAction(NpcActionType action)
    {
        switch (action)
        {
            case NpcActionType.Warm:
                return warmDialogue != null ? warmDialogue : neutralDialogue;

            case NpcActionType.Guarded:
                return guardedDialogue != null ? guardedDialogue : neutralDialogue;

            case NpcActionType.Hint:
                return hintDialogue != null ? hintDialogue : neutralDialogue;

            case NpcActionType.WarmHint:
                if (warmHintDialogue != null) return warmHintDialogue;
                if (warmDialogue != null) return warmDialogue;
                if (hintDialogue != null) return hintDialogue;
                return neutralDialogue;

            case NpcActionType.GuardedHint:
                if (guardedHintDialogue != null) return guardedHintDialogue;
                if (guardedDialogue != null) return guardedDialogue;
                if (hintDialogue != null) return hintDialogue;
                return neutralDialogue;

            case NpcActionType.Neutral:
            default:
                return neutralDialogue;
        }
    }
}
