// ============================================================
//  BaristaIntroPlanningSolvers  –  ROAE refactor
//  Adăugat: empathyTier + relationshipTier în statekey
//  Adăugat: acțiuni Warm (WarmOffer, WarmReassure)
//  Corectat: MapActionToTone acoperă toate acțiunile
//  Corectat: reward-uri echilibrate pentru Neutral/Warm/Mischievous
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

// ── Runtime state (trimis de Brain spre planner) ────────────────────────────

[Serializable]
public struct BaristaIntroPlanningRuntimeState
{
    public bool readUnknownText;
    public int creativity;
    public int corruption;
    public int empathy;
    public int relationship;          // valoare din BaristaWelcomeKeys.BaristaRelationship
    public bool introDone;
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
            if (introDone)
                return BaristaNarrativePhase.Order;
            return BaristaNarrativePhase.Intro;
        }
    }

    // 0 = scăzut (<=1), 1 = mediu (2-3), 2 = înalt (>=4)
    public int CorruptionTier
    {
        get
        {
            if (corruption <= 1) return 0;
            if (corruption <= 3) return 1;
            return 2;
        }
    }

    // 0 = scazut (<=30), 1 = mediu (31-69), 2 = inalt (>=70)
    public int CreativityTier
    {
        get
        {
            if (creativity <= 30) return 0;
            if (creativity < 70) return 1;
            return 2;
        }
    }

    // 0 = negativ (<0), 1 = neutru (0-1), 2 = înalt (>=2)
    public int EmpathyTier
    {
        get
        {
            if (empathy < 0) return 0;
            if (empathy <= 1) return 1;
            return 2;
        }
    }

    // 0 = ostil (<-2), 1 = neutru (-2..2), 2 = prieten (>2)
    public int RelationshipTier
    {
        get
        {
            if (relationship < -2) return 0;
            if (relationship <= 2) return 1;
            return 2;
        }
    }

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

    public string ToDebugString()
    {
        return "phase=" + Phase +
               " readUnknownText=" + readUnknownText +
               " creativity=" + creativity +
               " creativityTier=" + CreativityTier +
               " corruption=" + corruption +
               " corruptionTier=" + CorruptionTier +
               " empathy=" + empathy +
               " empathyTier=" + EmpathyTier +
               " relationship=" + relationship +
               " relTier=" + RelationshipTier +
               " introDone=" + introDone +
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
    public int phase;
    public int creativityTier;    // 0-2
    public int corruptionTier;    // 0-2
    public int empathyTier;       // 0-2  ← NOU
    public int relationshipTier;  // 0-2  ← NOU
    public int knowledgeTier;     // 0-1
    public int pendingDrinkTier;  // 0-2
    public int heldDrinkTier;     // 0-2

    public bool Equals(BaristaPlanningStateKey o)
    {
        return phase == o.phase &&
               creativityTier == o.creativityTier &&
               corruptionTier == o.corruptionTier &&
               empathyTier == o.empathyTier &&
               relationshipTier == o.relationshipTier &&
               knowledgeTier == o.knowledgeTier &&
               pendingDrinkTier == o.pendingDrinkTier &&
               heldDrinkTier == o.heldDrinkTier;
    }

    public override bool Equals(object obj)
        => obj is BaristaPlanningStateKey o && Equals(o);

    public override int GetHashCode()
    {
        unchecked
        {
            int h = phase;
            h = (h * 397) ^ creativityTier;
            h = (h * 397) ^ corruptionTier;
            h = (h * 397) ^ empathyTier;
            h = (h * 397) ^ relationshipTier;
            h = (h * 397) ^ knowledgeTier;
            h = (h * 397) ^ pendingDrinkTier;
            h = (h * 397) ^ heldDrinkTier;
            return h;
        }
    }

    public string ToDebugString()
    {
        return "phase=" + (BaristaNarrativePhase)phase +
               " creativity=" + creativityTier +
               " corruption=" + corruptionTier +
               " empathy=" + empathyTier +
               " relationship=" + relationshipTier +
               " knowledge=" + knowledgeTier +
               " pendingDrink=" + pendingDrinkTier +
               " heldDrink=" + heldDrinkTier;
    }
}

internal struct BaristaStateTransition
{
    public BaristaPlanningStateKey nextState;
    public float probability;
    public BaristaStateTransition(BaristaPlanningStateKey s, float p) { nextState = s; probability = p; }
}

internal struct BaristaPlannerDecision
{
    public BaristaNarrativeAction bestAction;
    public Dictionary<BaristaNarrativeAction, float> actionScores;
    public Dictionary<BaristaPlanningStateKey, BaristaNarrativeAction> policy;
    public Dictionary<BaristaPlanningStateKey, float> values;
}

public readonly struct BaristaPlannerSettings
{
    public static BaristaPlannerSettings Default =>
        new BaristaPlannerSettings(0.87f, 0.0001f, 96, 24, 96);

    public readonly float gamma;
    public readonly float evaluationEpsilon;
    public readonly int maxValueIterations;
    public readonly int maxPolicyIterations;
    public readonly int maxPolicyEvaluationSweeps;

    public BaristaPlannerSettings(
        float gamma,
        float evaluationEpsilon,
        int maxValueIterations,
        int maxPolicyIterations,
        int maxPolicyEvaluationSweeps)
    {
        this.gamma = Mathf.Clamp(gamma, 0f, 0.99f);
        this.evaluationEpsilon = Mathf.Max(0.00001f, evaluationEpsilon);
        this.maxValueIterations = Mathf.Max(1, maxValueIterations);
        this.maxPolicyIterations = Mathf.Max(1, maxPolicyIterations);
        this.maxPolicyEvaluationSweeps = Mathf.Max(1, maxPolicyEvaluationSweeps);
    }
}

public readonly struct BaristaPlannerEvaluation
{
    public readonly BaristaNarrativeAction bestAction;
    public readonly BaristaIntroTone mappedTone;
    public readonly float neutralScore;
    public readonly float warmScore;
    public readonly float mischievousScore;
    public readonly string actionScoreSummary;

    public BaristaPlannerEvaluation(
        BaristaNarrativeAction bestAction,
        BaristaIntroTone mappedTone,
        float neutralScore,
        float warmScore,
        float mischievousScore,
        string actionScoreSummary)
    {
        this.bestAction = bestAction;
        this.mappedTone = mappedTone;
        this.neutralScore = neutralScore;
        this.warmScore = warmScore;
        this.mischievousScore = mischievousScore;
        this.actionScoreSummary = actionScoreSummary ?? string.Empty;
    }

    public string BuildDebugString()
    {
        return "action=" + bestAction +
               " tone=" + mappedTone +
               " neutral=" + neutralScore.ToString("0.000") +
               " warm=" + warmScore.ToString("0.000") +
               " mischievous=" + mischievousScore.ToString("0.000") +
               " scores={" + actionScoreSummary + "}";
    }
}

// ── Solver principal ─────────────────────────────────────────────────────────

public static class BaristaIntroPlanningSolvers
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

    // ── Intrare publică ───────────────────────────────────────────────────────

    public static BaristaNarrativeAction DecideAction(
        BaristaIntroPlanningRuntimeState runtimeState,
        BaristaPlannerMode plannerMode,
        bool verboseLogs)
    {
        return Evaluate(runtimeState, plannerMode, BaristaPlannerSettings.Default, verboseLogs).bestAction;
    }

    public static BaristaNarrativeAction DecideAction(
        BaristaIntroPlanningRuntimeState runtimeState,
        BaristaPlannerMode plannerMode,
        BaristaPlannerSettings settings,
        bool verboseLogs)
    {
        return Evaluate(runtimeState, plannerMode, settings, verboseLogs).bestAction;
    }

    public static BaristaPlannerEvaluation Evaluate(
        BaristaIntroPlanningRuntimeState runtimeState,
        BaristaPlannerMode plannerMode,
        bool verboseLogs)
    {
        return Evaluate(runtimeState, plannerMode, BaristaPlannerSettings.Default, verboseLogs);
    }

    public static BaristaPlannerEvaluation Evaluate(
        BaristaIntroPlanningRuntimeState runtimeState,
        BaristaPlannerMode plannerMode,
        BaristaPlannerSettings settings,
        bool verboseLogs)
    {
        var stateSpace = GenerateStateSpace();
        var currentState = ToStateKey(runtimeState);

        if (verboseLogs)
            Debug.Log("[ROAE][Planner][" + plannerMode + "][START] states=" + stateSpace.Count +
                      " actions=" + Actions.Length +
                      " currentState={" + currentState.ToDebugString() + "}");

        var decision = plannerMode == BaristaPlannerMode.PolicyIteration
            ? RunPolicyIteration(stateSpace, currentState, settings, verboseLogs)
            : RunValueIteration(stateSpace, currentState, settings, verboseLogs);

        var evaluation = new BaristaPlannerEvaluation(
            decision.bestAction,
            MapActionToTone(decision.bestAction),
            GetBestToneScore(decision.actionScores, BaristaIntroTone.Neutral),
            GetBestToneScore(decision.actionScores, BaristaIntroTone.Warm),
            GetBestToneScore(decision.actionScores, BaristaIntroTone.Mischievous),
            FormatActionScores(decision.actionScores));

        if (verboseLogs)
            Debug.Log("[ROAE][Planner][" + plannerMode + "][RESULT] state={" + currentState.ToDebugString() +
                      "} " + evaluation.BuildDebugString());

        return evaluation;
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

        for (int phase = 0; phase <= (int)BaristaNarrativePhase.AlreadyHasDrink; phase++)
        {
            for (int crea = 0; crea < 3; crea++)
            for (int corr = 0; corr < 3; corr++)
            for (int emp = 0; emp < 3; emp++)
            for (int rel = 0; rel < 3; rel++)
            for (int know = 0; know < 2; know++)
            {
                foreach (var drinkState in EnumerateCanonicalDrinkStates((BaristaNarrativePhase)phase))
                {
                    states.Add(new BaristaPlanningStateKey
                    {
                        phase = phase,
                        creativityTier = crea,
                        corruptionTier = corr,
                        empathyTier = emp,
                        relationshipTier = rel,
                        knowledgeTier = know,
                        pendingDrinkTier = drinkState.pendingDrinkTier,
                        heldDrinkTier = drinkState.heldDrinkTier
                    });
                }
            }
        }

        return states;
    }

    private static BaristaPlanningStateKey ToStateKey(BaristaIntroPlanningRuntimeState rs)
    {
        return new BaristaPlanningStateKey
        {
            phase = (int)rs.Phase,
            creativityTier = rs.CreativityTier,
            corruptionTier = rs.CorruptionTier,
            empathyTier = rs.EmpathyTier,
            relationshipTier = rs.RelationshipTier,
            knowledgeTier = rs.readUnknownText ? 1 : 0,
            pendingDrinkTier = rs.PendingDrinkTier,
            heldDrinkTier = rs.HeldDrinkTier
        };
    }

    private static IEnumerable<(int pendingDrinkTier, int heldDrinkTier)> EnumerateCanonicalDrinkStates(
        BaristaNarrativePhase phase)
    {
        switch (phase)
        {
            case BaristaNarrativePhase.PendingDrinkInitial:
            case BaristaNarrativePhase.PendingDrinkReminder:
                yield return (1, 0);
                yield return (2, 0);
                yield break;

            case BaristaNarrativePhase.AlreadyHasDrink:
                yield return (0, 1);
                yield return (0, 2);
                yield break;

            default:
                yield return (0, 0);
                yield break;
        }
    }

    // ── Value Iteration ──────────────────────────────────────────────────────

    private static BaristaPlannerDecision RunValueIteration(
        List<BaristaPlanningStateKey> states,
        BaristaPlanningStateKey currentState,
        BaristaPlannerSettings settings,
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
        return new BaristaPlannerDecision
        {
            bestAction = policy[currentState],
            actionScores = ComputeActionScores(currentState, values, settings),
            policy = policy,
            values = values
        };
    }

    // ── Policy Iteration ─────────────────────────────────────────────────────

    private static BaristaPlannerDecision RunPolicyIteration(
        List<BaristaPlanningStateKey> states,
        BaristaPlanningStateKey currentState,
        BaristaPlannerSettings settings,
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

        return new BaristaPlannerDecision
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
        BaristaPlannerSettings settings)
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
        BaristaPlannerSettings settings)
    {
        var scores = new Dictionary<BaristaNarrativeAction, float>();
        foreach (var a in Actions) scores[a] = ComputeActionValue(state, a, values, settings);
        return scores;
    }

    private static float ComputeActionValue(
        BaristaPlanningStateKey state,
        BaristaNarrativeAction action,
        Dictionary<BaristaPlanningStateKey, float> values,
        BaristaPlannerSettings settings)
    {
        float reward = EvaluateReward(state, action);
        var transitions = EvaluateTransitions(state, action);
        float future = 0f;
        for (int i = 0; i < transitions.Count; i++)
            future += transitions[i].probability * values[transitions[i].nextState];
        return reward + settings.gamma * future;
    }

    // ── Reward function ──────────────────────────────────────────────────────
    //
    //  Logica principală:
    //  • Empathy ridicat   → bonus Warm (WarmOffer, WarmReassure)
    //  • Relationship > 2  → bonus Warm adițional
    //  • Corruption ridicat / readUnknownText → bonus Mischievous
    //  • Low corruption + low empathy → bias Neutral
    //

    private static float EvaluateReward(BaristaPlanningStateKey s, BaristaNarrativeAction a)
    {
        float r = 0f;

        // 1. Recompensă de bază per fază
        switch ((BaristaNarrativePhase)s.phase)
        {
            case BaristaNarrativePhase.Intro: r += RewardIntro(a); break;
            case BaristaNarrativePhase.Order: r += RewardOrder(a); break;
            case BaristaNarrativePhase.PendingDrinkInitial: r += RewardPendingInitial(a); break;
            case BaristaNarrativePhase.PendingDrinkReminder: r += RewardPendingReminder(a); break;
            case BaristaNarrativePhase.AlreadyHasDrink: r += RewardAlreadyHasDrink(a); break;
        }

        // 2. Empathy tier → favorizează Warm
        if (s.empathyTier == 2)                         // empathy ridicat
        {
            if (a == BaristaNarrativeAction.WarmOffer) r += 2.20f;
            if (a == BaristaNarrativeAction.WarmReassure) r += 1.80f;
            if (a == BaristaNarrativeAction.ObserveNeutral) r -= 0.60f;
            if (a == BaristaNarrativeAction.MischievousProbe) r -= 0.30f;
        }
        else if (s.empathyTier == 1)                    // empathy neutru
        {
            if (a == BaristaNarrativeAction.ObserveNeutral) r += 0.35f;
            if (a == BaristaNarrativeAction.WarmOffer) r += 0.10f;
            if (a == BaristaNarrativeAction.WarmReassure) r += 0.05f;
        }
        else                                            // empathy negativ
        {
            if (a == BaristaNarrativeAction.WarmOffer) r -= 1.10f;
            if (a == BaristaNarrativeAction.WarmReassure) r -= 0.90f;
            if (a == BaristaNarrativeAction.ObserveNeutral) r += 0.30f;
        }

        // 2b. Creativity tier
        if (s.creativityTier == 0)
        {
            if (a == BaristaNarrativeAction.ObserveNeutral) r += 0.40f;
            if (a == BaristaNarrativeAction.OfferSafe) r += 0.25f;
        }
        else if (s.creativityTier == 1)
        {
            if (a == BaristaNarrativeAction.ObserveNeutral) r += 0.25f;
            if (a == BaristaNarrativeAction.RevealHint) r += 0.10f;
        }
        else
        {
            if (a == BaristaNarrativeAction.RevealHint) r += 0.45f;
            if (a == BaristaNarrativeAction.MischievousProbe) r += 0.35f;
            if (a == BaristaNarrativeAction.OfferWeird) r += 0.25f;
        }

        // 3. Relationship tier → întărește Warm dacă relație bună
        if (s.relationshipTier == 2)                    // prieten
        {
            if (a == BaristaNarrativeAction.WarmOffer) r += 2.20f;
            if (a == BaristaNarrativeAction.WarmReassure) r += 1.60f;
            if (a == BaristaNarrativeAction.ObserveNeutral) r -= 0.35f;
            if (a == BaristaNarrativeAction.GuardedCheck) r -= 0.70f;
        }
        else if (s.relationshipTier == 0)               // ostil
        {
            if (a == BaristaNarrativeAction.WarmOffer) r -= 0.80f;
            if (a == BaristaNarrativeAction.GuardedCheck) r += 0.50f;
            if (a == BaristaNarrativeAction.MischievousProbe) r += 0.30f;
        }

        // 4. Knowledge tier → favorizează Mischievous / RevealHint
        if (s.knowledgeTier == 1)
        {
            if (a == BaristaNarrativeAction.RevealHint) r += 1.40f;
            if (a == BaristaNarrativeAction.MischievousProbe) r += 0.65f;
            if (a == BaristaNarrativeAction.ObserveNeutral) r -= 0.25f;
        }

        // 5. Corruption tier
        if (s.corruptionTier == 0)                      // low corruption → Neutral/Warm
        {
            if (a == BaristaNarrativeAction.ObserveNeutral) r += 0.45f;
            if (a == BaristaNarrativeAction.OfferSafe) r += 0.45f;
            if (a == BaristaNarrativeAction.OfferWeird) r -= 1.10f;
            if (a == BaristaNarrativeAction.MischievousProbe) r -= 0.40f;
        }
        else if (s.corruptionTier == 1)
        {
            if (a == BaristaNarrativeAction.MischievousProbe) r += 0.45f;
            if (a == BaristaNarrativeAction.Deflect) r += 0.35f;
            if (a == BaristaNarrativeAction.OfferWeird) r += 0.75f;
        }
        else                                            // high corruption → Mischievous
        {
            if (a == BaristaNarrativeAction.MischievousProbe) r += 1.10f;
            if (a == BaristaNarrativeAction.Deflect) r += 0.80f;
            if (a == BaristaNarrativeAction.OfferWeird) r += 1.30f;
            if (a == BaristaNarrativeAction.OfferSafe) r -= 0.50f;
            if (a == BaristaNarrativeAction.WarmOffer) r -= 0.40f;
        }

        // 6. Pending + held drink state
        if (s.pendingDrinkTier != 0)
        {
            if (a == BaristaNarrativeAction.InviteOrder) r -= 2.25f;
            if (a == BaristaNarrativeAction.OfferSafe) r -= 1.90f;
            if (a == BaristaNarrativeAction.OfferWeird) r -= 2.10f;
            if (a == BaristaNarrativeAction.WarmOffer) r -= 1.75f;
            if (a == BaristaNarrativeAction.RevealHint) r += 0.75f;
            if (a == BaristaNarrativeAction.Deflect) r += 0.55f;
        }
        if (s.pendingDrinkTier == 2)
        {
            if (a == BaristaNarrativeAction.MischievousProbe) r += 0.40f;
            if (a == BaristaNarrativeAction.RevealHint) r += 0.25f;
        }
        if (s.heldDrinkTier == 2)
        {
            if (a == BaristaNarrativeAction.Deflect) r += 1.00f;
            if (a == BaristaNarrativeAction.RevealHint) r += 0.50f;
        }
        if (s.heldDrinkTier == 1)
        {
            if (a == BaristaNarrativeAction.InviteOrder) r -= 1.20f;
            if (a == BaristaNarrativeAction.OfferSafe) r -= 1.40f;
            if (a == BaristaNarrativeAction.OfferWeird) r -= 1.60f;
            if (a == BaristaNarrativeAction.WarmOffer) r -= 1.40f;
        }

        if (s.phase == (int)BaristaNarrativePhase.Intro &&
            s.creativityTier == 1 &&
            s.corruptionTier == 0 &&
            s.empathyTier == 1 &&
            s.relationshipTier == 1 &&
            s.knowledgeTier == 0)
        {
            if (a == BaristaNarrativeAction.ObserveNeutral) r += 1.10f;
            if (a == BaristaNarrativeAction.InviteOrder) r += 0.25f;
            if (a == BaristaNarrativeAction.WarmOffer) r -= 0.55f;
        }

        return r;
    }

    // Recompense de bază per fază (acțiunile Warm au baseline pozitiv în Intro/Order)
    private static float RewardIntro(BaristaNarrativeAction a)
    {
        switch (a)
        {
            case BaristaNarrativeAction.ObserveNeutral: return 2.40f;
            case BaristaNarrativeAction.InviteOrder: return 1.80f;
            case BaristaNarrativeAction.GuardedCheck: return 1.10f;
            case BaristaNarrativeAction.OfferSafe: return 0.10f;
            case BaristaNarrativeAction.WarmOffer: return 1.35f;
            case BaristaNarrativeAction.WarmReassure: return 1.20f;
            case BaristaNarrativeAction.MischievousProbe: return 1.50f;
            case BaristaNarrativeAction.OfferWeird: return -0.90f;
            case BaristaNarrativeAction.RevealHint: return 1.20f;
            case BaristaNarrativeAction.Deflect: return 0.70f;
            default: return 0f;
        }
    }

    private static float RewardOrder(BaristaNarrativeAction a)
    {
        switch (a)
        {
            case BaristaNarrativeAction.ObserveNeutral: return 0.60f;
            case BaristaNarrativeAction.InviteOrder: return 1.10f;
            case BaristaNarrativeAction.GuardedCheck: return 1.20f;
            case BaristaNarrativeAction.OfferSafe: return 2.60f;
            case BaristaNarrativeAction.WarmOffer: return 2.40f;
            case BaristaNarrativeAction.WarmReassure: return 1.80f;
            case BaristaNarrativeAction.MischievousProbe: return 1.50f;
            case BaristaNarrativeAction.OfferWeird: return 2.80f;
            case BaristaNarrativeAction.RevealHint: return 1.85f;
            case BaristaNarrativeAction.Deflect: return 1.35f;
            default: return 0f;
        }
    }

    private static float RewardPendingInitial(BaristaNarrativeAction a)
    {
        switch (a)
        {
            case BaristaNarrativeAction.ObserveNeutral: return 1.60f;
            case BaristaNarrativeAction.InviteOrder: return -1.50f;
            case BaristaNarrativeAction.GuardedCheck: return 1.20f;
            case BaristaNarrativeAction.OfferSafe: return -1.75f;
            case BaristaNarrativeAction.WarmOffer: return -1.40f;
            case BaristaNarrativeAction.WarmReassure: return 2.10f;
            case BaristaNarrativeAction.MischievousProbe: return 1.30f;
            case BaristaNarrativeAction.OfferWeird: return -1.90f;
            case BaristaNarrativeAction.RevealHint: return 1.55f;
            case BaristaNarrativeAction.Deflect: return 1.85f;
            default: return 0f;
        }
    }

    private static float RewardPendingReminder(BaristaNarrativeAction a)
    {
        switch (a)
        {
            case BaristaNarrativeAction.ObserveNeutral: return 1.20f;
            case BaristaNarrativeAction.InviteOrder: return -2.10f;
            case BaristaNarrativeAction.GuardedCheck: return 1.35f;
            case BaristaNarrativeAction.OfferSafe: return -1.95f;
            case BaristaNarrativeAction.WarmOffer: return -1.65f;
            case BaristaNarrativeAction.WarmReassure: return 1.85f;
            case BaristaNarrativeAction.MischievousProbe: return 1.45f;
            case BaristaNarrativeAction.OfferWeird: return -2.15f;
            case BaristaNarrativeAction.RevealHint: return 1.65f;
            case BaristaNarrativeAction.Deflect: return 2.10f;
            default: return 0f;
        }
    }

    private static float RewardAlreadyHasDrink(BaristaNarrativeAction a)
    {
        switch (a)
        {
            case BaristaNarrativeAction.ObserveNeutral: return 0.20f;
            case BaristaNarrativeAction.InviteOrder: return -2.25f;
            case BaristaNarrativeAction.GuardedCheck: return 1.80f;
            case BaristaNarrativeAction.OfferSafe: return -2.50f;
            case BaristaNarrativeAction.WarmOffer: return -2.20f;
            case BaristaNarrativeAction.WarmReassure: return 1.40f;
            case BaristaNarrativeAction.MischievousProbe: return 1.40f;
            case BaristaNarrativeAction.OfferWeird: return -2.80f;
            case BaristaNarrativeAction.RevealHint: return 1.75f;
            case BaristaNarrativeAction.Deflect: return 2.35f;
            default: return 0f;
        }
    }

    // ── Tranziții ────────────────────────────────────────────────────────────

    private static List<BaristaStateTransition> EvaluateTransitions(
        BaristaPlanningStateKey state, BaristaNarrativeAction action)
    {
        var transitions = new List<BaristaStateTransition>();
        switch ((BaristaNarrativePhase)state.phase)
        {
            case BaristaNarrativePhase.Intro: BuildIntroTransitions(state, action, transitions); break;
            case BaristaNarrativePhase.Order: BuildOrderTransitions(state, action, transitions); break;
            case BaristaNarrativePhase.PendingDrinkInitial: BuildPendingInitialTransitions(state, action, transitions); break;
            case BaristaNarrativePhase.PendingDrinkReminder: BuildPendingReminderTransitions(state, action, transitions); break;
            case BaristaNarrativePhase.AlreadyHasDrink: BuildAlreadyHasDrinkTransitions(state, action, transitions); break;
        }
        NormalizeTransitions(transitions);
        return transitions;
    }

    private static void BuildIntroTransitions(
        BaristaPlanningStateKey state, BaristaNarrativeAction action, List<BaristaStateTransition> o)
    {
        switch (action)
        {
            case BaristaNarrativeAction.ObserveNeutral:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Intro), 0.55f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order), 0.45f);
                break;
            case BaristaNarrativeAction.InviteOrder:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order), 0.80f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Intro), 0.20f);
                break;
            case BaristaNarrativeAction.GuardedCheck:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Intro), 0.45f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order), 0.45f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Intro, corruptionDelta: +1), 0.10f);
                break;
            case BaristaNarrativeAction.WarmOffer:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, relDelta: +1), 0.65f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1, relDelta: +1), 0.25f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Intro), 0.10f);
                break;
            case BaristaNarrativeAction.WarmReassure:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Intro, relDelta: +1), 0.50f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, relDelta: +1), 0.50f);
                break;
            case BaristaNarrativeAction.MischievousProbe:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order), 0.50f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, corruptionDelta: +1), 0.35f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Intro, knowledgeTier: 1), 0.15f);
                break;
            case BaristaNarrativeAction.OfferSafe:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, relDelta: +1), 0.55f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1), 0.15f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order), 0.30f);
                break;
            case BaristaNarrativeAction.OfferWeird:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, corruptionDelta: +1), 0.60f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1, corruptionDelta: +1), 0.25f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Intro), 0.15f);
                break;
            case BaristaNarrativeAction.RevealHint:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1), 0.65f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Intro, knowledgeTier: 1), 0.35f);
                break;
            case BaristaNarrativeAction.Deflect:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Intro), 0.50f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order), 0.35f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Intro, corruptionDelta: +1), 0.15f);
                break;
        }
    }

    private static void BuildOrderTransitions(
        BaristaPlanningStateKey state, BaristaNarrativeAction action, List<BaristaStateTransition> o)
    {
        switch (action)
        {
            case BaristaNarrativeAction.ObserveNeutral:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order), 0.60f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, relDelta: +1), 0.15f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1), 0.25f);
                break;
            case BaristaNarrativeAction.InviteOrder:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order), 0.85f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, relDelta: +1), 0.15f);
                break;
            case BaristaNarrativeAction.GuardedCheck:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order), 0.65f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, corruptionDelta: +1), 0.20f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1), 0.15f);
                break;
            case BaristaNarrativeAction.WarmOffer:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, relDelta: +1), 0.75f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, relDelta: +1), 0.25f);
                break;
            case BaristaNarrativeAction.WarmReassure:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, relDelta: +1), 0.55f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1, relDelta: +1), 0.20f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order), 0.25f);
                break;
            case BaristaNarrativeAction.MischievousProbe:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, corruptionDelta: +1), 0.45f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1, corruptionDelta: +1), 0.35f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1), 0.15f);
                break;
            case BaristaNarrativeAction.OfferSafe:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, relDelta: +1), 0.45f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1), 0.20f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order), 0.35f);
                break;
            case BaristaNarrativeAction.OfferWeird:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, corruptionDelta: +1), 0.55f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1, corruptionDelta: +1), 0.25f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order), 0.20f);
                break;
            case BaristaNarrativeAction.RevealHint:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1), 0.55f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1, relDelta: +1), 0.25f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order), 0.20f);
                break;
            case BaristaNarrativeAction.Deflect:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order), 0.60f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, corruptionDelta: +1), 0.25f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1), 0.15f);
                break;
        }
    }

    private static void BuildPendingInitialTransitions(
        BaristaPlanningStateKey state, BaristaNarrativeAction action, List<BaristaStateTransition> o)
    {
        switch (action)
        {
            case BaristaNarrativeAction.WarmReassure:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkReminder, relDelta: +1), 0.80f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkInitial, relDelta: +1), 0.20f);
                break;
            case BaristaNarrativeAction.RevealHint:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkReminder, knowledgeTier: 1), 0.70f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkInitial, knowledgeTier: 1), 0.30f);
                break;
            case BaristaNarrativeAction.Deflect:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkReminder), 0.85f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkInitial), 0.15f);
                break;
            case BaristaNarrativeAction.MischievousProbe:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkReminder, corruptionDelta: +1), 0.65f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkInitial, corruptionDelta: +1), 0.35f);
                break;
            default:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkReminder), 0.80f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkInitial), 0.20f);
                break;
        }
    }

    private static void BuildPendingReminderTransitions(
        BaristaPlanningStateKey state, BaristaNarrativeAction action, List<BaristaStateTransition> o)
    {
        switch (action)
        {
            case BaristaNarrativeAction.RevealHint:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkReminder, knowledgeTier: 1), 0.80f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkReminder, knowledgeTier: 1, corruptionDelta: +1), 0.20f);
                break;
            case BaristaNarrativeAction.WarmReassure:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkReminder, relDelta: +1), 0.85f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkReminder), 0.15f);
                break;
            case BaristaNarrativeAction.Deflect:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkReminder), 0.90f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkReminder, corruptionDelta: +1), 0.10f);
                break;
            case BaristaNarrativeAction.MischievousProbe:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkReminder, corruptionDelta: +1), 0.60f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkReminder, knowledgeTier: 1), 0.40f);
                break;
            default:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.PendingDrinkReminder), 1.0f);
                break;
        }
    }

    private static void BuildAlreadyHasDrinkTransitions(
        BaristaPlanningStateKey state, BaristaNarrativeAction action, List<BaristaStateTransition> o)
    {
        switch (action)
        {
            case BaristaNarrativeAction.RevealHint:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, knowledgeTier: 1), 0.85f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, knowledgeTier: 1, corruptionDelta: +1), 0.15f);
                break;
            case BaristaNarrativeAction.WarmReassure:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, relDelta: +1), 0.90f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink), 0.10f);
                break;
            case BaristaNarrativeAction.Deflect:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink), 0.90f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, corruptionDelta: +1), 0.10f);
                break;
            case BaristaNarrativeAction.MischievousProbe:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, corruptionDelta: +1), 0.60f);
                Add(o, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, knowledgeTier: 1), 0.40f);
                break;
            default:
                Add(o, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink), 1.0f);
                break;
        }
    }

    // ── Helpers tranziție ─────────────────────────────────────────────────────

    private static BaristaPlanningStateKey Mutate(
        BaristaPlanningStateKey state,
        BaristaNarrativePhase? phase = null,
        int? knowledgeTier = null,
        int? pendingDrinkTier = null,
        int? heldDrinkTier = null,
        int corruptionDelta = 0,
        int relDelta = 0)
    {
        return new BaristaPlanningStateKey
        {
            phase = phase.HasValue ? (int)phase.Value : state.phase,
            creativityTier = state.creativityTier,
            corruptionTier = Mathf.Clamp(state.corruptionTier + corruptionDelta, 0, 2),
            empathyTier = state.empathyTier,                              // empathy nu se schimbă în tranziții MDP
            relationshipTier = Mathf.Clamp(state.relationshipTier + relDelta, 0, 2),
            knowledgeTier = knowledgeTier.HasValue ? Mathf.Clamp(knowledgeTier.Value, 0, 1) : state.knowledgeTier,
            pendingDrinkTier = pendingDrinkTier.HasValue ? Mathf.Clamp(pendingDrinkTier.Value, 0, 2) : state.pendingDrinkTier,
            heldDrinkTier = heldDrinkTier.HasValue ? Mathf.Clamp(heldDrinkTier.Value, 0, 2) : state.heldDrinkTier
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
        var phase = (BaristaNarrativePhase)s.phase;
        switch (phase)
        {
            case BaristaNarrativePhase.Intro:
                if (s.corruptionTier >= 2) return BaristaNarrativeAction.MischievousProbe;
                if (s.empathyTier >= 2 || s.relationshipTier >= 2) return BaristaNarrativeAction.WarmOffer;
                return BaristaNarrativeAction.ObserveNeutral;

            case BaristaNarrativePhase.Order:
                if (s.corruptionTier >= 2) return BaristaNarrativeAction.OfferWeird;
                if (s.empathyTier >= 2 || s.relationshipTier >= 2) return BaristaNarrativeAction.WarmOffer;
                return BaristaNarrativeAction.OfferSafe;

            case BaristaNarrativePhase.PendingDrinkInitial:
                if (s.corruptionTier >= 2 || s.pendingDrinkTier >= 2) return BaristaNarrativeAction.RevealHint;
                if (s.empathyTier >= 2 || s.relationshipTier >= 2) return BaristaNarrativeAction.WarmReassure;
                return BaristaNarrativeAction.ObserveNeutral;

            case BaristaNarrativePhase.PendingDrinkReminder:
                if (s.corruptionTier >= 2 || s.pendingDrinkTier >= 2) return BaristaNarrativeAction.Deflect;
                if (s.empathyTier >= 2 || s.relationshipTier >= 2) return BaristaNarrativeAction.WarmReassure;
                return BaristaNarrativeAction.ObserveNeutral;

            case BaristaNarrativePhase.AlreadyHasDrink:
            default:
                return BaristaNarrativeAction.Deflect;
        }
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
}
