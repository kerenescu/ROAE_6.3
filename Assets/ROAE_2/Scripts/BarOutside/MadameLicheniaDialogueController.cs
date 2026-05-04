using System.Collections.Generic;
using UnityEngine;

public class MadameLicheniaDialogueController : NpcToneDialogueControllerBase
{
    [Header("Profile")]
    [SerializeField] private NpcToneDialogueProfile profile;

    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData neutralIntroDialogue;
    [SerializeField] private DialogueData warmIntroDialogue;
    [SerializeField] private DialogueData mischievousIntroDialogue;
    [SerializeField] private DialogueData tarotReadyDialogue;

    [Header("State")]
    [SerializeField] private DialogueFlag tarotUnlockFlag;
    [SerializeField] private string npcRelationshipId = "madame_lichenia";

    [Header("Planner")]
    [SerializeField] private BaristaPlannerMode plannerMode = BaristaPlannerMode.PolicyIteration;
    [Range(0f, 0.99f)] [SerializeField] private float gamma = 0.85f;
    [SerializeField] private float epsilon = 0.0001f;
    [Min(1)] [SerializeField] private int maxValueIterations = 96;
    [Min(1)] [SerializeField] private int maxPolicyIterations = 24;
    [Min(1)] [SerializeField] private int maxPolicyEvaluationSweeps = 96;

    [Header("Debug")]
    [SerializeField] private bool auditLogs = true;
    [SerializeField] private bool verbosePlannerLogs;
    [SerializeField, HideInInspector] private bool debugLogs = true;

    public NpcToneDialogueProfile Profile => profile;

    protected override DialogueManager DialogueManagerReference
    {
        get => dialogueManager;
        set => dialogueManager = value;
    }

    protected override NpcToneDialogueProfile AssignedProfile => profile;
    protected override bool AuditLogsEnabled => auditLogs;
    protected override bool VerbosePlannerLogsEnabled => verbosePlannerLogs;
    protected override string ControllerLogPrefix => "Madame";

    public void TriggerDialogue()
    {
        TriggerDialogueInternal();
    }

    public void PopulateToneDialogueProfile(NpcToneDialogueProfile targetProfile)
    {
        if (targetProfile == null)
            return;

        ConfigureProfile(targetProfile);
        profile = targetProfile;
        InvalidateLegacyProfileCache();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (debugLogs && !auditLogs)
            auditLogs = true;

        InvalidateLegacyProfileCache();
    }
#endif

    [ContextMenu("ROAE/Madame AI/Print current decision")]
    public void PrintCurrentDecision()
    {
        PrintCurrentDecisionInternal();
    }

    [ContextMenu("ROAE/Madame AI/Reset dev state and planner cache")]
    public void ResetDevStateAndPlannerCache()
    {
        ResetDevStateAndPlannerCacheInternal();
    }

    protected override string BuildResetSummary(NpcToneDialogueProfile resolvedProfile)
    {
        string npcId = resolvedProfile != null ? resolvedProfile.NpcIdOrDefault : npcRelationshipId;
        return "npc=" + npcId + " tarotFlag=reset runtime=reset";
    }

    protected override NpcToneDialogueProfile BuildLegacyProfile()
    {
        NpcToneDialogueProfile legacyProfile = ScriptableObject.CreateInstance<NpcToneDialogueProfile>();
        ConfigureProfile(legacyProfile);
        return legacyProfile;
    }

    private void ConfigureProfile(NpcToneDialogueProfile targetProfile)
    {
        string resolvedNpcId = string.IsNullOrWhiteSpace(npcRelationshipId) ? "madame_lichenia" : npcRelationshipId.Trim();
        NpcTonePlannerSettings plannerSettings = ResolvePlannerSettings();

        targetProfile.npcId = resolvedNpcId;
        targetProfile.outcomeLogTag = "MadameOutcome";
        targetProfile.dialogueLogTag = "MadameDialogue";
        targetProfile.devSummaryLogTag = "MadameDev";
        targetProfile.devResetLogTag = "MadameDevReset";
        targetProfile.plannerMode = plannerMode;
        targetProfile.gamma = plannerSettings.gamma;
        targetProfile.epsilon = plannerSettings.evaluationEpsilon;
        targetProfile.maxValueIterations = plannerSettings.maxValueIterations;
        targetProfile.maxPolicyIterations = plannerSettings.maxPolicyIterations;
        targetProfile.maxPolicyEvaluationSweeps = plannerSettings.maxPolicyEvaluationSweeps;
        targetProfile.resetNpcIds = new[] { "barista", "anticar", resolvedNpcId };
        targetProfile.playerPrefFlagsToReset = new[]
        {
            NarrativeFlagKeys.TarotReadingCompleted
        };
        targetProfile.dialogueFlagsToReset = tarotUnlockFlag != null
            ? new[] { tarotUnlockFlag }
            : new DialogueFlag[0];
        targetProfile.interactionBlockers = new List<NpcToneDialogueInteractionBlocker>();
        targetProfile.phaseDefinitions = new List<NpcToneDialoguePhaseDefinition>
        {
            CreateIntroPhase(),
            CreateTarotReadyPhase()
        };
    }

    private NpcToneDialoguePhaseDefinition CreateIntroPhase()
    {
        return new NpcToneDialoguePhaseDefinition
        {
            phaseId = "Intro",
            notes = "Prima întâlnire cu Madame Lichenia.",
            conditions = new List<NpcToneDialogueCondition>
            {
                CreateDialogueFlagCondition(tarotUnlockFlag, false, "tarotReady")
            },
            readUnknownText = CreateFixedSignal(false),
            introDone = CreateFixedSignal(false),
            neutralDialogue = neutralIntroDialogue,
            warmDialogue = warmIntroDialogue,
            mischievousDialogue = mischievousIntroDialogue
        };
    }

    private NpcToneDialoguePhaseDefinition CreateTarotReadyPhase()
    {
        return new NpcToneDialoguePhaseDefinition
        {
            phaseId = "TarotReady",
            notes = "Deck-ul de tarot a fost deja deblocat pentru Madame.",
            conditions = new List<NpcToneDialogueCondition>
            {
                CreateDialogueFlagCondition(tarotUnlockFlag, true, "tarotReady")
            },
            readUnknownText = CreateFixedSignal(true),
            introDone = CreateFixedSignal(true),
            fixedDialogue = tarotReadyDialogue
        };
    }

    private NpcTonePlannerSettings ResolvePlannerSettings()
    {
        return new NpcTonePlannerSettings(
            Mathf.Clamp(gamma, 0f, 0.99f),
            Mathf.Max(0.00001f, epsilon),
            Mathf.Max(1, maxValueIterations),
            Mathf.Max(1, maxPolicyIterations),
            Mathf.Max(1, maxPolicyEvaluationSweeps));
    }

    private static NpcToneDialogueBooleanSignal CreateFixedSignal(bool value)
    {
        return new NpcToneDialogueBooleanSignal
        {
            source = NpcToneDialogueBooleanSource.Fixed,
            fixedValue = value
        };
    }

    private static NpcToneDialogueCondition CreateDialogueFlagCondition(
        DialogueFlag flag,
        bool expectedValue,
        string label)
    {
        return new NpcToneDialogueCondition
        {
            label = label,
            expectedValue = expectedValue,
            signal = new NpcToneDialogueBooleanSignal
            {
                source = NpcToneDialogueBooleanSource.DialogueFlagAsset,
                dialogueFlag = flag
            }
        };
    }
}
