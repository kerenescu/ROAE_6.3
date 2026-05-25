using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public readonly struct CompanionEmotionResult
{
    public CompanionEmotionResult(
        CompanionEmotionalState state,
        int score,
        CompanionAxisScores axes,
        string debugSummary = "",
        string debugKey = "")
    {
        State = state;
        Score = score;
        Axes = axes;
        DebugSummary = debugSummary ?? string.Empty;
        DebugKey = debugKey ?? string.Empty;
    }

    public CompanionEmotionalState State { get; }
    public int Score { get; }
    public CompanionAxisScores Axes { get; }
    public string DebugSummary { get; }
    public string DebugKey { get; }
}

public readonly struct CompanionDialogueResult
{
    public CompanionDialogueResult(CompanionDialogueEntry entry, int score)
    {
        Entry = entry;
        Score = score;
    }

    public CompanionDialogueEntry Entry { get; }
    public int Score { get; }
}

internal enum CompanionPlannerAction
{
    SeekWarmth = 0,
    Lament = 1,
    Withdraw = 2,
    Harden = 3
}

internal readonly struct CompanionPlannerEvaluation
{
    public CompanionPlannerEvaluation(
        CompanionEmotionalState recommendedState,
        CompanionAxisScores axes,
        CompanionPlannerAction chosenAction,
        float[] actionValues,
        CompanionSocialSignalSnapshot social,
        CompanionPlannerSolverStats solverStats,
        string inputMode,
        int[] policy)
    {
        RecommendedState = recommendedState;
        Axes = axes;
        ChosenAction = chosenAction;
        ActionValues = actionValues;
        Social = social;
        SolverStats = solverStats;
        InputMode = inputMode ?? string.Empty;
        Policy = policy ?? new int[PlannerStatesCount];
    }

    private const int PlannerStatesCount = 4;

    public CompanionEmotionalState RecommendedState { get; }
    public CompanionAxisScores Axes { get; }
    public CompanionPlannerAction ChosenAction { get; }
    public float[] ActionValues { get; }
    public CompanionSocialSignalSnapshot Social { get; }
    public CompanionPlannerSolverStats SolverStats { get; }
    public string InputMode { get; }
    public int[] Policy { get; }
}

internal readonly struct CompanionPlannerSolverStats
{
    public CompanionPlannerSolverStats(
        CompanionPlannerMode mode,
        int iterations,
        int evaluationSweeps,
        float finalDelta,
        bool converged,
        double durationMs)
    {
        Mode = mode;
        Iterations = iterations;
        EvaluationSweeps = evaluationSweeps;
        FinalDelta = finalDelta;
        Converged = converged;
        DurationMs = durationMs;
    }

    public CompanionPlannerMode Mode { get; }
    public int Iterations { get; }
    public int EvaluationSweeps { get; }
    public float FinalDelta { get; }
    public bool Converged { get; }
    public double DurationMs { get; }
}

internal readonly struct CompanionSocialSignalSnapshot
{
    public CompanionSocialSignalSnapshot(float warm, float neutral, float mischievous)
    {
        Warm = Mathf.Clamp(warm, 0f, 100f);
        Neutral = Mathf.Clamp(neutral, 0f, 100f);
        Mischievous = Mathf.Clamp(mischievous, 0f, 100f);
    }

    public float Warm { get; }
    public float Neutral { get; }
    public float Mischievous { get; }
}

public static class CompanionEmotionResolver
{
    private const float PlannerConvergenceEpsilon = 0.0001f;
    private static readonly CompanionEmotionalState[] PlannerStates =
    {
        CompanionEmotionalState.Healthy,
        CompanionEmotionalState.Sad,
        CompanionEmotionalState.Numb,
        CompanionEmotionalState.Malicious
    };

    public static CompanionEmotionResult Resolve(
        CompanionProfile profile,
        CompanionEvaluationContext context)
    {
        if (profile == null)
            return new CompanionEmotionResult(
                CompanionEmotionalState.Numb,
                0,
                new CompanionAxisScores(0, 0, 0, 0));

        CompanionContextSnapshot snapshot = context != null ? context.Snapshot : null;
        CompanionAxisScores axes;
        CompanionEmotionalState bestState;

        if (profile.useSocialPlanner)
        {
            CompanionPlannerEvaluation evaluation = EvaluatePlanner(profile, snapshot);
            axes = evaluation.Axes;
            bestState = evaluation.RecommendedState;
            string debugSummary = BuildPlannerDebugSummary(snapshot, evaluation);
            string debugKey = BuildPlannerDebugKey(snapshot, evaluation);

            int plannerBestScore = axes.GetIntensity(bestState);
            if (profile.emotionalUnpredictability > 0f && Random.value < profile.emotionalUnpredictability)
            {
                CompanionEmotionalState alt = PickAlternative(bestState);
                if (alt != bestState)
                    return new CompanionEmotionResult(
                        alt,
                        Mathf.Max(0, axes.GetIntensity(alt) - 1),
                        axes,
                        debugSummary,
                        debugKey);
            }

            return new CompanionEmotionResult(
                bestState,
                Mathf.Max(0, plannerBestScore),
                axes,
                debugSummary,
                debugKey);
        }
        else
        {
            axes = ComputeStatOnlyAxes(profile, snapshot);
            bestState = axes.GetDominantState();
        }

        int bestScore = axes.GetIntensity(bestState);

        if (profile.emotionalUnpredictability > 0f && Random.value < profile.emotionalUnpredictability)
        {
            CompanionEmotionalState alt = PickAlternative(bestState);
            if (alt != bestState)
                return new CompanionEmotionResult(alt, Mathf.Max(0, axes.GetIntensity(alt) - 1), axes);
        }

        return new CompanionEmotionResult(bestState, Mathf.Max(0, bestScore), axes);
    }

    private static CompanionPlannerEvaluation EvaluatePlanner(CompanionProfile profile, CompanionContextSnapshot snapshot)
    {
        if (profile == null || snapshot == null)
            return new CompanionPlannerEvaluation(
                CompanionEmotionalState.Numb,
                new CompanionAxisScores(0, 0, 0, 0),
                CompanionPlannerAction.Withdraw,
                new float[PlannerStates.Length],
                new CompanionSocialSignalSnapshot(0f, 0f, 0f),
                new CompanionPlannerSolverStats(
                    CompanionPlannerMode.PolicyIteration,
                    0,
                    0,
                    0f,
                    false,
                    0d),
                "Unavailable",
                new int[PlannerStates.Length]);

        bool useNpcSignalsOnly = profile.socialPlannerUsesNpcSignalsOnly;
        CompanionSocialSignalSnapshot social = ResolveSocialSignals(snapshot, useNpcSignalsOnly);
        CompanionAxisScores axes = ComputeAxes(profile, snapshot, social);
        float[,] rewards = BuildRewardTable(snapshot, social, axes);
        float[,,] transitions = BuildTransitionTable(snapshot, social, axes);
        float gamma = Mathf.Clamp(profile.plannerDiscount, 0.1f, 0.95f);
        Stopwatch stopwatch = Stopwatch.StartNew();
        CompanionPlannerSolverStats solverStats;
        int[] policy = profile.plannerMode == CompanionPlannerMode.PolicyIteration
            ? SolvePolicyIteration(rewards, transitions, gamma, out solverStats)
            : SolveValueIterationPolicy(rewards, transitions, gamma, out solverStats);
        stopwatch.Stop();
        solverStats = new CompanionPlannerSolverStats(
            solverStats.Mode,
            solverStats.Iterations,
            solverStats.EvaluationSweeps,
            solverStats.FinalDelta,
            solverStats.Converged,
            stopwatch.Elapsed.TotalMilliseconds);

        int stateIndex = EmotionToPlannerIndex(CompanionEmotionStateUtility.Normalize(snapshot.currentEmotion));
        int actionIndex = Mathf.Clamp(policy[stateIndex], 0, PlannerStates.Length - 1);
        float[] actionValues = ComputeActionValues(rewards, transitions, gamma, stateIndex);
        CompanionEmotionalState recommendedState = ResolveRecommendedState(
            profile,
            snapshot.currentEmotion,
            axes,
            actionIndex,
            actionValues);

        return new CompanionPlannerEvaluation(
            recommendedState,
            axes,
            (CompanionPlannerAction)actionIndex,
            actionValues,
            social,
            solverStats,
            useNpcSignalsOnly ? "NpcSignalsOnly" : "StatsPlusNpcSignals",
            policy);
    }

    private static CompanionAxisScores ComputeAxes(
        CompanionProfile profile,
        CompanionContextSnapshot snapshot,
        CompanionSocialSignalSnapshot social)
    {
        if (profile == null || snapshot == null)
            return new CompanionAxisScores(0, 0, 0, 0);

        if (profile.socialPlannerUsesNpcSignalsOnly)
        {
            int healthyFromSignals = Mathf.RoundToInt(social.Warm);
            int sadFromSignals = Mathf.RoundToInt(social.Neutral * 0.78f);
            int numbFromSignals = Mathf.RoundToInt(social.Neutral * 0.92f);
            int maliciousFromSignals = Mathf.RoundToInt(social.Mischievous);
            return new CompanionAxisScores(
                numbFromSignals,
                sadFromSignals,
                maliciousFromSignals,
                healthyFromSignals);
        }

        int lowCreativity = 100 - snapshot.creativity;
        int lowEmpathy = 100 - snapshot.empathy;
        int lowCorruption = 100 - snapshot.corruption;

        float healthyStat = (snapshot.creativity + snapshot.empathy + lowCorruption) / 3f;
        float sadStat = (lowCreativity + lowEmpathy) * 0.5f;
        float numbStat = lowCreativity;
        float maliciousStat = snapshot.corruption;

        int healthy = Mathf.RoundToInt((0.55f * social.Warm) + (0.45f * healthyStat));
        int sad = Mathf.RoundToInt((0.40f * social.Neutral) + (0.60f * sadStat));
        int numb = Mathf.RoundToInt((0.60f * social.Neutral) + (0.40f * numbStat));
        int malicious = Mathf.RoundToInt((0.55f * social.Mischievous) + (0.45f * maliciousStat));

        if (snapshot.corruption >= profile.maliciousCorruptionOverrideThreshold)
            malicious = 100;

        return new CompanionAxisScores(numb, sad, malicious, healthy);
    }

    private static CompanionAxisScores ComputeStatOnlyAxes(CompanionProfile profile, CompanionContextSnapshot snapshot)
    {
        if (profile == null || snapshot == null)
            return new CompanionAxisScores(0, 0, 0, 0);

        int lowCreativity = 100 - snapshot.creativity;
        int lowEmpathy = 100 - snapshot.empathy;
        int lowCorruption = 100 - snapshot.corruption;

        int numb = lowCreativity;
        int sad = Mathf.RoundToInt((lowCreativity + lowEmpathy) * 0.5f);
        int malicious = snapshot.corruption;
        int healthy = Mathf.RoundToInt((snapshot.creativity + snapshot.empathy + lowCorruption) / 3f);

        if (snapshot.corruption >= profile.maliciousCorruptionOverrideThreshold)
            malicious = 100;

        return new CompanionAxisScores(numb, sad, malicious, healthy);
    }

    private static CompanionSocialSignalSnapshot ResolveSocialSignals(
        CompanionContextSnapshot snapshot,
        bool useNpcSignalsOnly)
    {
        if (snapshot == null)
            return new CompanionSocialSignalSnapshot(0f, 0f, 0f);

        float warm = snapshot.warmSignal;
        float neutral = snapshot.neutralSignal;
        float mischievous = snapshot.mischievousSignal;

        if (warm <= 0.01f && neutral <= 0.01f && mischievous <= 0.01f)
        {
            if (useNpcSignalsOnly)
                return new CompanionSocialSignalSnapshot(33.34f, 33.33f, 33.33f);

            int lowCreativity = 100 - snapshot.creativity;
            int lowEmpathy = 100 - snapshot.empathy;
            int lowCorruption = 100 - snapshot.corruption;

            warm = (snapshot.creativity + snapshot.empathy + lowCorruption) / 3f;
            neutral = (lowCreativity + lowEmpathy) * 0.5f;
            mischievous = snapshot.corruption;
        }

        float total = warm + neutral + mischievous;
        if (total <= 0.01f)
            return new CompanionSocialSignalSnapshot(33.34f, 33.33f, 33.33f);

        float scale = 100f / total;
        return new CompanionSocialSignalSnapshot(warm * scale, neutral * scale, mischievous * scale);
    }

    private static float[,] BuildRewardTable(
        CompanionContextSnapshot snapshot,
        CompanionSocialSignalSnapshot social,
        CompanionAxisScores axes)
    {
        float[,] rewards = new float[PlannerStates.Length, PlannerStates.Length];

        float healthyReward = (axes.Healthy / 100f) * 2.75f + (social.Warm / 100f) * 0.25f - (social.Mischievous / 100f) * 0.65f;
        float sadReward = (axes.Sad / 100f) * 2.0f + (social.Neutral / 100f) * 0.2f - (social.Warm / 100f) * 0.25f;
        float numbReward = (axes.Numb / 100f) * 1.45f + (social.Neutral / 100f) * 0.12f - (social.Warm / 100f) * 0.30f;
        float maliciousReward = (axes.Malicious / 100f) * 2.75f + (social.Mischievous / 100f) * 0.25f - (social.Warm / 100f) * 0.45f;

        float[] actionRewards =
        {
            healthyReward,
            sadReward,
            numbReward,
            maliciousReward
        };

        for (int stateIndex = 0; stateIndex < PlannerStates.Length; stateIndex++)
        {
            for (int actionIndex = 0; actionIndex < PlannerStates.Length; actionIndex++)
            {
                rewards[stateIndex, actionIndex] = actionRewards[actionIndex];
                if (stateIndex == actionIndex)
                    rewards[stateIndex, actionIndex] += PlannerStates[stateIndex] == CompanionEmotionalState.Numb ? 0.03f : 0.10f;
            }
        }

        return rewards;
    }

    private static float[,,] BuildTransitionTable(
        CompanionContextSnapshot snapshot,
        CompanionSocialSignalSnapshot social,
        CompanionAxisScores axes)
    {
        float[,,] transitions = new float[PlannerStates.Length, PlannerStates.Length, PlannerStates.Length];
        float[] axisWeights =
        {
            axes.Healthy / 100f,
            axes.Sad / 100f,
            axes.Numb / 100f,
            axes.Malicious / 100f
        };

        float[] socialBias =
        {
            social.Warm / 100f,
            social.Neutral / 100f,
            social.Neutral / 100f,
            social.Mischievous / 100f
        };

        for (int stateIndex = 0; stateIndex < PlannerStates.Length; stateIndex++)
        {
            for (int actionIndex = 0; actionIndex < PlannerStates.Length; actionIndex++)
            {
                float total = 0f;
                for (int nextStateIndex = 0; nextStateIndex < PlannerStates.Length; nextStateIndex++)
                {
                    float weight = 0.10f + axisWeights[nextStateIndex] + (0.25f * socialBias[nextStateIndex]);
                    if (nextStateIndex == stateIndex)
                        weight += PlannerStates[stateIndex] == CompanionEmotionalState.Numb ? 0.06f : 0.16f;
                    if (nextStateIndex == actionIndex)
                        weight += PlannerStates[actionIndex] == CompanionEmotionalState.Numb ? 0.32f : 0.52f;

                    transitions[stateIndex, actionIndex, nextStateIndex] = weight;
                    total += weight;
                }

                for (int nextStateIndex = 0; nextStateIndex < PlannerStates.Length; nextStateIndex++)
                    transitions[stateIndex, actionIndex, nextStateIndex] /= total;
            }
        }

        return transitions;
    }

    private static int[] SolveValueIterationPolicy(
        float[,] rewards,
        float[,,] transitions,
        float gamma,
        out CompanionPlannerSolverStats solverStats)
    {
        float[] values = new float[PlannerStates.Length];
        int iterations = 0;
        float finalDelta = 0f;
        bool converged = false;

        for (int iteration = 0; iteration < 24; iteration++)
        {
            float[] nextValues = new float[PlannerStates.Length];
            float delta = 0f;

            for (int stateIndex = 0; stateIndex < PlannerStates.Length; stateIndex++)
            {
                float bestValue = float.NegativeInfinity;
                for (int actionIndex = 0; actionIndex < PlannerStates.Length; actionIndex++)
                {
                    float q = rewards[stateIndex, actionIndex];
                    for (int nextStateIndex = 0; nextStateIndex < PlannerStates.Length; nextStateIndex++)
                        q += gamma * transitions[stateIndex, actionIndex, nextStateIndex] * values[nextStateIndex];

                    if (q > bestValue)
                        bestValue = q;
                }

                nextValues[stateIndex] = bestValue;
                delta = Mathf.Max(delta, Mathf.Abs(nextValues[stateIndex] - values[stateIndex]));
            }

            values = nextValues;
            iterations = iteration + 1;
            finalDelta = delta;

            if (delta <= PlannerConvergenceEpsilon)
            {
                converged = true;
                break;
            }
        }

        int[] policy = new int[PlannerStates.Length];
        for (int stateIndex = 0; stateIndex < PlannerStates.Length; stateIndex++)
            policy[stateIndex] = GetBestActionIndex(rewards, transitions, values, gamma, stateIndex);

        solverStats = new CompanionPlannerSolverStats(
            CompanionPlannerMode.ValueIteration,
            iterations,
            0,
            finalDelta,
            converged,
            0d);
        return policy;
    }

    private static int[] SolvePolicyIteration(
        float[,] rewards,
        float[,,] transitions,
        float gamma,
        out CompanionPlannerSolverStats solverStats)
    {
        int[] policy = new int[PlannerStates.Length];
        float[] values = new float[PlannerStates.Length];
        int iterations = 0;
        int evaluationSweeps = 0;
        float finalDelta = 0f;
        bool converged = false;

        for (int iteration = 0; iteration < 12; iteration++)
        {
            for (int evaluationPass = 0; evaluationPass < 16; evaluationPass++)
            {
                float sweepDelta = 0f;
                for (int stateIndex = 0; stateIndex < PlannerStates.Length; stateIndex++)
                {
                    int actionIndex = policy[stateIndex];
                    float updated = rewards[stateIndex, actionIndex];
                    for (int nextStateIndex = 0; nextStateIndex < PlannerStates.Length; nextStateIndex++)
                        updated += gamma * transitions[stateIndex, actionIndex, nextStateIndex] * values[nextStateIndex];

                    sweepDelta = Mathf.Max(sweepDelta, Mathf.Abs(updated - values[stateIndex]));
                    values[stateIndex] = updated;
                }

                evaluationSweeps++;
                finalDelta = sweepDelta;

                if (sweepDelta <= PlannerConvergenceEpsilon)
                    break;
            }

            bool stable = true;
            for (int stateIndex = 0; stateIndex < PlannerStates.Length; stateIndex++)
            {
                int bestAction = GetBestActionIndex(rewards, transitions, values, gamma, stateIndex);
                if (bestAction != policy[stateIndex])
                {
                    policy[stateIndex] = bestAction;
                    stable = false;
                }
            }

            iterations = iteration + 1;
            if (stable)
            {
                converged = true;
                break;
            }
        }

        solverStats = new CompanionPlannerSolverStats(
            CompanionPlannerMode.PolicyIteration,
            iterations,
            evaluationSweeps,
            finalDelta,
            converged,
            0d);
        return policy;
    }

    private static int GetBestActionIndex(
        float[,] rewards,
        float[,,] transitions,
        float[] values,
        float gamma,
        int stateIndex)
    {
        int bestAction = 0;
        float bestValue = float.NegativeInfinity;

        for (int actionIndex = 0; actionIndex < PlannerStates.Length; actionIndex++)
        {
            float q = rewards[stateIndex, actionIndex];
            for (int nextStateIndex = 0; nextStateIndex < PlannerStates.Length; nextStateIndex++)
                q += gamma * transitions[stateIndex, actionIndex, nextStateIndex] * values[nextStateIndex];

            if (q > bestValue)
            {
                bestValue = q;
                bestAction = actionIndex;
            }
        }

        return bestAction;
    }

    private static float[] ComputeActionValues(
        float[,] rewards,
        float[,,] transitions,
        float gamma,
        int stateIndex)
    {
        float[] values = new float[PlannerStates.Length];

        for (int actionIndex = 0; actionIndex < PlannerStates.Length; actionIndex++)
        {
            float q = rewards[stateIndex, actionIndex];
            for (int nextStateIndex = 0; nextStateIndex < PlannerStates.Length; nextStateIndex++)
                q += gamma * transitions[stateIndex, actionIndex, nextStateIndex] * rewards[nextStateIndex, actionIndex];

            values[actionIndex] = q;
        }

        return values;
    }

    private static CompanionEmotionalState ResolveRecommendedState(
        CompanionProfile profile,
        CompanionEmotionalState currentState,
        CompanionAxisScores axes,
        int actionIndex,
        float[] actionValues)
    {
        CompanionEmotionalState candidate = ActionIndexToState(actionIndex);
        CompanionEmotionalState normalizedCurrent = CompanionEmotionStateUtility.Normalize(currentState);

        if (candidate == normalizedCurrent)
            return candidate;

        int threshold = profile != null ? Mathf.Max(0, profile.emotionSwitchThreshold) : 0;
        float currentValue = actionValues != null && actionValues.Length > EmotionToPlannerIndex(normalizedCurrent)
            ? actionValues[EmotionToPlannerIndex(normalizedCurrent)]
            : 0f;
        float candidateValue = actionValues != null && actionValues.Length > actionIndex
            ? actionValues[actionIndex]
            : currentValue;

        int currentIntensity = axes.GetIntensity(normalizedCurrent);
        int candidateIntensity = axes.GetIntensity(candidate);

        bool strongerUtility = candidateValue >= currentValue + (threshold * 0.01f);
        bool strongerEmotion = candidateIntensity >= currentIntensity + threshold;

        return (strongerUtility || strongerEmotion) ? candidate : normalizedCurrent;
    }

    private static CompanionEmotionalState ActionIndexToState(int actionIndex)
    {
        switch ((CompanionPlannerAction)Mathf.Clamp(actionIndex, 0, PlannerStates.Length - 1))
        {
            case CompanionPlannerAction.SeekWarmth:
                return CompanionEmotionalState.Healthy;

            case CompanionPlannerAction.Lament:
                return CompanionEmotionalState.Sad;

            case CompanionPlannerAction.Harden:
                return CompanionEmotionalState.Malicious;

            default:
                return CompanionEmotionalState.Numb;
        }
    }

    private static int EmotionToPlannerIndex(CompanionEmotionalState state)
    {
        for (int i = 0; i < PlannerStates.Length; i++)
        {
            if (PlannerStates[i] == state)
                return i;
        }

        return 2;
    }

    private static CompanionEmotionalState PickAlternative(CompanionEmotionalState current)
    {
        switch (current)
        {
            case CompanionEmotionalState.Malicious:
                return Random.value < 0.5f ? CompanionEmotionalState.Sad : CompanionEmotionalState.Numb;

            case CompanionEmotionalState.Sad:
                return Random.value < 0.5f ? CompanionEmotionalState.Numb : CompanionEmotionalState.Healthy;

            case CompanionEmotionalState.Numb:
                return Random.value < 0.5f ? CompanionEmotionalState.Sad : CompanionEmotionalState.Malicious;

            default:
                return Random.value < 0.5f ? CompanionEmotionalState.Sad : CompanionEmotionalState.Numb;
        }
    }

    private static string BuildPlannerDebugSummary(
        CompanionContextSnapshot snapshot,
        CompanionPlannerEvaluation evaluation)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("[ROAE][CompanionPlanner][SUCCESS]");
        builder.Append(" mode=").Append(evaluation.SolverStats.Mode);
        builder.Append(" inputMode=").Append(evaluation.InputMode);
        builder.Append(" cache=none");
        builder.Append(" states=").Append(PlannerStates.Length);
        builder.Append(" actions=").Append(PlannerStates.Length);
        builder.Append(" iterations=").Append(evaluation.SolverStats.Iterations);
        builder.Append(" evalSweeps=").Append(evaluation.SolverStats.EvaluationSweeps);
        builder.Append(" finalDelta=").Append(evaluation.SolverStats.FinalDelta.ToString("0.00000"));
        builder.Append(" converged=").Append(evaluation.SolverStats.Converged);
        builder.Append(" durationMs=").Append(evaluation.SolverStats.DurationMs.ToString("0.00"));
        builder.Append(" currentState=").Append(snapshot != null ? snapshot.currentEmotion.ToString() : "Unknown");
        builder.Append(" recommendedState=").Append(evaluation.RecommendedState);
        builder.Append(" chosenAction=").Append(evaluation.ChosenAction);
        builder.Append(" social={warm=").Append(evaluation.Social.Warm.ToString("0.0"));
        builder.Append(" neutral=").Append(evaluation.Social.Neutral.ToString("0.0"));
        builder.Append(" mischievous=").Append(evaluation.Social.Mischievous.ToString("0.0")).Append("}");
        builder.Append(" axes={healthy=").Append(evaluation.Axes.Healthy);
        builder.Append(" sad=").Append(evaluation.Axes.Sad);
        builder.Append(" numb=").Append(evaluation.Axes.Numb);
        builder.Append(" malicious=").Append(evaluation.Axes.Malicious).Append("}");
        builder.Append(" policy={").Append(FormatPolicy(evaluation.Policy)).Append("}");
        builder.Append(" actionValues={").Append(FormatActionValues(evaluation.ActionValues)).Append("}");
        return builder.ToString();
    }

    private static string BuildPlannerDebugKey(
        CompanionContextSnapshot snapshot,
        CompanionPlannerEvaluation evaluation)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(evaluation.SolverStats.Mode);
        builder.Append("|").Append(evaluation.InputMode);
        builder.Append("|").Append(snapshot != null ? snapshot.currentEmotion.ToString() : "Unknown");
        builder.Append("|").Append(evaluation.RecommendedState);
        builder.Append("|").Append(evaluation.ChosenAction);
        builder.Append("|").Append(Mathf.RoundToInt(evaluation.Social.Warm));
        builder.Append("|").Append(Mathf.RoundToInt(evaluation.Social.Neutral));
        builder.Append("|").Append(Mathf.RoundToInt(evaluation.Social.Mischievous));
        builder.Append("|").Append(evaluation.Axes.Healthy);
        builder.Append("|").Append(evaluation.Axes.Sad);
        builder.Append("|").Append(evaluation.Axes.Numb);
        builder.Append("|").Append(evaluation.Axes.Malicious);
        builder.Append("|").Append(FormatPolicy(evaluation.Policy));
        return builder.ToString();
    }

    private static string FormatActionValues(float[] actionValues)
    {
        if (actionValues == null || actionValues.Length == 0)
            return string.Empty;

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < actionValues.Length; i++)
        {
            if (i > 0)
                builder.Append(" | ");

            builder.Append(ActionIndexToState(i)).Append("=").Append(actionValues[i].ToString("0.000"));
        }

        return builder.ToString();
    }

    private static string FormatPolicy(int[] policy)
    {
        if (policy == null || policy.Length == 0)
            return string.Empty;

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < policy.Length && i < PlannerStates.Length; i++)
        {
            if (i > 0)
                builder.Append(" | ");

            builder.Append(PlannerStates[i]).Append("->").Append(ActionIndexToState(policy[i]));
        }

        return builder.ToString();
    }
}

public static class CompanionDialogueResolver
{
    public static CompanionDialogueResult Resolve(
        CompanionDialogueLibrary library,
        CompanionEvaluationContext context,
        CompanionSpeechRequest request,
        CompanionEmotionalState emotion,
        CompanionSaveState saveState,
        CompanionProfile profile)
    {
        if (library == null || request == null)
            return new CompanionDialogueResult(null, 0);

        library.EnsureStarterEntries();

        CompanionDialogueEntry bestEntry = null;
        int bestScore = int.MinValue;
        float bestNoise = float.MinValue;

        for (int i = 0; i < library.Entries.Count; i++)
        {
            CompanionDialogueEntry entry = library.Entries[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.line))
                continue;

            float secondsSinceLastUse = GetSecondsSinceUse(entry.entryId, saveState);
            int weight = entry.EvaluateWeight(context, request, emotion, secondsSinceLastUse);
            if (weight == int.MinValue)
                continue;

            float noise = profile != null ? Random.Range(0f, Mathf.Max(0.0001f, profile.dialogueUnpredictability * 10f)) : 0f;
            if (weight > bestScore || (weight == bestScore && noise > bestNoise))
            {
                bestEntry = entry;
                bestScore = weight;
                bestNoise = noise;
            }
        }

        return new CompanionDialogueResult(bestEntry, bestScore == int.MinValue ? 0 : bestScore);
    }

    private static float GetSecondsSinceUse(string entryId, CompanionSaveState saveState)
    {
        if (saveState == null || saveState.dialogueMemory == null || string.IsNullOrWhiteSpace(entryId))
            return float.MaxValue;

        long now = System.DateTime.UtcNow.Ticks;
        for (int i = 0; i < saveState.dialogueMemory.Count; i++)
        {
            if (!string.Equals(saveState.dialogueMemory[i].entryId, entryId, System.StringComparison.OrdinalIgnoreCase))
                continue;

            long usedTicks = saveState.dialogueMemory[i].lastUsedUtcTicks;
            if (usedTicks <= 0L)
                return float.MaxValue;

            return Mathf.Max(0f, (float)new System.TimeSpan(now - usedTicks).TotalSeconds);
        }

        return float.MaxValue;
    }
}
