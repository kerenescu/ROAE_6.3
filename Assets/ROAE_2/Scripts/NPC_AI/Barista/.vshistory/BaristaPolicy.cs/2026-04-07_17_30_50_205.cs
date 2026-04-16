using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class BaristaPolicy
{
    private const string RewardProfilePath = "NPC_AI/Barista/BaristaRewardProfile";
    private const string TransitionProfilePath = "NPC_AI/Barista/BaristaTransitionProfile";
    private const float Gamma = 0.85f;
    private const float Epsilon = 0.0001f;
    private const int MaxIterations = 100;

    private static Dictionary<NpcDecisionState, NpcActionType> cachedPolicy;
    private static NpcRewardProfile cachedRewardProfile;
    private static NpcTransitionProfile cachedTransitionProfile;

    public static NpcActionType GetBestAction(NpcDecisionState state)
    {
        if (cachedPolicy == null || cachedPolicy.Count == 0)
            BuildPolicy();

        if (!cachedPolicy.TryGetValue(state, out NpcActionType action))
        {
            BaristaDebug.Warn("BaristaPolicy.GetBestAction", "missing policy entry state=" + state + " fallback=Neutral");
            return NpcActionType.Neutral;
        }

        LogEvaluation(state, action);
        return action;
    }

    public static void RebuildPolicy()
    {
        cachedRewardProfile = null;
        cachedTransitionProfile = null;
        cachedPolicy = null;
        BuildPolicy();
    }

    private static void BuildPolicy()
    {
        if (cachedRewardProfile == null)
            cachedRewardProfile = Resources.Load<NpcRewardProfile>(RewardProfilePath);
        if (cachedTransitionProfile == null)
            cachedTransitionProfile = Resources.Load<NpcTransitionProfile>(TransitionProfilePath);

        List<NpcDecisionState> states = NpcStateSpaceGenerator.GenerateAllStates();

        NpcActionType[] actions =
        {
            NpcActionType.Neutral,
            NpcActionType.Warm,
            NpcActionType.Guarded,
            NpcActionType.Hint,
            NpcActionType.WarmHint,
            NpcActionType.GuardedHint
        };

        BaristaDebug.Log(
            "BaristaPolicy.BuildPolicy",
            "rewardProfileLoaded=" + (cachedRewardProfile != null) +
            " transitionProfileLoaded=" + (cachedTransitionProfile != null) +
            " states=" + states.Count +
            " actions=" + actions.Length);

        if (cachedRewardProfile == null)
        {
            cachedPolicy = new Dictionary<NpcDecisionState, NpcActionType>();
            for (int i = 0; i < states.Count; i++)
                cachedPolicy[states[i]] = NpcActionType.Neutral;

            BaristaDebug.Warn("BaristaPolicy.BuildPolicy", "reward profile missing. policy forced to Neutral for all states");
            return;
        }

        cachedPolicy = ValueIterationSolver.Solve(
            states,
            actions,
            (s, a) => NpcRewardEvaluator.GetReward(cachedRewardProfile, s, a),
            (s, a) => NpcTransitionEvaluator.GetTransitions(cachedTransitionProfile, s, a),
            Gamma,
            Epsilon,
            MaxIterations);
    }

    private static void LogEvaluation(NpcDecisionState state, NpcActionType selectedAction)
    {
        if (cachedRewardProfile == null)
            return;

        IReadOnlyDictionary<NpcDecisionState, float> values = ValueIterationSolver.LastComputedValues;
        NpcActionType[] actions =
        {
            NpcActionType.Neutral,
            NpcActionType.Warm,
            NpcActionType.Guarded,
            NpcActionType.Hint,
            NpcActionType.WarmHint,
            NpcActionType.GuardedHint
        };

        StringBuilder sb = new StringBuilder();
        sb.Append("state=").Append(state).Append(" selected=").Append(selectedAction);

        for (int i = 0; i < actions.Length; i++)
        {
            NpcActionType action = actions[i];
            NpcRewardBreakdown rewardBreakdown = NpcRewardEvaluator.GetRewardBreakdown(cachedRewardProfile, state, action);
            List<StateTransition> transitions = NpcTransitionEvaluator.GetTransitions(cachedTransitionProfile, state, action);
            float q = rewardBreakdown.total;

            StringBuilder transitionSb = new StringBuilder();
            for (int t = 0; t < transitions.Count; t++)
            {
                StateTransition transition = transitions[t];
                float nextValue = values != null && values.ContainsKey(transition.nextState) ? values[transition.nextState] : 0f;
                q += Gamma * transition.probability * nextValue;
                if (t > 0)
                    transitionSb.Append(" | ");
                transitionSb.Append(transition.nextState).Append(" p=").Append(transition.probability).Append(" v=").Append(nextValue);
            }

            sb.Append(" || action=")
              .Append(action)
              .Append(" reward{")
              .Append(rewardBreakdown.ToDebugString())
              .Append("}")
              .Append(" transitions{")
              .Append(transitionSb)
              .Append("}")
              .Append(" q=")
              .Append(q);
        }

        BaristaDebug.Log("BaristaPolicy.GetBestAction", sb.ToString());
    }
}
