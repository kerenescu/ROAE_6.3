using System.Collections.Generic;
using UnityEngine;

public static class BaristaPolicy
{
    private const string RewardProfilePath = "NPC_AI/Barista/BaristaRewardProfile";
    private const string TransitionProfilePath = "NPC_AI/Barista/BaristaTransitionProfile";

    private static Dictionary<NpcDecisionState, NpcActionType> cachedPolicy;
    private static NpcRewardProfile cachedRewardProfile;
    private static NpcTransitionProfile cachedTransitionProfile;

    public static NpcActionType GetBestAction(NpcDecisionState state)
    {
        if (cachedPolicy == null || cachedPolicy.Count == 0)
            BuildPolicy();

        if (cachedPolicy.TryGetValue(state, out NpcActionType action))
            return action;

        return NpcActionType.Neutral;
    }

    public static void RebuildPolicy()
    {
        cachedRewardProfile = null;
        cachedTransitionProfile = null;
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

        if (cachedRewardProfile == null)
        {
            cachedPolicy = new Dictionary<NpcDecisionState, NpcActionType>();
            for (int i = 0; i < states.Count; i++)
                cachedPolicy[states[i]] = NpcActionType.Neutral;

            return;
        }

        cachedPolicy = ValueIterationSolver.Solve(
            states,
            actions,
            (s, a) => NpcRewardEvaluator.GetReward(cachedRewardProfile, s, a),
            (s, a) => NpcTransitionEvaluator.GetTransitions(cachedTransitionProfile, s, a),
            0.85f,
            0.0001f,
            100
        );
    }
}