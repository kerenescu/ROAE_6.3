using System;
using System.Collections.Generic;
using UnityEngine;

public enum NpcToneDialogueToneSelectionMode
{
    UsePlanner = 0,
    ForceNeutral = 1,
    ForceWarm = 2,
    ForceMischievous = 3
}

[Serializable]
public class NpcToneActionToneMapping
{
    public NpcActionType action = NpcActionType.Neutral;
    public BaristaIntroTone tone = BaristaIntroTone.Neutral;
}

[Serializable]
public class NpcToneBucketThresholds
{
    [Header("Creativity")]
    public int creativityLowMax = CreativeStatScale.CreativityLowMax;
    public int creativityHighMin = CreativeStatScale.CreativityHighMin;

    [Header("Corruption")]
    public int corruptionLowMax = CreativeStatScale.CorruptionLowMax;
    public int corruptionHighMin = CreativeStatScale.CorruptionHighMin;

    [Header("Empathy")]
    public int empathyLowMax = CreativeStatScale.EmpathyLowMax;
    public int empathyHighMin = CreativeStatScale.EmpathyHighMin;

    [Header("Relationship")]
    public int relationshipBadMax = CreativeStatScale.RelationshipBadMax;
    public int relationshipGoodMin = CreativeStatScale.RelationshipGoodMin;

    public NpcToneBucketConfig ToRuntimeConfig()
    {
        int resolvedEmpathyLowMax = empathyLowMax;
        int resolvedEmpathyHighMin = empathyHighMin;
        if (CreativeStatScale.LooksLikeLegacyEmpathyThresholds(empathyLowMax, empathyHighMin))
        {
            resolvedEmpathyLowMax = CreativeStatScale.ConvertLegacyEmpathyThreshold(empathyLowMax);
            resolvedEmpathyHighMin = CreativeStatScale.ConvertLegacyEmpathyThreshold(empathyHighMin);
        }

        int resolvedCorruptionLowMax = corruptionLowMax;
        int resolvedCorruptionHighMin = corruptionHighMin;
        if (CreativeStatScale.LooksLikeLegacyCorruptionThresholds(corruptionLowMax, corruptionHighMin))
        {
            resolvedCorruptionLowMax = CreativeStatScale.ConvertLegacyCorruptionLowMax(corruptionLowMax);
            resolvedCorruptionHighMin = CreativeStatScale.ConvertLegacyCorruptionHighMin(corruptionHighMin);
        }

        return new NpcToneBucketConfig(
            creativityLowMax,
            creativityHighMin,
            resolvedCorruptionLowMax,
            resolvedCorruptionHighMin,
            resolvedEmpathyLowMax,
            resolvedEmpathyHighMin,
            relationshipBadMax,
            relationshipGoodMin);
    }
}

[CreateAssetMenu(fileName = "NpcToneDialogueProfile", menuName = "ROAE/Narrative V2/NPC Tone Dialogue Profile")]
public class NpcToneDialogueProfile : ScriptableObject, INarrativeMomentDefinition
{
    [Header("Identity")]
    public string npcId = "npc";
    public string momentId = "";
    public string outcomeLogTag = "NpcOutcome";
    public string dialogueLogTag = "NpcDialogue";
    public string devSummaryLogTag = "NpcDev";
    public string devResetLogTag = "NpcDevReset";

    [Header("Moment Activation")]
    public bool isEnabled = true;
    public int priority = 0;
    public string requiredChapterId = "";
    public string requiredSceneId = "";
    public string requiredNarrativeMomentId = "";
    public List<string> requiredTrueFlags = new List<string>();
    public List<string> requiredFalseFlags = new List<string>();
    public bool skipWhenCompleted = false;
    public string completedFlagKey = "";

    [Header("Decision Model")]
    public NpcDefinition decisionDefinition;
    public List<NpcToneActionToneMapping> actionToneMappings = new List<NpcToneActionToneMapping>();

    [Header("Planner")]
    public BaristaPlannerMode plannerMode = BaristaPlannerMode.PolicyIteration;
    [Range(0f, 0.99f)] public float gamma = 0.85f;
    public float epsilon = 0.0001f;
    [Min(1)] public int maxValueIterations = 96;
    [Min(1)] public int maxPolicyIterations = 24;
    [Min(1)] public int maxPolicyEvaluationSweeps = 96;
    public NpcToneDialogueToneSelectionMode toneSelectionMode = NpcToneDialogueToneSelectionMode.UsePlanner;

    [Header("Tone Buckets")]
    public NpcToneBucketThresholds toneBuckets = new NpcToneBucketThresholds();

    [Header("Reset")]
    public string[] resetNpcIds = { "barista", "anticar", "madame_lichenia" };
    public string[] playerPrefFlagsToReset = new string[0];
    public DialogueFlag[] dialogueFlagsToReset = new DialogueFlag[0];

    [Header("Interaction")]
    public List<NpcToneDialogueInteractionBlocker> interactionBlockers = new List<NpcToneDialogueInteractionBlocker>();
    public List<NpcToneDialoguePhaseDefinition> phaseDefinitions = new List<NpcToneDialoguePhaseDefinition>();

    public string NpcIdOrDefault
    {
        get
        {
            return string.IsNullOrWhiteSpace(npcId) ? name : npcId.Trim();
        }
    }

    public string MomentIdOrDefault
    {
        get
        {
            return string.IsNullOrWhiteSpace(momentId) ? name : momentId.Trim();
        }
    }

    public string MomentId => MomentIdOrDefault;
    public bool IsEnabled => isEnabled;
    public int Priority => priority;
    public string RequiredChapterId => requiredChapterId;
    public string RequiredSceneId => requiredSceneId;
    public string RequiredNarrativeMomentId => requiredNarrativeMomentId;
    public IReadOnlyList<string> RequiredTrueFlags => requiredTrueFlags;
    public IReadOnlyList<string> RequiredFalseFlags => requiredFalseFlags;
    public bool SkipWhenCompleted => skipWhenCompleted;
    public string CompletedFlagKey => completedFlagKey;

    public string OutcomeLogTagOrDefault(string fallbackPrefix)
    {
        return string.IsNullOrWhiteSpace(outcomeLogTag) ? fallbackPrefix + "Outcome" : outcomeLogTag;
    }

    public string DialogueLogTagOrDefault(string fallbackPrefix)
    {
        return string.IsNullOrWhiteSpace(dialogueLogTag) ? fallbackPrefix + "Dialogue" : dialogueLogTag;
    }

    public string DevSummaryLogTagOrDefault(string fallbackPrefix)
    {
        return string.IsNullOrWhiteSpace(devSummaryLogTag) ? fallbackPrefix + "Dev" : devSummaryLogTag;
    }

    public string DevResetLogTagOrDefault(string fallbackPrefix)
    {
        return string.IsNullOrWhiteSpace(devResetLogTag) ? fallbackPrefix + "DevReset" : devResetLogTag;
    }

    public NpcDefinition DecisionDefinition => decisionDefinition;

    public NpcTonePlannerSettings ToPlannerSettings()
    {
        return new NpcTonePlannerSettings(
            Mathf.Clamp(gamma, 0f, 0.99f),
            Mathf.Max(0.00001f, epsilon),
            Mathf.Max(1, maxValueIterations),
            Mathf.Max(1, maxPolicyIterations),
            Mathf.Max(1, maxPolicyEvaluationSweeps),
            toneBuckets != null ? toneBuckets.ToRuntimeConfig() : NpcToneBucketConfig.Default);
    }

    public NpcTonePlannerSettings ResolvePlannerSettings()
    {
        if (decisionDefinition?.PlannerConfig != null)
        {
            NpcPlannerSettings settings = decisionDefinition.PlannerConfig.ToSettings();
            return new NpcTonePlannerSettings(
                settings.gamma,
                settings.epsilon,
                settings.maxValueIterations,
                settings.maxPolicyIterations,
                settings.maxPolicyEvaluationSweeps,
                toneBuckets != null ? toneBuckets.ToRuntimeConfig() : NpcToneBucketConfig.Default);
        }

        return ToPlannerSettings();
    }

    public BaristaPlannerMode ResolveEffectivePlannerMode()
    {
        if (decisionDefinition?.PlannerConfig != null)
            return decisionDefinition.PlannerConfig.PlannerMode == NpcPlannerMode.PolicyIteration
                ? BaristaPlannerMode.PolicyIteration
                : BaristaPlannerMode.ValueIteration;

        return plannerMode;
    }

    public void ApplyPlannerMode(BaristaPlannerMode mode)
    {
        plannerMode = mode;

        if (decisionDefinition?.PlannerConfig != null)
        {
            decisionDefinition.PlannerConfig.SetPlannerMode(
                mode == BaristaPlannerMode.PolicyIteration
                    ? NpcPlannerMode.PolicyIteration
                    : NpcPlannerMode.ValueIteration);
        }
    }

    public BaristaIntroTone ResolveToneForAction(NpcActionType action, BaristaIntroTone fallbackTone)
    {
        // Definition-backed NPCs use the shared action->tone mapping so that
        // per-NPC attitude differences come only from the explicit affine bias.
        if (decisionDefinition == null && actionToneMappings != null)
        {
            for (int i = 0; i < actionToneMappings.Count; i++)
            {
                NpcToneActionToneMapping mapping = actionToneMappings[i];
                if (mapping != null && mapping.action == action)
                    return mapping.tone;
            }
        }

        switch (action)
        {
            case NpcActionType.Warm:
            case NpcActionType.WarmHint:
            case NpcActionType.Offer:
                return BaristaIntroTone.Warm;

            case NpcActionType.Mischievous:
            case NpcActionType.Suspicious:
            case NpcActionType.LoreHint:
            case NpcActionType.Deflect:
                return BaristaIntroTone.Mischievous;

            case NpcActionType.Hint:
            case NpcActionType.Guarded:
            case NpcActionType.GuardedHint:
            case NpcActionType.Refuse:
            case NpcActionType.Neutral:
                return BaristaIntroTone.Neutral;

            default:
                return fallbackTone;
        }
    }

    public bool TryGetBlockingReason(out string failureReason)
    {
        if (interactionBlockers != null)
        {
            for (int i = 0; i < interactionBlockers.Count; i++)
            {
                NpcToneDialogueInteractionBlocker blocker = interactionBlockers[i];
                if (blocker != null && blocker.IsBlocked())
                {
                    failureReason = string.IsNullOrWhiteSpace(blocker.failureReason)
                        ? "interaction_blocked"
                        : blocker.failureReason;
                    return true;
                }
            }
        }

        failureReason = string.Empty;
        return false;
    }

    public bool TryResolvePhase(out NpcToneDialoguePhaseDefinition phase)
    {
        return TryResolvePhase(null, string.Empty, out phase);
    }

    public bool TryResolvePhase(
        NpcFactContext factContext,
        string introDoneFlagKeyOverride,
        out NpcToneDialoguePhaseDefinition phase)
    {
        if (phaseDefinitions != null)
        {
            for (int i = 0; i < phaseDefinitions.Count; i++)
            {
                NpcToneDialoguePhaseDefinition candidate = phaseDefinitions[i];
                if (candidate != null && candidate.Matches(factContext, introDoneFlagKeyOverride))
                {
                    phase = candidate;
                    return true;
                }
            }
        }

        phase = null;
        return false;
    }

    public bool MatchesActivation()
    {
        return NarrativeMomentSelection.IsActive(this, ReadPlayerPrefFlag);
    }

    private static bool ReadPlayerPrefFlag(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && PlayerPrefs.GetInt(key, 0) == 1;
    }
}
