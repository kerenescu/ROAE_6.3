using UnityEngine;

public class MadameLicheniaDialogueController : MonoBehaviour
{
    private enum MadameLicheniaTone
    {
        Neutral = 0,
        Warm = 1,
        Mischievous = 2
    }

    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData neutralIntroDialogue;
    [SerializeField] private DialogueData warmIntroDialogue;
    [SerializeField] private DialogueData mischievousIntroDialogue;
    [SerializeField] private DialogueData tarotReadyDialogue;

    [Header("State")]
    [SerializeField] private DialogueFlag tarotUnlockFlag;
    [SerializeField] private string npcRelationshipId = "madame_lichenia";

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public void TriggerDialogue()
    {
        if (dialogueManager == null)
        {
            Debug.LogWarning("[ROAE][MadameLichenia] DialogueManager missing.");
            return;
        }

        DialogueData selectedDialogue = ResolveDialogue();
        if (selectedDialogue == null)
        {
            Debug.LogWarning("[ROAE][MadameLichenia] No dialogue could be resolved.");
            return;
        }

        Log("TriggerDialogue -> " + selectedDialogue.name);
        dialogueManager.StartDialogue(selectedDialogue);
    }

    private DialogueData ResolveDialogue()
    {
        if (tarotUnlockFlag != null && tarotUnlockFlag.WasTriggered)
            return tarotReadyDialogue != null ? tarotReadyDialogue : neutralIntroDialogue;

        MadameLicheniaTone tone = ResolveTone();
        Log("Resolved tone=" + tone);

        switch (tone)
        {
            case MadameLicheniaTone.Warm:
                return warmIntroDialogue != null ? warmIntroDialogue : neutralIntroDialogue;

            case MadameLicheniaTone.Mischievous:
                return mischievousIntroDialogue != null ? mischievousIntroDialogue : neutralIntroDialogue;

            default:
                return neutralIntroDialogue;
        }
    }

    private MadameLicheniaTone ResolveTone()
    {
        CreativeCore core = CreativeCore.Instance ?? Object.FindFirstObjectByType<CreativeCore>();
        int creativity = core != null ? core.creativity : 0;
        int empathy = core != null ? core.empathy : 0;
        int corruption = core != null ? core.plantCorruption : 0;
        int relationship = NpcRelationshipState.GetRelationshipScore(npcRelationshipId);

        int warmth = empathy + Mathf.Max(relationship, 0);
        int mischief = corruption + (creativity >= 60 ? 1 : 0) + Mathf.Max(-relationship, 0);

        Log(
            "Tone input" +
            " creativity=" + creativity +
            " empathy=" + empathy +
            " corruption=" + corruption +
            " relationship=" + relationship +
            " warmth=" + warmth +
            " mischief=" + mischief);

        if (warmth >= mischief + 2 && warmth >= 2)
            return MadameLicheniaTone.Warm;

        if (mischief >= warmth + 2 && mischief >= 3)
            return MadameLicheniaTone.Mischievous;

        return MadameLicheniaTone.Neutral;
    }

    private void Log(string message)
    {
        if (!debugLogs)
            return;

        Debug.Log("[ROAE][MadameLichenia] " + message);
    }
}
