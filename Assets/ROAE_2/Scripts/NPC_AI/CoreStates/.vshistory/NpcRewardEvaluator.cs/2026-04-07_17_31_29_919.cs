using System.Collections.Generic;
using System.Text;

public sealed class NpcRewardBreakdown
{
    public float total;
    public float baseReward;
    public List<string> matchedRules = new List<string>();

    public string ToDebugString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("base=").Append(baseReward).Append(" total=").Append(total);
        if (matchedRules.Count > 0)
        {
            sb.Append(" matchedRules=");
            for (int i = 0; i < matchedRules.Count; i++)
            {
                if (i > 0)
                    sb.Append(" | ");
                sb.Append(matchedRules[i]);
            }
        }
        return sb.ToString();
    }
}

public static class NpcRewardEvaluator
{
    public static float GetReward(NpcRewardProfile profile, NpcDecisionState state, NpcActionType action)
    {
        return GetRewardBreakdown(profile, state, action).total;
    }

    public static NpcRewardBreakdown GetRewardBreakdown(NpcRewardProfile profile, NpcDecisionState state, NpcActionType action)
    {
        NpcRewardBreakdown result = new NpcRewardBreakdown();

        if (profile == null)
            return result;

        var baseRewards = profile.BaseRewards;
        for (int i = 0; i < baseRewards.Count; i++)
        {
            if (baseRewards[i].action == action)
            {
                result.baseReward += baseRewards[i].reward;
                result.total += baseRewards[i].reward;
            }
        }

        var rules = profile.Rules;
        for (int i = 0; i < rules.Count; i++)
        {
            if (!rules[i].Matches(state, action))
                continue;

            result.total += rules[i].rewardDelta;
            result.matchedRules.Add(BuildRuleLabel(rules[i]));
        }

        return result;
    }

    private static string BuildRuleLabel(NpcRewardRule rule)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("action=").Append(rule.action);
        sb.Append(" rewardDelta=").Append(rule.rewardDelta);

        if (rule.useEmpathy)
            sb.Append(" empathy=").Append(rule.empathy);
        if (rule.useCreativity)
            sb.Append(" creativity=").Append(rule.creativity);
        if (rule.useCorruption)
            sb.Append(" corruption=").Append(rule.corruption);
        if (rule.useRelationship)
            sb.Append(" relationship=").Append(rule.relationship);

        return sb.ToString();
    }
}
