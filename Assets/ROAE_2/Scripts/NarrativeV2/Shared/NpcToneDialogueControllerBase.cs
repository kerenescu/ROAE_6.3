using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public abstract class NpcToneDialogueControllerBase : MonoBehaviour
{
    private const int DefaultCreativity = 50;
    private const int DefaultEmpathy = 0;
    private const int DefaultCorruption = 0;

    private const int DevResetCreativity = 40;
    private const int DevResetEmpathy = 0;
    private const int DevResetCorruption = 0;

    private NpcToneDialogueProfile legacyProfileCache;

    protected readonly struct NpcToneDialogueDecision
    {
        public readonly NpcToneDialogueProfile profile;
        public readonly NpcToneDialoguePhaseDefinition phase;
        public readonly NpcTonePlanningRuntimeState runtimeState;
        public readonly NpcTonePlannerEvaluation evaluation;
        public readonly BaristaIntroTone tone;
        public readonly DialogueData dialogue;
        public readonly NpcToneDialogueRoutingKind routingKind;
        public readonly string routingSourceTone;

        public NpcToneDialogueDecision(
            NpcToneDialogueProfile profile,
            NpcToneDialoguePhaseDefinition phase,
            NpcTonePlanningRuntimeState runtimeState,
            NpcTonePlannerEvaluation evaluation,
            BaristaIntroTone tone,
            DialogueData dialogue,
            NpcToneDialogueRoutingKind routingKind,
            string routingSourceTone)
        {
            this.profile = profile;
            this.phase = phase;
            this.runtimeState = runtimeState;
            this.evaluation = evaluation;
            this.tone = tone;
            this.dialogue = dialogue;
            this.routingKind = routingKind;
            this.routingSourceTone = routingSourceTone;
        }
    }

    protected abstract DialogueManager DialogueManagerReference { get; set; }
    protected abstract NpcToneDialogueProfile AssignedProfile { get; }
    protected abstract bool AuditLogsEnabled { get; }
    protected abstract bool VerbosePlannerLogsEnabled { get; }
    protected abstract string ControllerLogPrefix { get; }
    protected abstract NpcToneDialogueProfile BuildLegacyProfile();

    protected virtual string BuildResetSummary(NpcToneDialogueProfile profile)
    {
        string npcId = profile != null ? profile.NpcIdOrDefault : "npc";
        return "npc=" + npcId + " runtime=reset";
    }

    protected void TriggerDialogueInternal()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        NpcToneDialogueProfile profile = ResolveProfile();
        if (profile == null)
        {
            LogFail(null, "missing_tone_profile", stopwatch.Elapsed.TotalMilliseconds);
            return;
        }

        if (!TryGetDialogueManager(out DialogueManager dialogueManager))
        {
            LogFail(profile, "missing_dialogue_manager", stopwatch.Elapsed.TotalMilliseconds);
            return;
        }

        if (profile.TryGetBlockingReason(out string blockingReason))
        {
            LogFail(profile, blockingReason, stopwatch.Elapsed.TotalMilliseconds);
            return;
        }

        if (!TryResolveDecision(profile, out NpcToneDialogueDecision decision, out string failReason))
        {
            LogFail(profile, failReason, stopwatch.Elapsed.TotalMilliseconds);
            return;
        }

        LogOutcome(decision, stopwatch.Elapsed.TotalMilliseconds);
        dialogueManager.StartDialogue(decision.dialogue);
        LogDialogueSuccess(decision, stopwatch.Elapsed.TotalMilliseconds);
    }

    protected void PrintCurrentDecisionInternal()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        NpcToneDialogueProfile profile = ResolveProfile();

        if (profile == null)
        {
            LogFail(null, "missing_tone_profile", stopwatch.Elapsed.TotalMilliseconds);
            return;
        }

        if (!TryResolveDecision(profile, out NpcToneDialogueDecision decision, out string failReason))
        {
            LogFail(profile, failReason, stopwatch.Elapsed.TotalMilliseconds);
            return;
        }

        LogOutcome(decision, stopwatch.Elapsed.TotalMilliseconds);
        Debug.Log(
            "[ROAE][AI][" + profile.DevSummaryLogTagOrDefault(ControllerLogPrefix) + "][SUMMARY] state={" +
            decision.runtimeState.ToDebugString() +
            "} result={" + BuildDecisionResult(decision) + "}");
    }

    protected void ResetDevStateAndPlannerCacheInternal()
    {
        NpcToneDialogueProfile profile = ResolveProfile();

        NpcAIDevTools.ResetRuntimeState(
            DevResetCreativity,
            DevResetEmpathy,
            DevResetCorruption,
            profile != null ? profile.resetNpcIds : null);

        if (profile != null)
        {
            NpcAIDevTools.ResetPlayerPrefFlags(profile.playerPrefFlagsToReset);
            NpcAIDevTools.ResetDialogueFlags(profile.dialogueFlagsToReset);
        }

        PlayerPrefs.Save();

        if (AuditLogsEnabled)
        {
            string resetTag = profile != null
                ? profile.DevResetLogTagOrDefault(ControllerLogPrefix)
                : ControllerLogPrefix + "DevReset";

            Debug.Log("[ROAE][AI][" + resetTag + "][SUCCESS] " + BuildResetSummary(profile));
        }
    }

    protected void InvalidateLegacyProfileCache()
    {
        if (legacyProfileCache == null)
            return;

        if (Application.isPlaying)
            Destroy(legacyProfileCache);
        else
            DestroyImmediate(legacyProfileCache);

        legacyProfileCache = null;
    }

    protected virtual void OnDestroy()
    {
        InvalidateLegacyProfileCache();
    }

    protected NpcToneDialogueProfile ResolveCurrentProfile()
    {
        return ResolveProfile();
    }

    private bool TryGetDialogueManager(out DialogueManager dialogueManager)
    {
        dialogueManager = DialogueManagerReference;
        if (dialogueManager != null)
            return true;

        dialogueManager = Object.FindFirstObjectByType<DialogueManager>();
        if (dialogueManager != null)
            DialogueManagerReference = dialogueManager;

        return dialogueManager != null;
    }

    private bool TryResolveDecision(
        NpcToneDialogueProfile profile,
        out NpcToneDialogueDecision decision,
        out string failReason)
    {
        if (!profile.TryResolvePhase(out NpcToneDialoguePhaseDefinition phase))
        {
            decision = default;
            failReason = "no_matching_phase";
            return false;
        }

        NpcTonePlanningRuntimeState runtimeState = BuildRuntimeState(profile, phase);
        NpcTonePlannerEvaluation evaluation = NpcTonePlanningSolvers.Evaluate(
            runtimeState,
            profile.plannerMode,
            profile.ToPlannerSettings(),
            VerbosePlannerLogsEnabled,
            AuditLogsEnabled);

        BaristaIntroTone tone = ResolveTone(profile, evaluation);
        NpcToneDialoguePhaseDefinition.DialogueResolution resolution = phase.ResolveDialogueWithRouting(tone);
        if (resolution.dialogue == null)
        {
            decision = default;
            failReason = "no_dialogue_for_tone_" + tone;
            return false;
        }

        decision = new NpcToneDialogueDecision(
            profile,
            phase,
            runtimeState,
            evaluation,
            tone,
            resolution.dialogue,
            resolution.routingKind,
            resolution.sourceToneLabel);
        failReason = string.Empty;
        return true;
    }

    private NpcTonePlanningRuntimeState BuildRuntimeState(
        NpcToneDialogueProfile profile,
        NpcToneDialoguePhaseDefinition phase)
    {
        CreativeCore core = CreativeCore.Instance ?? Object.FindFirstObjectByType<CreativeCore>();

        int creativity = core != null ? core.Creativity : PlayerPrefs.GetInt("creativity", DefaultCreativity);
        int corruption = core != null ? core.PlantCorruption : PlayerPrefs.GetInt("plantCorruption", DefaultCorruption);
        int empathy = core != null ? core.Empathy : PlayerPrefs.GetInt("empathy", DefaultEmpathy);
        int relationship = NpcRelationshipState.GetRelationshipScore(profile.NpcIdOrDefault);

        return phase.BuildRuntimeState(creativity, corruption, empathy, relationship);
    }

    private NpcToneDialogueProfile ResolveProfile()
    {
        if (AssignedProfile != null)
            return AssignedProfile;

        if (legacyProfileCache == null)
        {
            legacyProfileCache = BuildLegacyProfile();
            if (legacyProfileCache != null)
                legacyProfileCache.hideFlags = HideFlags.HideAndDontSave;
        }

        return legacyProfileCache;
    }

    private void LogOutcome(NpcToneDialogueDecision decision, double durationMs)
    {
        if (!AuditLogsEnabled)
            return;

        Debug.Log(
            "[ROAE][AI][" + decision.profile.OutcomeLogTagOrDefault(ControllerLogPrefix) + "][SUCCESS] planner=" +
            decision.profile.plannerMode +
            " moment=" + decision.profile.MomentIdOrDefault +
            " state={" + decision.runtimeState.ToDebugString() + "}" +
            " result={" + BuildDecisionResult(decision) + "}" +
            " durationMs=" + FormatDuration(durationMs));
    }

    private void LogDialogueSuccess(NpcToneDialogueDecision decision, double durationMs)
    {
        if (!AuditLogsEnabled)
            return;

        Debug.Log(
            "[ROAE][AI][" + decision.profile.DialogueLogTagOrDefault(ControllerLogPrefix) + "][SUCCESS] phase=" +
            decision.phase.PhaseIdOrDefault +
            " moment=" + decision.profile.MomentIdOrDefault +
            " tone=" + decision.tone +
            " dialogue=" + DialogueName(decision.dialogue) +
            " durationMs=" + FormatDuration(durationMs));

        Debug.Log(
            "[ROAE][AI][" + decision.profile.DialogueLogTagOrDefault(ControllerLogPrefix) + "][TONE] tone - [" +
            decision.tone +
            "] routing=" + decision.routingKind +
            " sourceTone=" + decision.routingSourceTone +
            " phase=" + decision.phase.PhaseIdOrDefault +
            " dialogue=" + DialogueName(decision.dialogue));
    }

    private void LogFail(NpcToneDialogueProfile profile, string reason, double durationMs)
    {
        if (!AuditLogsEnabled)
            return;

        string tag = profile != null
            ? profile.DialogueLogTagOrDefault(ControllerLogPrefix)
            : ControllerLogPrefix + "Dialogue";

        Debug.LogWarning("[ROAE][AI][" + tag + "][FAIL] reason=" + reason + " durationMs=" + FormatDuration(durationMs));
    }

    private string BuildDecisionResult(NpcToneDialogueDecision decision)
    {
        return "npc=" + decision.profile.NpcIdOrDefault +
               " moment=" + decision.profile.MomentIdOrDefault +
               " phase=" + decision.phase.PhaseIdOrDefault +
               " action=" + decision.evaluation.bestAction +
               " tone=" + decision.tone +
               " routing=" + decision.routingKind +
               " sourceTone=" + decision.routingSourceTone +
               " dialogue=" + DialogueName(decision.dialogue) +
               " reason=planner=" + decision.profile.plannerMode + " " + decision.evaluation.BuildDebugString();
    }

    private static string DialogueName(DialogueData dialogue)
    {
        return dialogue != null ? dialogue.name : "None";
    }

    private static BaristaIntroTone ResolveTone(
        NpcToneDialogueProfile profile,
        NpcTonePlannerEvaluation evaluation)
    {
        if (profile == null)
            return BaristaDialogueResolver.NormalizeTone(evaluation.mappedTone);

        switch (profile.toneSelectionMode)
        {
            case NpcToneDialogueToneSelectionMode.ForceWarm:
                return BaristaIntroTone.Warm;

            case NpcToneDialogueToneSelectionMode.ForceMischievous:
                return BaristaIntroTone.Mischievous;

            case NpcToneDialogueToneSelectionMode.ForceNeutral:
                return BaristaIntroTone.Neutral;

            default:
                return BaristaDialogueResolver.NormalizeTone(evaluation.mappedTone);
        }
    }

    private static string FormatDuration(double durationMs)
    {
        return durationMs.ToString("0.00");
    }
}
