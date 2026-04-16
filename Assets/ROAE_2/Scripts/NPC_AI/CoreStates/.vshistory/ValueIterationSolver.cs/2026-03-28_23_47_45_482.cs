using System;
using System.Collections.Generic;

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

public static class ValueIterationSolver
{
    public static Dictionary<NpcDecisionState, NpcActionType> Solve(
        List<NpcDecisionState> states,
        NpcActionType[] actions,
        Func<NpcDecisionState, NpcActionType, float> rewardFunction,
        Func<NpcDecisionState, NpcActionType, List<StateTransition>> transitionFunction,
        float gamma = 0.85f,
        float epsilon = 0.0001f,
        int maxIterations = 100)
    {
        Dictionary<NpcDecisionState, float> values = new Dictionary<NpcDecisionState, float>();
        Dictionary<NpcDecisionState, float> newValues = new Dictionary<NpcDecisionState, float>();

        for (int i = 0; i < states.Count; i++)
            values[states[i]] = 0f;

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

            if (delta < epsilon)
                break;
        }

        Dictionary<NpcDecisionState, NpcActionType> policy =
            new Dictionary<NpcDecisionState, NpcActionType>();

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

        return policy;
    }
}