public static class NpcRewardEvaluator
{
    public static float GetReward(NpcRewardProfile profile, NpcDecisionState state, NpcActionType action)
    {
        if (profile == null)
            return 0f;

        float reward = 0f;

        var baseRewards = profile.BaseRewards;
        for (int i = 0; i < baseRewards.Count; i++)
        {
            if (baseRewards[i].action == action)
                reward += baseRewards[i].reward;
        }

        var rules = profile.Rules;
        for (int i = 0; i < rules.Count; i++)
        {
            if (rules[i].Matches(state, action))
                reward += rules[i].rewardDelta;
        }

        return reward;
    }
}
