// ============================================================
//  NpcTonePlanningSolvers  –  tone planner generic
//  Folosit ca fallback MDP atunci cand un profil nu vine direct
//  cu NpcDefinition/NpcPlannerConfig din subsistemul NPC_AI.
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public enum BaristaNarrativeAction
{
    // Neutral
    ObserveNeutral = 0,
    InviteOrder = 1,
    GuardedCheck = 2,
    OfferSafe = 3,

    // Warm
    WarmOffer = 4,
    WarmReassure = 5,

    // Mischievous
    MischievousProbe = 6,
    OfferWeird = 7,
    RevealHint = 8,
    Deflect = 9
}

public enum BaristaNarrativePhase
{
    Intro = 0,
    Order = 1,
    PendingDrinkInitial = 2,
    PendingDrinkReminder = 3,
    AlreadyHasDrink = 4
}

// ── Runtime state (trimis de controllere spre planner) ──────────────────────

[Serializable]
public readonly struct NpcToneBucketConfig
{
    public static NpcToneBucketConfig Default => new NpcToneBucketConfig(
        CreativeStatScale.CreativityLowMax,
        CreativeStatScale.CreativityHighMin,
        CreativeStatScale.CorruptionLowMax,
        CreativeStatScale.CorruptionHighMin,
        CreativeStatScale.EmpathyLowMax,
        CreativeStatScale.EmpathyHighMin,
        CreativeStatScale.RelationshipBadMax,
        CreativeStatScale.RelationshipGoodMin);

    public readonly int creativityLowMax;
    public readonly int creativityHighMin;
    public readonly int corruptionLowMax;
    public readonly int corruptionHighMin;
    public readonly int empathyLowMax;
    public readonly int empathyHighMin;
    public readonly int relationshipBadMax;
    public readonly int relationshipGoodMin;

    public NpcToneBucketConfig(
        int creativityLowMax,
        int creativityHighMin,
        int corruptionLowMax,
        int corruptionHighMin,
        int empathyLowMax,
        int empathyHighMin,
        int relationshipBadMax,
        int relationshipGoodMin)
    {
        this.creativityLowMax = creativityLowMax;
        this.creativityHighMin = Mathf.Max(creativityLowMax + 1, creativityHighMin);
        this.corruptionLowMax = corruptionLowMax;
        this.corruptionHighMin = Mathf.Max(corruptionLowMax + 1, corruptionHighMin);
        this.empathyLowMax = empathyLowMax;
        this.empathyHighMin = Mathf.Max(empathyLowMax + 1, empathyHighMin);
        this.relationshipBadMax = relationshipBadMax;
        this.relationshipGoodMin = Mathf.Max(relationshipBadMax + 1, relationshipGoodMin);
    }
}

[Serializable]
public struct NpcTonePlanningRuntimeState
{
    public bool readUnknownText;
    public int creativity;
    public int corruption;
    public int empathy;
    public int relationship;          // valoare din BaristaWelcomeKeys.BaristaRelationship
    public bool introDone;
    public bool hasDrink;
    public BaristaDrinkType pendingDrink;
    public bool pendingDrinkAcknowledged;
    public BaristaDrinkType heldDrink;

    public BaristaNarrativePhase Phase
    {
        get
        {
            if (heldDrink != BaristaDrinkType.None)
                return BaristaNarrativePhase.AlreadyHasDrink;
            if (pendingDrink != BaristaDrinkType.None)
                return pendingDrinkAcknowledged
                    ? BaristaNarrativePhase.PendingDrinkReminder
                    : BaristaNarrativePhase.PendingDrinkInitial;
            if (hasDrink)
                return BaristaNarrativePhase.AlreadyHasDrink;
            if (introDone)
                return BaristaNarrativePhase.Order;
            return BaristaNarrativePhase.Intro;
        }
    }

    public int CorruptionTier => GetCorruptionTier(NpcToneBucketConfig.Default);

    public int CreativityTier => GetCreativityTier(NpcToneBucketConfig.Default);

    public int EmpathyTier => GetEmpathyTier(NpcToneBucketConfig.Default);

    public int RelationshipTier => GetRelationshipTier(NpcToneBucketConfig.Default);

    public int PendingDrinkTier
    {
        get
        {
            return ToDrinkTier(pendingDrink);
        }
    }

    public int HeldDrinkTier
    {
        get
        {
            return ToDrinkTier(heldDrink);
        }
    }

    public int GetCorruptionTier(NpcToneBucketConfig bucketConfig)
    {
        if (corruption <= bucketConfig.corruptionLowMax)
            return 0;

        if (corruption >= bucketConfig.corruptionHighMin)
            return 2;

        return 1;
    }

    public int GetCreativityTier(NpcToneBucketConfig bucketConfig)
    {
        if (creativity <= bucketConfig.creativityLowMax)
            return 0;

        if (creativity >= bucketConfig.creativityHighMin)
            return 2;

        return 1;
    }

    public int GetEmpathyTier(NpcToneBucketConfig bucketConfig)
    {
        if (empathy <= bucketConfig.empathyLowMax)
            return 0;

        if (empathy >= bucketConfig.empathyHighMin)
            return 2;

        return 1;
    }

    public int GetRelationshipTier(NpcToneBucketConfig bucketConfig)
    {
        if (relationship <= bucketConfig.relationshipBadMax)
            return 0;

        if (relationship >= bucketConfig.relationshipGoodMin)
            return 2;

        return 1;
    }

    public string ToDebugString()
    {
        return ToDebugString(NpcToneBucketConfig.Default);
    }

    public string ToDebugString(NpcToneBucketConfig bucketConfig)
    {
        return "phase=" + Phase +
               " readUnknownText=" + readUnknownText +
               " creativity=" + creativity +
               " creativityTier=" + GetCreativityTier(bucketConfig) +
               " corruption=" + corruption +
               " corruptionTier=" + GetCorruptionTier(bucketConfig) +
               " empathy=" + empathy +
               " empathyTier=" + GetEmpathyTier(bucketConfig) +
               " relationship=" + relationship +
               " relTier=" + GetRelationshipTier(bucketConfig) +
               " introDone=" + introDone +
               " hasDrink=" + hasDrink +
               " pendingDrink=" + pendingDrink +
               " pendingDrinkTier=" + PendingDrinkTier +
               " pendingAcknowledged=" + pendingDrinkAcknowledged +
               " heldDrink=" + heldDrink +
               " heldDrinkTier=" + HeldDrinkTier;
    }

    private static int ToDrinkTier(BaristaDrinkType drink)
    {
        switch (drink)
        {
            case BaristaDrinkType.Cola:
                return 1;
            case BaristaDrinkType.PhotosyntheticSap:
                return 2;
            default:
                return 0;
        }
    }
}

// ── State key pentru MDP ─────────────────────────────────────────────────────

internal struct BaristaPlanningStateKey : IEquatable<BaristaPlanningStateKey>
{
    public int creativityTier;    // 0-2
    public int corruptionTier;    // 0-2
    public int empathyTier;       // 0-2
    public int relationshipTier;  // 0-2
    public int knowledgeTier;     // 0-1

    public bool Equals(BaristaPlanningStateKey o)
    {
        return creativityTier == o.creativityTier &&
               corruptionTier == o.corruptionTier &&
               empathyTier == o.empathyTier &&
               relationshipTier == o.relationshipTier &&
               knowledgeTier == o.knowledgeTier;
    }

    public override bool Equals(object obj)
        => obj is BaristaPlanningStateKey o && Equals(o);

    public override int GetHashCode()
    {
        unchecked
        {
            int h = creativityTier;
            h = (h * 397) ^ corruptionTier;
            h = (h * 397) ^ empathyTier;
            h = (h * 397) ^ relationshipTier;
            h = (h * 397) ^ knowledgeTier;
            return h;
        }
    }

    public string ToDebugString()
    {
        return "creativity=" + creativityTier +
               " corruption=" + corruptionTier +
               " empathy=" + empathyTier +
               " relationship=" + relationshipTier +
               " knowledge=" + knowledgeTier;
    }
}

internal struct BaristaStateTransition
{
    public BaristaPlanningStateKey nextState;
    public float probability;
    public BaristaStateTransition(BaristaPlanningStateKey s, float p) { nextState = s; probability = p; }
}

internal struct NpcTonePlannerDecision
{
    public BaristaNarrativeAction bestAction;
    public Dictionary<BaristaNarrativeAction, float> actionScores;
    public Dictionary<BaristaPlanningStateKey, BaristaNarrativeAction> policy;
    public Dictionary<BaristaPlanningStateKey, float> values;
}

public readonly struct NpcTonePlannerSettings
{
    public static NpcTonePlannerSettings Default =>
        new NpcTonePlannerSettings(0.87f, 0.0001f, 96, 24, 96);

    public readonly float gamma;
    public readonly float evaluationEpsilon;
    public readonly int maxValueIterations;
    public readonly int maxPolicyIterations;
    public readonly int maxPolicyEvaluationSweeps;
    public readonly NpcToneBucketConfig bucketConfig;

    public NpcTonePlannerSettings(
        float gamma,
        float evaluationEpsilon,
        int maxValueIterations,
        int maxPolicyIterations,
        int maxPolicyEvaluationSweeps)
        : this(
            gamma,
            evaluationEpsilon,
            maxValueIterations,
            maxPolicyIterations,
            maxPolicyEvaluationSweeps,
            NpcToneBucketConfig.Default)
    {
    }

    public NpcTonePlannerSettings(
        float gamma,
        float evaluationEpsilon,
        int maxValueIterations,
        int maxPolicyIterations,
        int maxPolicyEvaluationSweeps,
        NpcToneBucketConfig bucketConfig)
    {
        this.gamma = Mathf.Clamp(gamma, 0f, 0.99f);
        this.evaluationEpsilon = Mathf.Max(0.00001f, evaluationEpsilon);
        this.maxValueIterations = Mathf.Max(1, maxValueIterations);
        this.maxPolicyIterations = Mathf.Max(1, maxPolicyIterations);
        this.maxPolicyEvaluationSweeps = Mathf.Max(1, maxPolicyEvaluationSweeps);
        this.bucketConfig = bucketConfig;
    }
}

public readonly struct NpcTonePlannerEvaluation
{
    public readonly BaristaNarrativeAction bestAction;
    public readonly BaristaIntroTone mappedTone;
    public readonly float neutralScore;
    public readonly float warmScore;
    public readonly float mischievousScore;
    public readonly string actionScoreSummary;
    public readonly string contextBiasSummary;

    public NpcTonePlannerEvaluation(
        BaristaNarrativeAction bestAction,
        BaristaIntroTone mappedTone,
        float neutralScore,
        float warmScore,
        float mischievousScore,
        string actionScoreSummary,
        string contextBiasSummary = "")
    {
        this.bestAction = bestAction;
        this.mappedTone = mappedTone;
        this.neutralScore = neutralScore;
        this.warmScore = warmScore;
        this.mischievousScore = mischievousScore;
        this.actionScoreSummary = actionScoreSummary ?? string.Empty;
        this.contextBiasSummary = contextBiasSummary ?? string.Empty;
    }

    public string BuildDebugString()
    {
        string debug =
            "action=" + bestAction +
               " tone=" + mappedTone +
               " neutral=" + neutralScore.ToString("0.000") +
               " warm=" + warmScore.ToString("0.000") +
               " mischievous=" + mischievousScore.ToString("0.000") +
               " scores={" + actionScoreSummary + "}";

        if (!string.IsNullOrWhiteSpace(contextBiasSummary))
            debug += " context={" + contextBiasSummary + "}";

        return debug;
    }
}

// ── Solver principal ─────────────────────────────────────────────────────────

public static class NpcTonePlanningSolvers
{
    private static readonly BaristaNarrativeAction[] Actions =
    {
        BaristaNarrativeAction.ObserveNeutral,
        BaristaNarrativeAction.InviteOrder,
        BaristaNarrativeAction.GuardedCheck,
        BaristaNarrativeAction.OfferSafe,
        BaristaNarrativeAction.WarmOffer,
        BaristaNarrativeAction.WarmReassure,
        BaristaNarrativeAction.MischievousProbe,
        BaristaNarrativeAction.OfferWeird,
        BaristaNarrativeAction.RevealHint,
        BaristaNarrativeAction.Deflect
    };

    private static readonly Dictionary<NpcTonePlannerCacheKey, NpcToneCachedPolicy> PolicyCache =
        new Dictionary<NpcTonePlannerCacheKey, NpcToneCachedPolicy>();

    // ── Intrare publică ───────────────────────────────────────────────────────

    public static void ClearCache()
    {
        PolicyCache.Clear();
    }

    public static BaristaNarrativeAction DecideAction(
        NpcTonePlanningRuntimeState runtimeState,
        BaristaPlannerMode plannerMode,
        bool verboseLogs)
    {
        return Evaluate(runtimeState, plannerMode, NpcTonePlannerSettings.Default, verboseLogs).bestAction;
    }

    public static BaristaNarrativeAction DecideAction(
        NpcTonePlanningRuntimeState runtimeState,
        BaristaPlannerMode plannerMode,
        NpcTonePlannerSettings settings,
        bool verboseLogs)
    {
        return Evaluate(runtimeState, plannerMode, settings, verboseLogs).bestAction;
    }

    public static NpcTonePlannerEvaluation Evaluate(
        NpcTonePlanningRuntimeState runtimeState,
        BaristaPlannerMode plannerMode,
        bool verboseLogs)
    {
        return Evaluate(runtimeState, plannerMode, NpcTonePlannerSettings.Default, verboseLogs);
    }

    public static NpcTonePlannerEvaluation Evaluate(
        NpcTonePlanningRuntimeState runtimeState,
        BaristaPlannerMode plannerMode,
        NpcTonePlannerSettings settings,
        bool verboseLogs)
    {
        return Evaluate(runtimeState, plannerMode, settings, verboseLogs, false);
    }

    public static NpcTonePlannerEvaluation Evaluate(
        NpcTonePlanningRuntimeState runtimeState,
        BaristaPlannerMode plannerMode,
        NpcTonePlannerSettings settings,
        bool verboseLogs,
        bool auditCacheLogs)
    {
        var currentState = ToStateKey(runtimeState, settings.bucketConfig);

        if (verboseLogs)
            Debug.Log("[ROAE][Planner][" + plannerMode + "][START]" +
                      " actions=" + Actions.Length +
                      " currentState={" + currentState.ToDebugString() + "}");

        NpcToneCachedPolicy cachedPolicy = GetOrBuildPolicy(plannerMode, settings, verboseLogs, auditCacheLogs);
        BaristaNarrativeAction bestAction = cachedPolicy.policy.TryGetValue(currentState, out BaristaNarrativeAction policyAction)
            ? policyAction
            : DefaultActionForState(currentState);

        Dictionary<BaristaNarrativeAction, float> actionScores =
            ComputeActionScores(currentState, cachedPolicy.values, settings);

        var evaluation = new NpcTonePlannerEvaluation(
            bestAction,
            MapActionToTone(bestAction),
            GetBestToneScore(actionScores, BaristaIntroTone.Neutral),
            GetBestToneScore(actionScores, BaristaIntroTone.Warm),
            GetBestToneScore(actionScores, BaristaIntroTone.Mischievous),
            FormatActionScores(actionScores),
            string.Empty);

        if (verboseLogs)
            Debug.Log("[ROAE][Planner][" + plannerMode + "][RESULT] state={" + currentState.ToDebugString() +
                      "} " + evaluation.BuildDebugString());

        return evaluation;
    }

    private static NpcToneCachedPolicy GetOrBuildPolicy(
        BaristaPlannerMode plannerMode,
        NpcTonePlannerSettings settings,
        bool verboseLogs,
        bool auditCacheLogs)
    {
        var cacheKey = new NpcTonePlannerCacheKey(plannerMode, settings);
        if (PolicyCache.TryGetValue(cacheKey, out NpcToneCachedPolicy cached))
        {
            if (auditCacheLogs)
            {
                Debug.Log(
                    "[ROAE][AI][NpcTonePlannerCache][HIT]");
            }

            return cached;
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        List<BaristaPlanningStateKey> stateSpace = GenerateStateSpace();
        BaristaPlanningStateKey seedState = stateSpace[0];
        NpcTonePlannerDecision decision = plannerMode == BaristaPlannerMode.PolicyIteration
            ? RunPolicyIteration(stateSpace, seedState, settings, verboseLogs)
            : RunValueIteration(stateSpace, seedState, settings, verboseLogs);
        stopwatch.Stop();

        cached = new NpcToneCachedPolicy(
            decision.policy,
            decision.values,
            stateSpace.Count,
            stopwatch.Elapsed.TotalMilliseconds);
        PolicyCache[cacheKey] = cached;

        if (auditCacheLogs)
        {
            Debug.Log(
                "[ROAE][AI][NpcTonePlannerCache][MISS] planner=" + plannerMode +
                " states=" + cached.stateCount +
                " policyEntries=" + cached.policy.Count +
                " buildDurationMs=" + cached.buildDurationMs.ToString("0.00"));
        }

        return cached;
    }

    /// <summary>
    /// Mapare acțiune → ton de intro.
    /// Warm e acum complet acoperit.
    /// </summary>
    public static BaristaIntroTone MapActionToTone(BaristaNarrativeAction action)
    {
        switch (action)
        {
            case BaristaNarrativeAction.WarmOffer:
            case BaristaNarrativeAction.WarmReassure:
                return BaristaIntroTone.Warm;

            case BaristaNarrativeAction.MischievousProbe:
            case BaristaNarrativeAction.OfferWeird:
            case BaristaNarrativeAction.RevealHint:
            case BaristaNarrativeAction.Deflect:
                return BaristaIntroTone.Mischievous;

            // ObserveNeutral, InviteOrder, GuardedCheck, OfferSafe
            default:
                return BaristaIntroTone.Neutral;
        }
    }

    // ── Generare state space ─────────────────────────────────────────────────

    private static List<BaristaPlanningStateKey> GenerateStateSpace()
    {
        var states = new List<BaristaPlanningStateKey>();

        for (int crea = 0; crea < 3; crea++)
        for (int corr = 0; corr < 3; corr++)
        for (int emp = 0; emp < 3; emp++)
        for (int rel = 0; rel < 3; rel++)
        for (int know = 0; know < 2; know++)
        {
            states.Add(new BaristaPlanningStateKey
            {
                creativityTier = crea,
                corruptionTier = corr,
                empathyTier = emp,
                relationshipTier = rel,
                knowledgeTier = know
            });
        }

        return states;
    }

    private static BaristaPlanningStateKey ToStateKey(
        NpcTonePlanningRuntimeState rs,
        NpcToneBucketConfig bucketConfig)
    {
        return new BaristaPlanningStateKey
        {
            creativityTier = rs.GetCreativityTier(bucketConfig),
            corruptionTier = rs.GetCorruptionTier(bucketConfig),
            empathyTier = rs.GetEmpathyTier(bucketConfig),
            relationshipTier = rs.GetRelationshipTier(bucketConfig),
            knowledgeTier = rs.readUnknownText ? 1 : 0
        };
    }

    // ── Value Iteration ──────────────────────────────────────────────────────

    private static NpcTonePlannerDecision RunValueIteration(
        List<BaristaPlanningStateKey> states,
        BaristaPlanningStateKey currentState,
        NpcTonePlannerSettings settings,
        bool verboseLogs)
    {
        var values = states.ToDictionary(s => s, s => 0f);

        for (int iter = 0; iter < settings.maxValueIterations; iter++)
        {
            var next = new Dictionary<BaristaPlanningStateKey, float>(values.Count);
            float delta = 0f;

            foreach (var s in states)
            {
                float best = float.NegativeInfinity;
                foreach (var a in Actions)
                {
                    float q = ComputeActionValue(s, a, values, settings);
                    if (q > best) best = q;
                }
                next[s] = best;
                delta = Mathf.Max(delta, Mathf.Abs(best - values[s]));
            }

            values = next;
            if (verboseLogs) Debug.Log("[ROAE][Planner][VI] iter=" + iter + " delta=" + delta.ToString("0.00000"));
            if (delta < settings.evaluationEpsilon) { if (verboseLogs) Debug.Log("[ROAE][Planner][VI] converged iter=" + iter); break; }
        }

        var policy = states.ToDictionary(s => s, s => SelectBestAction(s, values, settings).bestAction);
        return new NpcTonePlannerDecision
        {
            bestAction = policy[currentState],
            actionScores = ComputeActionScores(currentState, values, settings),
            policy = policy,
            values = values
        };
    }

    // ── Policy Iteration ─────────────────────────────────────────────────────

    private static NpcTonePlannerDecision RunPolicyIteration(
        List<BaristaPlanningStateKey> states,
        BaristaPlanningStateKey currentState,
        NpcTonePlannerSettings settings,
        bool verboseLogs)
    {
        var values = states.ToDictionary(s => s, s => 0f);
        var policy = states.ToDictionary(s => s, s => DefaultActionForState(s));

        for (int iter = 0; iter < settings.maxPolicyIterations; iter++)
        {
            // Policy evaluation
            for (int sweep = 0; sweep < settings.maxPolicyEvaluationSweeps; sweep++)
            {
                float delta = 0f;
                foreach (var s in states)
                {
                    float v = ComputeActionValue(s, policy[s], values, settings);
                    delta = Mathf.Max(delta, Mathf.Abs(v - values[s]));
                    values[s] = v;
                }
                if (verboseLogs) Debug.Log("[ROAE][Planner][PI][Eval] iter=" + iter + " sweep=" + sweep + " delta=" + delta.ToString("0.00000"));
                if (delta < settings.evaluationEpsilon) break;
            }

            // Policy improvement
            bool stable = true;
            int changed = 0;
            foreach (var s in states)
            {
                var oldA = policy[s];
                var newA = SelectBestAction(s, values, settings).bestAction;
                if (newA != oldA) { stable = false; changed++; policy[s] = newA; }
            }
            if (verboseLogs) Debug.Log("[ROAE][Planner][PI][Improve] iter=" + iter + " changed=" + changed + " stable=" + stable);
            if (stable) break;
        }

        return new NpcTonePlannerDecision
        {
            bestAction = policy[currentState],
            actionScores = ComputeActionScores(currentState, values, settings),
            policy = policy,
            values = values
        };
    }

    // ── Helpers de selecție ──────────────────────────────────────────────────

    private static (BaristaNarrativeAction bestAction, float bestScore) SelectBestAction(
        BaristaPlanningStateKey state,
        Dictionary<BaristaPlanningStateKey, float> values,
        NpcTonePlannerSettings settings)
    {
        var bestAction = Actions[0];
        float bestScore = float.NegativeInfinity;
        foreach (var a in Actions)
        {
            float q = ComputeActionValue(state, a, values, settings);
            if (q > bestScore) { bestScore = q; bestAction = a; }
        }
        return (bestAction, bestScore);
    }

    private static Dictionary<BaristaNarrativeAction, float> ComputeActionScores(
        BaristaPlanningStateKey state,
        Dictionary<BaristaPlanningStateKey, float> values,
        NpcTonePlannerSettings settings)
    {
        var scores = new Dictionary<BaristaNarrativeAction, float>();
        foreach (var a in Actions) scores[a] = ComputeActionValue(state, a, values, settings);
        return scores;
    }

    private static float ComputeActionValue(
        BaristaPlanningStateKey state,
        BaristaNarrativeAction action,
        Dictionary<BaristaPlanningStateKey, float> values,
        NpcTonePlannerSettings settings)
    {
        float reward = EvaluateReward(state, action);
        var transitions = EvaluateTransitions(state, action);
        float future = 0f;
        for (int i = 0; i < transitions.Count; i++)
            future += transitions[i].probability * values[transitions[i].nextState];
        return reward + settings.gamma * future;
    }

    // ── Reward function ──────────────────────────────────────────────────────

    private static float EvaluateReward(BaristaPlanningStateKey s, BaristaNarrativeAction a)
    {
        float r = BaseActionReward(a);

        if (s.empathyTier == 2)
        {
            if (a == BaristaNarrativeAction.WarmOffer) r += 2.10f;
            if (a == BaristaNarrativeAction.WarmReassure) r += 1.75f;
            if (a == BaristaNarrativeAction.ObserveNeutral) r -= 0.40f;
            if (a == BaristaNarrativeAction.Deflect) r -= 0.25f;
        }
        else if (s.empathyTier == 1)
        {
            if (a == BaristaNarrativeAction.ObserveNeutral) r += 0.30f;
            if (a == BaristaNarrativeAction.OfferSafe) r += 0.20f;
            if (a == BaristaNarrativeAction.WarmOffer) r += 0.10f;
        }
        else
        {
            if (a == BaristaNarrativeAction.WarmOffer) r -= 1.10f;
            if (a == BaristaNarrativeAction.WarmReassure) r -= 0.95f;
            if (a == BaristaNarrativeAction.GuardedCheck) r += 0.55f;
            if (a == BaristaNarrativeAction.ObserveNeutral) r += 0.25f;
        }

        if (s.creativityTier == 0)
        {
            if (a == BaristaNarrativeAction.ObserveNeutral) r += 0.45f;
            if (a == BaristaNarrativeAction.OfferSafe) r += 0.35f;
            if (a == BaristaNarrativeAction.OfferWeird) r -= 0.40f;
        }
        else if (s.creativityTier == 1)
        {
            if (a == BaristaNarrativeAction.ObserveNeutral) r += 0.20f;
            if (a == BaristaNarrativeAction.RevealHint) r += 0.15f;
        }
        else
        {
            if (a == BaristaNarrativeAction.RevealHint) r += 0.55f;
            if (a == BaristaNarrativeAction.MischievousProbe) r += 0.40f;
            if (a == BaristaNarrativeAction.OfferWeird) r += 0.30f;
            if (a == BaristaNarrativeAction.OfferSafe) r -= 0.20f;
        }

        if (s.relationshipTier == 2)
        {
            if (a == BaristaNarrativeAction.WarmOffer) r += 1.85f;
            if (a == BaristaNarrativeAction.WarmReassure) r += 1.40f;
            if (a == BaristaNarrativeAction.GuardedCheck) r -= 0.65f;
        }
        else if (s.relationshipTier == 0)
        {
            if (a == BaristaNarrativeAction.WarmOffer) r -= 0.95f;
            if (a == BaristaNarrativeAction.WarmReassure) r -= 0.60f;
            if (a == BaristaNarrativeAction.GuardedCheck) r += 0.55f;
            if (a == BaristaNarrativeAction.Deflect) r += 0.30f;
            if (a == BaristaNarrativeAction.MischievousProbe) r += 0.20f;
        }

        if (s.knowledgeTier == 1)
        {
            if (a == BaristaNarrativeAction.RevealHint) r += 1.30f;
            if (a == BaristaNarrativeAction.MischievousProbe) r += 0.55f;
            if (a == BaristaNarrativeAction.Deflect) r += 0.20f;
            if (a == BaristaNarrativeAction.ObserveNeutral) r -= 0.20f;
        }

        if (s.corruptionTier == 0)
        {
            if (a == BaristaNarrativeAction.ObserveNeutral) r += 0.40f;
            if (a == BaristaNarrativeAction.OfferSafe) r += 0.45f;
            if (a == BaristaNarrativeAction.WarmOffer) r += 0.20f;
            if (a == BaristaNarrativeAction.OfferWeird) r -= 1.00f;
            if (a == BaristaNarrativeAction.MischievousProbe) r -= 0.35f;
        }
        else if (s.corruptionTier == 1)
        {
            if (a == BaristaNarrativeAction.MischievousProbe) r += 0.45f;
            if (a == BaristaNarrativeAction.Deflect) r += 0.35f;
            if (a == BaristaNarrativeAction.OfferWeird) r += 0.70f;
            if (a == BaristaNarrativeAction.RevealHint) r += 0.25f;
        }
        else
        {
            if (a == BaristaNarrativeAction.MischievousProbe) r += 1.20f;
            if (a == BaristaNarrativeAction.Deflect) r += 0.85f;
            if (a == BaristaNarrativeAction.OfferWeird) r += 1.35f;
            if (a == BaristaNarrativeAction.RevealHint) r += 0.55f;
            if (a == BaristaNarrativeAction.OfferSafe) r -= 0.50f;
            if (a == BaristaNarrativeAction.WarmOffer) r -= 0.45f;
        }

        return r;
    }

    private static float BaseActionReward(BaristaNarrativeAction action)
    {
        switch (action)
        {
            case BaristaNarrativeAction.ObserveNeutral: return 2.10f;
            case BaristaNarrativeAction.InviteOrder: return 1.45f;
            case BaristaNarrativeAction.GuardedCheck: return 1.55f;
            case BaristaNarrativeAction.OfferSafe: return 1.80f;
            case BaristaNarrativeAction.WarmOffer: return 1.75f;
            case BaristaNarrativeAction.WarmReassure: return 1.65f;
            case BaristaNarrativeAction.MischievousProbe: return 1.35f;
            case BaristaNarrativeAction.OfferWeird: return 1.05f;
            case BaristaNarrativeAction.RevealHint: return 1.45f;
            case BaristaNarrativeAction.Deflect: return 1.10f;
            default: return 0f;
        }
    }

    // ── Tranziții ────────────────────────────────────────────────────────────

    private static List<BaristaStateTransition> EvaluateTransitions(
        BaristaPlanningStateKey state,
        BaristaNarrativeAction action)
    {
        var transitions = new List<BaristaStateTransition>();

        switch (action)
        {
            case BaristaNarrativeAction.WarmOffer:
            case BaristaNarrativeAction.WarmReassure:
                BuildWarmTransitions(state, action, transitions);
                break;

            case BaristaNarrativeAction.MischievousProbe:
            case BaristaNarrativeAction.OfferWeird:
            case BaristaNarrativeAction.RevealHint:
            case BaristaNarrativeAction.Deflect:
                BuildMischievousTransitions(state, action, transitions);
                break;

            default:
                BuildNeutralTransitions(state, action, transitions);
                break;
        }

        NormalizeTransitions(transitions);
        return transitions;
    }

    private static void BuildNeutralTransitions(
        BaristaPlanningStateKey state,
        BaristaNarrativeAction action,
        List<BaristaStateTransition> transitions)
    {
        switch (action)
        {
            case BaristaNarrativeAction.InviteOrder:
                Add(transitions, Mutate(state), 0.70f);
                Add(transitions, Mutate(state, relDelta: +1), 0.20f);
                Add(transitions, Mutate(state, knowledgeTier: 1), 0.10f);
                break;

            case BaristaNarrativeAction.GuardedCheck:
                Add(transitions, Mutate(state), 0.60f);
                Add(transitions, Mutate(state, corruptionDelta: +1), 0.20f);
                Add(transitions, Mutate(state, knowledgeTier: 1), 0.20f);
                break;

            case BaristaNarrativeAction.OfferSafe:
                Add(transitions, Mutate(state), 0.50f);
                Add(transitions, Mutate(state, relDelta: +1), 0.35f);
                Add(transitions, Mutate(state, knowledgeTier: 1), 0.15f);
                break;

            case BaristaNarrativeAction.ObserveNeutral:
            default:
                Add(transitions, Mutate(state), 0.60f);
                Add(transitions, Mutate(state, relDelta: +1), 0.20f);
                Add(transitions, Mutate(state, knowledgeTier: 1), 0.20f);
                break;
        }
    }

    private static void BuildWarmTransitions(
        BaristaPlanningStateKey state,
        BaristaNarrativeAction action,
        List<BaristaStateTransition> transitions)
    {
        switch (action)
        {
            case BaristaNarrativeAction.WarmReassure:
                Add(transitions, Mutate(state, relDelta: +1), 0.70f);
                Add(transitions, Mutate(state), 0.20f);
                Add(transitions, Mutate(state, knowledgeTier: 1, relDelta: +1), 0.10f);
                break;

            case BaristaNarrativeAction.WarmOffer:
            default:
                Add(transitions, Mutate(state, relDelta: +1), 0.65f);
                Add(transitions, Mutate(state, knowledgeTier: 1, relDelta: +1), 0.20f);
                Add(transitions, Mutate(state), 0.15f);
                break;
        }
    }

    private static void BuildMischievousTransitions(
        BaristaPlanningStateKey state,
        BaristaNarrativeAction action,
        List<BaristaStateTransition> transitions)
    {
        switch (action)
        {
            case BaristaNarrativeAction.RevealHint:
                Add(transitions, Mutate(state, knowledgeTier: 1), 0.60f);
                Add(transitions, Mutate(state, knowledgeTier: 1, relDelta: +1), 0.25f);
                Add(transitions, Mutate(state, corruptionDelta: +1), 0.15f);
                break;

            case BaristaNarrativeAction.Deflect:
                Add(transitions, Mutate(state), 0.65f);
                Add(transitions, Mutate(state, corruptionDelta: +1), 0.20f);
                Add(transitions, Mutate(state, knowledgeTier: 1), 0.15f);
                break;

            case BaristaNarrativeAction.OfferWeird:
                Add(transitions, Mutate(state, corruptionDelta: +1), 0.60f);
                Add(transitions, Mutate(state, knowledgeTier: 1, corruptionDelta: +1), 0.25f);
                Add(transitions, Mutate(state), 0.15f);
                break;

            case BaristaNarrativeAction.MischievousProbe:
            default:
                Add(transitions, Mutate(state, corruptionDelta: +1), 0.45f);
                Add(transitions, Mutate(state, knowledgeTier: 1), 0.35f);
                Add(transitions, Mutate(state, knowledgeTier: 1, corruptionDelta: +1), 0.20f);
                break;
        }
    }

    // ── Helpers tranziție ─────────────────────────────────────────────────────

    private static BaristaPlanningStateKey Mutate(
        BaristaPlanningStateKey state,
        int? knowledgeTier = null,
        int corruptionDelta = 0,
        int relDelta = 0)
    {
        return new BaristaPlanningStateKey
        {
            creativityTier = state.creativityTier,
            corruptionTier = Mathf.Clamp(state.corruptionTier + corruptionDelta, 0, 2),
            empathyTier = state.empathyTier,
            relationshipTier = Mathf.Clamp(state.relationshipTier + relDelta, 0, 2),
            knowledgeTier = knowledgeTier.HasValue ? Mathf.Clamp(knowledgeTier.Value, 0, 1) : state.knowledgeTier
        };
    }

    private static void Add(List<BaristaStateTransition> o, BaristaPlanningStateKey s, float p)
        => o.Add(new BaristaStateTransition(s, p));

    private static void NormalizeTransitions(List<BaristaStateTransition> transitions)
    {
        var grouped = new Dictionary<BaristaPlanningStateKey, float>();
        for (int i = 0; i < transitions.Count; i++)
        {
            var t = transitions[i];
            if (!grouped.ContainsKey(t.nextState)) grouped[t.nextState] = 0f;
            grouped[t.nextState] += t.probability;
        }
        transitions.Clear();
        float sum = grouped.Values.Sum();
        if (sum <= 0f) return;
        foreach (var pair in grouped)
            transitions.Add(new BaristaStateTransition(pair.Key, pair.Value / sum));
    }

    private static BaristaNarrativeAction DefaultActionForState(BaristaPlanningStateKey s)
    {
        if (s.corruptionTier >= 2)
            return s.knowledgeTier >= 1 ? BaristaNarrativeAction.RevealHint : BaristaNarrativeAction.OfferWeird;

        if (s.empathyTier >= 2 || s.relationshipTier >= 2)
            return BaristaNarrativeAction.WarmOffer;

        if (s.knowledgeTier >= 1 && s.creativityTier >= 2)
            return BaristaNarrativeAction.RevealHint;

        if (s.relationshipTier == 0)
            return BaristaNarrativeAction.GuardedCheck;

        return s.creativityTier == 0
            ? BaristaNarrativeAction.OfferSafe
            : BaristaNarrativeAction.ObserveNeutral;
    }

    private static float GetBestToneScore(
        Dictionary<BaristaNarrativeAction, float> scores,
        BaristaIntroTone tone)
    {
        float best = float.NegativeInfinity;

        foreach (var pair in scores)
        {
            if (MapActionToTone(pair.Key) != tone)
                continue;

            if (pair.Value > best)
                best = pair.Value;
        }

        return float.IsNegativeInfinity(best) ? 0f : best;
    }

    private static string FormatActionScores(Dictionary<BaristaNarrativeAction, float> scores)
    {
        return string.Join(" | ", scores.OrderByDescending(p => p.Value)
                                        .Select(p => p.Key + "=" + p.Value.ToString("0.000")));
    }

    private readonly struct NpcTonePlannerCacheKey : IEquatable<NpcTonePlannerCacheKey>
    {
        private readonly BaristaPlannerMode plannerMode;
        private readonly float gamma;
        private readonly float epsilon;
        private readonly int maxValueIterations;
        private readonly int maxPolicyIterations;
        private readonly int maxPolicyEvaluationSweeps;

        public NpcTonePlannerCacheKey(BaristaPlannerMode plannerMode, NpcTonePlannerSettings settings)
        {
            this.plannerMode = plannerMode;
            gamma = settings.gamma;
            epsilon = settings.evaluationEpsilon;
            maxValueIterations = settings.maxValueIterations;
            maxPolicyIterations = settings.maxPolicyIterations;
            maxPolicyEvaluationSweeps = settings.maxPolicyEvaluationSweeps;
        }

        public bool Equals(NpcTonePlannerCacheKey other)
        {
            return plannerMode == other.plannerMode &&
                   gamma.Equals(other.gamma) &&
                   epsilon.Equals(other.epsilon) &&
                   maxValueIterations == other.maxValueIterations &&
                   maxPolicyIterations == other.maxPolicyIterations &&
                   maxPolicyEvaluationSweeps == other.maxPolicyEvaluationSweeps;
        }

        public override bool Equals(object obj)
            => obj is NpcTonePlannerCacheKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)plannerMode;
                hash = (hash * 397) ^ gamma.GetHashCode();
                hash = (hash * 397) ^ epsilon.GetHashCode();
                hash = (hash * 397) ^ maxValueIterations;
                hash = (hash * 397) ^ maxPolicyIterations;
                hash = (hash * 397) ^ maxPolicyEvaluationSweeps;
                return hash;
            }
        }
    }

    private sealed class NpcToneCachedPolicy
    {
        public readonly Dictionary<BaristaPlanningStateKey, BaristaNarrativeAction> policy;
        public readonly Dictionary<BaristaPlanningStateKey, float> values;
        public readonly int stateCount;
        public readonly double buildDurationMs;

        public NpcToneCachedPolicy(
            Dictionary<BaristaPlanningStateKey, BaristaNarrativeAction> policy,
            Dictionary<BaristaPlanningStateKey, float> values,
            int stateCount,
            double buildDurationMs)
        {
            this.policy = policy;
            this.values = values;
            this.stateCount = stateCount;
            this.buildDurationMs = buildDurationMs;
        }
    }
}
