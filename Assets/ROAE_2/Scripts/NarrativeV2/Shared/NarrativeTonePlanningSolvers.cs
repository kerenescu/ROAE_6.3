using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class NarrativeTonePlanningSolvers
{
    public static NpcTonePlannerEvaluation Evaluate(
        NpcTonePlanningRuntimeState runtimeState,
        NpcToneDialogueProfile profile,
        bool verboseLogs,
        bool auditCacheLogs)
    {
        if (profile?.DecisionDefinition != null)
            return EvaluateWithDefinition(runtimeState, profile, verboseLogs, auditCacheLogs);

        BaristaPlannerMode fallbackPlannerMode = profile != null
            ? profile.ResolveEffectivePlannerMode()
            : BaristaPlannerMode.PolicyIteration;
        NpcTonePlannerSettings fallbackSettings = profile != null
            ? profile.ResolvePlannerSettings()
            : NpcTonePlannerSettings.Default;

        return NpcTonePlanningSolvers.Evaluate(
            runtimeState,
            fallbackPlannerMode,
            fallbackSettings,
            verboseLogs,
            auditCacheLogs);
    }

    public static NpcTonePlannerEvaluation Evaluate(
        NpcTonePlanningRuntimeState runtimeState,
        BaristaPlannerMode plannerMode,
        NpcTonePlannerSettings settings,
        bool verboseLogs,
        bool auditCacheLogs)
    {
        return NpcTonePlanningSolvers.Evaluate(
            runtimeState,
            plannerMode,
            settings,
            verboseLogs,
            auditCacheLogs);
    }

    public static BaristaIntroTone ResolveTone(
        NpcToneDialogueProfile profile,
        NpcActionType action,
        BaristaIntroTone fallbackTone)
    {
        return profile != null
            ? profile.ResolveToneForAction(action, fallbackTone)
            : fallbackTone;
    }

    private static NpcTonePlannerEvaluation EvaluateWithDefinition(
        NpcTonePlanningRuntimeState runtimeState,
        NpcToneDialogueProfile profile,
        bool verboseLogs,
        bool auditCacheLogs)
    {
        NpcDefinition definition = profile.DecisionDefinition;
        NpcToneBucketConfig bucketConfig = profile.toneBuckets != null
            ? profile.toneBuckets.ToRuntimeConfig()
            : NpcToneBucketConfig.Default;
        NpcAffineBiasResult biasResult = definition != null
            ? definition.ApplyStatAffineBias(
                runtimeState.creativity,
                runtimeState.empathy,
                runtimeState.corruption,
                runtimeState.relationship)
            : NpcAffineBiasResult.Identity(
                runtimeState.creativity,
                runtimeState.empathy,
                runtimeState.corruption,
                runtimeState.relationship);
        NpcDecisionContext context = new NpcDecisionContext(
            profile.NpcIdOrDefault,
            biasResult.rawCreativity,
            biasResult.rawEmpathy,
            biasResult.rawCorruption,
            biasResult.rawRelationship,
            biasResult.creativity,
            biasResult.empathy,
            biasResult.corruption,
            biasResult.relationship,
            BucketCreativity(biasResult.creativity, bucketConfig),
            BucketEmpathy(biasResult.empathy, bucketConfig),
            BucketCorruption(biasResult.corruption, bucketConfig),
            BucketRelationship(biasResult.relationship, bucketConfig));

        Dictionary<NpcActionType, float> actionScores = ComputeActionScores(definition, context, verboseLogs, auditCacheLogs);
        string contextBiasSummary = "affine={" + biasResult.ToDebugString() + "} " + ApplyContextualBiases(actionScores, profile, context);
        NpcActionType selectedAction = SelectBestAction(actionScores);
        BaristaNarrativeAction legacyAction = MapNpcActionToLegacyAction(selectedAction);
        BaristaIntroTone mappedTone = profile.ResolveToneForAction(
            selectedAction,
            NpcTonePlanningSolvers.MapActionToTone(legacyAction));

        if (auditCacheLogs || verboseLogs)
        {
            Debug.Log(
                "[ROAE][AI][RelationDebug] npc=" + profile.NpcIdOrDefault +
                " relationship=" + context.Relationship +
                " bucket=" + context.RelationshipBucket +
                " " + contextBiasSummary);
        }

        if (verboseLogs)
        {
            Debug.Log(
                "[ROAE][Planner][Generic][RESULT] npc=" + profile.NpcIdOrDefault +
                " action=" + selectedAction +
                " tone=" + mappedTone +
                " state={" + context.ToDebugString() + "}" +
                " context={" + contextBiasSummary + "}");
        }

        return new NpcTonePlannerEvaluation(
            legacyAction,
            mappedTone,
            GetBestToneScore(actionScores, profile, BaristaIntroTone.Neutral),
            GetBestToneScore(actionScores, profile, BaristaIntroTone.Warm),
            GetBestToneScore(actionScores, profile, BaristaIntroTone.Mischievous),
            FormatActionScores(actionScores, profile),
            contextBiasSummary);
    }

    private static CreativityBucket BucketCreativity(int value, NpcToneBucketConfig bucketConfig)
    {
        if (value <= bucketConfig.creativityLowMax)
            return CreativityBucket.Low;

        if (value >= bucketConfig.creativityHighMin)
            return CreativityBucket.High;

        return CreativityBucket.Medium;
    }

    private static EmpathyBucket BucketEmpathy(int value, NpcToneBucketConfig bucketConfig)
    {
        if (value <= bucketConfig.empathyLowMax)
            return EmpathyBucket.Low;

        if (value >= bucketConfig.empathyHighMin)
            return EmpathyBucket.High;

        return EmpathyBucket.Neutral;
    }

    private static CorruptionBucket BucketCorruption(int value, NpcToneBucketConfig bucketConfig)
    {
        if (value <= bucketConfig.corruptionLowMax)
            return CorruptionBucket.Low;

        if (value >= bucketConfig.corruptionHighMin)
            return CorruptionBucket.High;

        return CorruptionBucket.Medium;
    }

    private static RelationshipBucket BucketRelationship(int value, NpcToneBucketConfig bucketConfig)
    {
        if (value <= bucketConfig.relationshipBadMax)
            return RelationshipBucket.Bad;

        if (value >= bucketConfig.relationshipGoodMin)
            return RelationshipBucket.Good;

        return RelationshipBucket.Neutral;
    }

    private static string ApplyContextualBiases(
        Dictionary<NpcActionType, float> actionScores,
        NpcToneDialogueProfile profile,
        NpcDecisionContext context)
    {
        float creativity01 = Mathf.Clamp01(context.Creativity / 100f);
        float empathy01 = Mathf.Clamp01(context.Empathy / 100f);
        float corruption01 = Mathf.Clamp01(context.Corruption / 100f);
        float relationshipSigned = Mathf.Clamp(context.Relationship / 20f, -1f, 1f);
        float relationshipAbs = Mathf.Abs(relationshipSigned);

        float warmStatBias = (0.45f * empathy01) + (0.35f * creativity01) + (0.20f * (1f - corruption01));
        float neutralStatBias = 1f - (
            (Mathf.Abs(creativity01 - 0.5f) +
             Mathf.Abs(empathy01 - 0.5f) +
             Mathf.Abs(corruption01 - 0.5f)) / 1.5f);
        float mischievousStatBias = (0.55f * corruption01) + (0.30f * (1f - empathy01)) + (0.15f * (1f - creativity01));

        float warmRelationshipBias = Mathf.Max(0f, relationshipSigned) * 0.9f;
        float neutralRelationshipBias = (1f - relationshipAbs) * 0.35f;
        float mischievousRelationshipBias = Mathf.Max(0f, -relationshipSigned) * 0.9f;

        float warmBonus = (warmStatBias * 1.6f) + warmRelationshipBias;
        float neutralBonus = (Mathf.Clamp01(neutralStatBias) * 0.9f) + neutralRelationshipBias;
        float mischievousBonus = (mischievousStatBias * 1.6f) + mischievousRelationshipBias;

        List<NpcActionType> keys = new List<NpcActionType>(actionScores.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            NpcActionType action = keys[i];
            BaristaNarrativeAction legacyAction = MapNpcActionToLegacyAction(action);
            BaristaIntroTone tone = ResolveTone(profile, action, NpcTonePlanningSolvers.MapActionToTone(legacyAction));

            switch (tone)
            {
                case BaristaIntroTone.Warm:
                    actionScores[action] += warmBonus;
                    break;

                case BaristaIntroTone.Mischievous:
                    actionScores[action] += mischievousBonus;
                    break;

                default:
                    actionScores[action] += neutralBonus;
                    break;
            }
        }

        return "relationshipRaw=" + context.RawRelationship +
               " relationshipBiased=" + context.Relationship +
               " relationshipBucket=" + context.RelationshipBucket +
               " relationshipSigned=" + relationshipSigned.ToString("0.00") +
               " warmBonus=" + warmBonus.ToString("0.000") +
               " neutralBonus=" + neutralBonus.ToString("0.000") +
               " mischievousBonus=" + mischievousBonus.ToString("0.000") +
               " statWarm=" + warmStatBias.ToString("0.000") +
               " statNeutral=" + Mathf.Clamp01(neutralStatBias).ToString("0.000") +
               " statMischievous=" + mischievousStatBias.ToString("0.000") +
               " relWarm=" + warmRelationshipBias.ToString("0.000") +
               " relNeutral=" + neutralRelationshipBias.ToString("0.000") +
               " relMischievous=" + mischievousRelationshipBias.ToString("0.000");
    }

    private static NpcActionType SelectBestAction(Dictionary<NpcActionType, float> actionScores)
    {
        NpcActionType bestAction = NpcActionType.Neutral;
        float bestScore = float.NegativeInfinity;

        foreach (KeyValuePair<NpcActionType, float> pair in actionScores)
        {
            if (pair.Value > bestScore)
            {
                bestAction = pair.Key;
                bestScore = pair.Value;
            }
        }

        return bestAction;
    }

    private static Dictionary<NpcActionType, float> ComputeActionScores(
        NpcDefinition definition,
        NpcDecisionContext context,
        bool verboseLogs,
        bool auditCacheLogs)
    {
        Dictionary<NpcActionType, float> scores = new Dictionary<NpcActionType, float>();
        IReadOnlyList<NpcActionType> actions = definition != null && definition.AvailableActions != null && definition.AvailableActions.Count > 0
            ? definition.AvailableActions
            : GetDefaultActions();

        if (definition?.PlannerConfig != null)
        {
            NpcPolicySolution solution = NpcPolicySolver.GetOrBuildPolicy(
                definition.PlannerConfig,
                verboseLogs,
                auditCacheLogs);
            NpcPlannerSettings settings = definition.PlannerConfig.ToSettings();
            NpcDecisionState state = context.ToDecisionState();

            for (int i = 0; i < actions.Count; i++)
            {
                NpcActionType action = actions[i];
                scores[action] = ComputePlannerActionValue(
                    definition.PlannerConfig,
                    state,
                    action,
                    solution.values,
                    settings.gamma);
            }

            return scores;
        }

        for (int i = 0; i < actions.Count; i++)
        {
            NpcActionType action = actions[i];
            scores[action] = ComputeActionBaseScore(action);
        }

        return scores;
    }

    private static float ComputePlannerActionValue(
        NpcPlannerConfig config,
        NpcDecisionState state,
        NpcActionType action,
        IReadOnlyDictionary<NpcDecisionState, float> values,
        float gamma)
    {
        float score = NpcRewardEvaluator.GetReward(config.RewardProfile, state, action);
        List<StateTransition> transitions = NpcTransitionEvaluator.GetTransitions(config.TransitionProfile, state, action);

        for (int i = 0; i < transitions.Count; i++)
        {
            StateTransition transition = transitions[i];
            if (values.TryGetValue(transition.nextState, out float nextValue))
                score += gamma * transition.probability * nextValue;
        }

        return score;
    }

    private static float ComputeActionBaseScore(NpcActionType action)
    {
        return action == NpcActionType.Neutral ? 0.1f : 0f;
    }

    private static float GetBestToneScore(
        Dictionary<NpcActionType, float> actionScores,
        NpcToneDialogueProfile profile,
        BaristaIntroTone tone)
    {
        float bestScore = float.NegativeInfinity;

        foreach (KeyValuePair<NpcActionType, float> pair in actionScores)
        {
            BaristaNarrativeAction legacyAction = MapNpcActionToLegacyAction(pair.Key);
            BaristaIntroTone mappedTone = ResolveTone(
                profile,
                pair.Key,
                NpcTonePlanningSolvers.MapActionToTone(legacyAction));

            if (mappedTone != tone)
                continue;

            if (pair.Value > bestScore)
                bestScore = pair.Value;
        }

        return float.IsNegativeInfinity(bestScore) ? 0f : bestScore;
    }

    private static string FormatActionScores(
        Dictionary<NpcActionType, float> actionScores,
        NpcToneDialogueProfile profile)
    {
        StringBuilder builder = new StringBuilder();
        bool first = true;

        foreach (KeyValuePair<NpcActionType, float> pair in actionScores)
        {
            if (!first)
                builder.Append(" | ");

            BaristaNarrativeAction legacyAction = MapNpcActionToLegacyAction(pair.Key);
            BaristaIntroTone mappedTone = ResolveTone(
                profile,
                pair.Key,
                NpcTonePlanningSolvers.MapActionToTone(legacyAction));

            builder.Append(pair.Key)
                .Append("->")
                .Append(mappedTone)
                .Append("=")
                .Append(pair.Value.ToString("0.000"));

            first = false;
        }

        return builder.ToString();
    }

    private static IReadOnlyList<NpcActionType> GetDefaultActions()
    {
        return new[]
        {
            NpcActionType.Neutral,
            NpcActionType.Warm,
            NpcActionType.Guarded,
            NpcActionType.Hint,
            NpcActionType.WarmHint,
            NpcActionType.GuardedHint,
            NpcActionType.Mischievous,
            NpcActionType.Suspicious,
            NpcActionType.LoreHint,
            NpcActionType.Refuse,
            NpcActionType.Offer,
            NpcActionType.Deflect
        };
    }

    private static BaristaNarrativeAction MapNpcActionToLegacyAction(NpcActionType action)
    {
        switch (action)
        {
            case NpcActionType.Warm:
            case NpcActionType.Offer:
                return BaristaNarrativeAction.WarmOffer;

            case NpcActionType.WarmHint:
                return BaristaNarrativeAction.WarmReassure;

            case NpcActionType.Guarded:
            case NpcActionType.Refuse:
                return BaristaNarrativeAction.GuardedCheck;

            case NpcActionType.Hint:
            case NpcActionType.LoreHint:
                return BaristaNarrativeAction.RevealHint;

            case NpcActionType.GuardedHint:
            case NpcActionType.Deflect:
                return BaristaNarrativeAction.Deflect;

            case NpcActionType.Mischievous:
            case NpcActionType.Suspicious:
                return BaristaNarrativeAction.MischievousProbe;

            default:
                return BaristaNarrativeAction.ObserveNeutral;
        }
    }
}
