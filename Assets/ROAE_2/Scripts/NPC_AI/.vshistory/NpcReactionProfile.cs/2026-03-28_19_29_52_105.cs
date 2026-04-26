using UnityEngine;

[CreateAssetMenu(fileName = "NewNpcReactionProfile", menuName = "Dialogue System/NPC Reaction Profile")]
public class NpcReactionProfile : ScriptableObject
{
    [Header("Authored dialogues for each action")]
    [SerializeField] private DialogueData neutralDialogue;
    [SerializeField] private DialogueData warmDialogue;
    [SerializeField] private DialogueData guardedDialogue;
    [SerializeField] private DialogueData hintDialogue;

    public DialogueData GetDialogueForAction(NpcActionType action)
    {
        switch (action)
        {
            case NpcActionType.Warm:
                return warmDialogue;
            case NpcActionType.Guarded:
                return guardedDialogue;
            case NpcActionType.Hint:
                return hintDialogue;
            case NpcActionType.Neutral:
            default:
                return neutralDialogue;
        }
    }
}