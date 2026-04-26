using UnityEngine;

public class AnticariatDialogueController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private AnticariatDialogueConfig config;

    [Header("Dialogues")]
    [SerializeField] private DialogueData neutralIntroDialogue;
    [SerializeField] private DialogueData warmIntroDialogue;
    [SerializeField] private DialogueData mischievousIntroDialogue;
    [SerializeField] private DialogueData neutralRepeatDialogue;
    [SerializeField] private DialogueData warmRepeatDialogue;
    [SerializeField] private DialogueData mischievousRepeatDialogue;

    [Header("Narrative State")]
    [SerializeField] private string npcRelationshipId = "anticar";
    [SerializeField] private string introDoneFlagKey = "anticariat_intro_done";
    [SerializeField] private string knowledgeFlagKey = "barista_tarot_followup_done";
    [SerializeField] private string escapeFlagKey = "AnticarLeft";

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public void TriggerAnticariatDialogue()
    {
        if (dialogueManager == null)
            dialogueManager = Object.FindFirstObjectByType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogWarning("[ROAE][AnticariatDialogueController] DialogueManager missing.");
            return;
        }

        if (ReadFlag(escapeFlagKey))
        {
            Log("Trigger blocked because Anticar already left.");
            return;
        }

        bool introDone = ReadFlag(introDoneFlagKey);
        BaristaIntroTone tone = ResolveTone(introDone);
        DialogueData selectedDialogue = introDone
            ? ResolveRepeatDialogue(tone)
            : ResolveIntroDialogue(tone);

        if (selectedDialogue == null)
        {
            Debug.LogWarning("[ROAE][AnticariatDialogueController] No dialogue resolved.");
            return;
        }

        Log(
            "TriggerDialogue | introDone=" + introDone +
            " | plannerMode=" + ResolvePlannerMode() +
            " | tone=" + tone +
            " | dialogue=" + selectedDialogue.name +
            " | relationship=" + NpcRelationshipState.GetRelationshipScore(npcRelationshipId) +
            " | knowledgeFlag=" + knowledgeFlagKey +
            " | knowledgeValue=" + ReadFlag(knowledgeFlagKey));

        dialogueManager.StartDialogue(selectedDialogue);
    }

    public void TriggerDialogue()
    {
        TriggerAnticariatDialogue();
    }

    private BaristaIntroTone ResolveTone(bool introDone)
    {
        CreativeCore creativeCore = CreativeCore.Instance ?? Object.FindFirstObjectByType<CreativeCore>();
        BaristaIntroPlanningRuntimeState runtimeState = new BaristaIntroPlanningRuntimeState
        {
            readUnknownText = ReadFlag(knowledgeFlagKey),
            creativity = creativeCore != null ? creativeCore.Creativity : PlayerPrefs.GetInt("creativity", 50),
            corruption = creativeCore != null ? creativeCore.PlantCorruption : PlayerPrefs.GetInt("plantCorruption", 0),
            empathy = creativeCore != null ? creativeCore.Empathy : PlayerPrefs.GetInt("empathy", 0),
            relationship = NpcRelationshipState.GetRelationshipScore(npcRelationshipId),
            introDone = introDone,
            pendingDrink = BaristaDrinkType.None,
            pendingDrinkAcknowledged = false,
            heldDrink = BaristaDrinkType.None
        };

        BaristaPlannerEvaluation evaluation = BaristaIntroPlanningSolvers.Evaluate(
            runtimeState,
            ResolvePlannerMode(),
            ResolvePlannerSettings(),
            debugLogs);

        return BaristaDialogueResolver.NormalizeTone(evaluation.mappedTone);
    }

    private DialogueData ResolveIntroDialogue(BaristaIntroTone tone)
    {
        switch (tone)
        {
            case BaristaIntroTone.Warm:
                return FirstAssigned(warmIntroDialogue, neutralIntroDialogue, mischievousIntroDialogue);
            case BaristaIntroTone.Mischievous:
                return FirstAssigned(mischievousIntroDialogue, neutralIntroDialogue, warmIntroDialogue);
            default:
                return FirstAssigned(neutralIntroDialogue, warmIntroDialogue, mischievousIntroDialogue);
        }
    }

    private DialogueData ResolveRepeatDialogue(BaristaIntroTone tone)
    {
        switch (tone)
        {
            case BaristaIntroTone.Warm:
                return FirstAssigned(warmRepeatDialogue, neutralRepeatDialogue, mischievousRepeatDialogue);
            case BaristaIntroTone.Mischievous:
                return FirstAssigned(mischievousRepeatDialogue, neutralRepeatDialogue, warmRepeatDialogue);
            default:
                return FirstAssigned(neutralRepeatDialogue, warmRepeatDialogue, mischievousRepeatDialogue);
        }
    }

    private BaristaPlannerMode ResolvePlannerMode()
    {
        return config != null ? config.plannerMode : BaristaPlannerMode.ValueIteration;
    }

    private BaristaPlannerSettings ResolvePlannerSettings()
    {
        return config != null ? config.ToPlannerSettings() : BaristaPlannerSettings.Default;
    }

    private static DialogueData FirstAssigned(params DialogueData[] options)
    {
        if (options == null)
            return null;

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null)
                return options[i];
        }

        return null;
    }

    private static bool ReadFlag(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && PlayerPrefs.GetInt(key, 0) == 1;
    }

    private void Log(string message)
    {
        if (!debugLogs)
            return;

        Debug.Log("[ROAE][AnticariatDialogueController] " + message);
    }
}
