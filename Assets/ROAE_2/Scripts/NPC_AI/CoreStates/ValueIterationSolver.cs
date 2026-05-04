using System;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;

public struct StateTransition
{
    public NpcDecisionState nextState;
    public float probability;

    public StateTransition(NpcDecisionState nextState, float probability)
    {
        this.nextState = nextState;
        this.probability = probability;
    }
}

public readonly struct NpcValueIterationSolution
{
    public readonly Dictionary<NpcDecisionState, NpcActionType> policy;
    public readonly Dictionary<NpcDecisionState, float> values;

    public NpcValueIterationSolution(
        Dictionary<NpcDecisionState, NpcActionType> policy,
        Dictionary<NpcDecisionState, float> values)
    {
        this.policy = policy ?? new Dictionary<NpcDecisionState, NpcActionType>();
        this.values = values ?? new Dictionary<NpcDecisionState, float>();
    }
}

public readonly struct NpcValueIterationRunStats
{
    public readonly bool converged;
    public readonly int iterations;
    public readonly float finalDelta;
    public readonly double durationMs;

    public NpcValueIterationRunStats(
        bool converged,
        int iterations,
        float finalDelta,
        double durationMs)
    {
        this.converged = converged;
        this.iterations = iterations;
        this.finalDelta = finalDelta;
        this.durationMs = durationMs;
    }
}

public static class ValueIterationSolver
{
    public static IReadOnlyDictionary<NpcDecisionState, float> LastComputedValues => lastComputedValues;
    public static NpcValueIterationRunStats LastRunStats => lastRunStats;

    private static Dictionary<NpcDecisionState, float> lastComputedValues = new Dictionary<NpcDecisionState, float>();
    private static NpcValueIterationRunStats lastRunStats = new NpcValueIterationRunStats(false, 0, 0f, 0d);

    public static Dictionary<NpcDecisionState, NpcActionType> Solve(
        List<NpcDecisionState> states,
        NpcActionType[] actions,
        Func<NpcDecisionState, NpcActionType, float> rewardFunction,
        Func<NpcDecisionState, NpcActionType, List<StateTransition>> transitionFunction,
        float gamma = 0.85f,
        float epsilon = 0.0001f,
        int maxIterations = 100)
    {
        return SolveWithValues(
            states,
            actions,
            rewardFunction,
            transitionFunction,
            gamma,
            epsilon,
            maxIterations).policy;
    }

    public static NpcValueIterationSolution SolveWithValues(
        List<NpcDecisionState> states,
        NpcActionType[] actions,
        Func<NpcDecisionState, NpcActionType, float> rewardFunction,
        Func<NpcDecisionState, NpcActionType, List<StateTransition>> transitionFunction,
        float gamma = 0.85f,
        float epsilon = 0.0001f,
        int maxIterations = 100)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Dictionary<NpcDecisionState, float> values = new Dictionary<NpcDecisionState, float>();
        Dictionary<NpcDecisionState, float> newValues = new Dictionary<NpcDecisionState, float>();

        for (int i = 0; i < states.Count; i++)
            values[states[i]] = 0f;

        BaristaDebug.Log("ValueIterationSolver.Solve", "start states=" + states.Count + " actions=" + actions.Length + " gamma=" + gamma + " epsilon=" + epsilon + " maxIterations=" + maxIterations);

        bool converged = false;
        int iterationsRun = 0;
        float finalDelta = 0f;

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            float delta = 0f;

            for (int i = 0; i < states.Count; i++)
            {
                NpcDecisionState state = states[i];
                float bestValue = float.NegativeInfinity;

                for (int j = 0; j < actions.Length; j++)
                {
                    NpcActionType action = actions[j];
                    float q = rewardFunction(state, action);

                    List<StateTransition> transitions = transitionFunction(state, action);
                    for (int k = 0; k < transitions.Count; k++)
                    {
                        StateTransition t = transitions[k];
                        q += gamma * t.probability * values[t.nextState];
                    }

                    if (q > bestValue)
                        bestValue = q;
                }

                newValues[state] = bestValue;
                float diff = Math.Abs(bestValue - values[state]);
                if (diff > delta)
                    delta = diff;
            }

            foreach (NpcDecisionState state in states)
                values[state] = newValues[state];

            iterationsRun = iteration + 1;
            finalDelta = delta;

            BaristaDebug.Log("ValueIterationSolver.Solve", "iteration=" + iteration + " delta=" + delta);

            if (delta < epsilon)
            {
                converged = true;
                BaristaDebug.Log("ValueIterationSolver.Solve", "converged iteration=" + iteration + " delta=" + delta);
                break;
            }
        }

        lastComputedValues = new Dictionary<NpcDecisionState, float>(values);

        Dictionary<NpcDecisionState, NpcActionType> policy = new Dictionary<NpcDecisionState, NpcActionType>();

        for (int i = 0; i < states.Count; i++)
        {
            NpcDecisionState state = states[i];
            float bestValue = float.NegativeInfinity;
            NpcActionType bestAction = actions[0];

            for (int j = 0; j < actions.Length; j++)
            {
                NpcActionType action = actions[j];
                float q = rewardFunction(state, action);

                List<StateTransition> transitions = transitionFunction(state, action);
                for (int k = 0; k < transitions.Count; k++)
                {
                    StateTransition t = transitions[k];
                    q += gamma * t.probability * values[t.nextState];
                }

                if (q > bestValue)
                {
                    bestValue = q;
                    bestAction = action;
                }
            }

            policy[state] = bestAction;
        }

        BaristaDebug.Log("ValueIterationSolver.Solve", "policyReady entries=" + policy.Count);
        stopwatch.Stop();
        lastRunStats = new NpcValueIterationRunStats(
            converged,
            iterationsRun,
            finalDelta,
            stopwatch.Elapsed.TotalMilliseconds);

        BaristaDebug.Log(
            "ValueIterationSolver.Solve",
            "finish success=" + converged +
            " iterations=" + iterationsRun +
            " finalDelta=" + finalDelta.ToString("0.00000") +
            " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00") +
            " policyEntries=" + policy.Count);

        return new NpcValueIterationSolution(
            policy,
            new Dictionary<NpcDecisionState, float>(values));
    }
}
