using System.Collections.Generic;
using UnityEngine;

public class AnticariatDialogueController : NpcToneDialogueControllerBase
{
    [Header("Profile")]
    [SerializeField] private NpcToneDialogueProfile profile;

    [Header("Dependencies")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private AnticariatDialogueConfig config;

    [Header("Legacy Dialogues (used only when profile is missing)")]
    [SerializeField] private DialogueData neutralIntroDialogue;
    [SerializeField] private DialogueData warmIntroDialogue;
    [SerializeField] private DialogueData mischievousIntroDialogue;
    [SerializeField] private DialogueData neutralRepeatDialogue;
    [SerializeField] private DialogueData warmRepeatDialogue;
    [SerializeField] private DialogueData mischievousRepeatDialogue;

    [Header("Legacy Narrative State (used only when profile is missing)")]
    [SerializeField] private string npcRelationshipId = "anticar";
    [SerializeField] private string introDoneFlagKey = "anticariat_intro_done";
    [SerializeField] private string knowledgeFlagKey = "barista_tarot_followup_done";
    [SerializeField] private string escapeFlagKey = "AnticarLeft";

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
    protected override string ControllerLogPrefix => "Anticariat";

    public void TriggerAnticariatDialogue()
    {
        TriggerDialogueInternal();
    }

    public void TriggerDialogue()
    {
        TriggerAnticariatDialogue();
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

    [ContextMenu("ROAE/Anticariat AI/Print current decision")]
    public void PrintCurrentDecision()
    {
        PrintCurrentDecisionInternal();
    }

    [ContextMenu("ROAE/Anticariat AI/Reset dev state and planner cache")]
    public void ResetDevStateAndPlannerCache()
    {
        ResetDevStateAndPlannerCacheInternal();
    }

    protected override string BuildResetSummary(NpcToneDialogueProfile resolvedProfile)
    {
        string npcId = resolvedProfile != null ? resolvedProfile.NpcIdOrDefault : npcRelationshipId;
        return "npc=" + npcId + " introFlag=reset escapeFlag=reset knowledgeFlag=reset runtime=reset";
    }

    protected override NpcToneDialogueProfile BuildLegacyProfile()
    {
        NpcToneDialogueProfile legacyProfile = ScriptableObject.CreateInstance<NpcToneDialogueProfile>();
        ConfigureProfile(legacyProfile);
        return legacyProfile;
    }

    private void ConfigureProfile(NpcToneDialogueProfile targetProfile)
    {
        string resolvedNpcId = string.IsNullOrWhiteSpace(npcRelationshipId) ? "anticar" : npcRelationshipId.Trim();
        NpcTonePlannerSettings plannerSettings = config != null ? config.ToPlannerSettings() : NpcTonePlannerSettings.Default;

        targetProfile.npcId = resolvedNpcId;
        targetProfile.outcomeLogTag = "AnticariatOutcome";
        targetProfile.dialogueLogTag = "AnticariatDialogue";
        targetProfile.devSummaryLogTag = "AnticariatDev";
        targetProfile.devResetLogTag = "AnticariatDevReset";
        targetProfile.plannerMode = config != null ? config.plannerMode : BaristaPlannerMode.ValueIteration;
        targetProfile.gamma = plannerSettings.gamma;
        targetProfile.epsilon = plannerSettings.evaluationEpsilon;
        targetProfile.maxValueIterations = plannerSettings.maxValueIterations;
        targetProfile.maxPolicyIterations = plannerSettings.maxPolicyIterations;
        targetProfile.maxPolicyEvaluationSweeps = plannerSettings.maxPolicyEvaluationSweeps;
        targetProfile.resetNpcIds = new[] { "barista", resolvedNpcId, "madame_lichenia" };
        targetProfile.playerPrefFlagsToReset = new[]
        {
            introDoneFlagKey,
            knowledgeFlagKey,
            escapeFlagKey,
            NarrativeFlagKeys.TarotReadingCompleted
        };
        targetProfile.dialogueFlagsToReset = new DialogueFlag[0];
        targetProfile.interactionBlockers = new List<NpcToneDialogueInteractionBlocker>
        {
            new NpcToneDialogueInteractionBlocker
            {
                failureReason = "anticar_already_left",
                conditions = new List<NpcToneDialogueCondition>
                {
                    CreatePlayerPrefCondition(escapeFlagKey, true, "escapeFlag")
                }
            }
        };
        targetProfile.phaseDefinitions = new List<NpcToneDialoguePhaseDefinition>
        {
            CreateIntroPhase(),
            CreateReturnPhase()
        };
    }

    private NpcToneDialoguePhaseDefinition CreateIntroPhase()
    {
        return new NpcToneDialoguePhaseDefinition
        {
            phaseId = "Intro",
            notes = "Prima interacțiune din anticariat.",
            conditions = new List<NpcToneDialogueCondition>
            {
                CreatePlayerPrefCondition(introDoneFlagKey, false, "introDone")
            },
            readUnknownText = CreatePlayerPrefSignal(knowledgeFlagKey),
            introDone = CreateFixedSignal(false),
            neutralDialogue = neutralIntroDialogue,
            warmDialogue = warmIntroDialogue,
            mischievousDialogue = mischievousIntroDialogue
        };
    }

    private NpcToneDialoguePhaseDefinition CreateReturnPhase()
    {
        return new NpcToneDialoguePhaseDefinition
        {
            phaseId = "Return",
            notes = "Vizită ulterioară după ce intro-ul a fost marcat ca făcut.",
            conditions = new List<NpcToneDialogueCondition>
            {
                CreatePlayerPrefCondition(introDoneFlagKey, true, "introDone")
            },
            readUnknownText = CreatePlayerPrefSignal(knowledgeFlagKey),
            introDone = CreateFixedSignal(true),
            neutralDialogue = neutralRepeatDialogue,
            warmDialogue = warmRepeatDialogue,
            mischievousDialogue = mischievousRepeatDialogue
        };
    }

    private static NpcToneDialogueBooleanSignal CreateFixedSignal(bool value)
    {
        return new NpcToneDialogueBooleanSignal
        {
            source = NpcToneDialogueBooleanSource.Fixed,
            fixedValue = value
        };
    }

    private static NpcToneDialogueBooleanSignal CreatePlayerPrefSignal(string flagKey)
    {
        return new NpcToneDialogueBooleanSignal
        {
            source = NpcToneDialogueBooleanSource.PlayerPrefFlag,
            playerPrefKey = flagKey
        };
    }

    private static NpcToneDialogueCondition CreatePlayerPrefCondition(
        string flagKey,
        bool expectedValue,
        string label)
    {
        return new NpcToneDialogueCondition
        {
            label = label,
            expectedValue = expectedValue,
            signal = CreatePlayerPrefSignal(flagKey)
        };
    }
}
