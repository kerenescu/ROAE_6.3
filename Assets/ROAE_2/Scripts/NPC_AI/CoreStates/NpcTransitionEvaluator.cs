using System.Collections.Generic;

public static class NpcTransitionEvaluator
{
    public static List<StateTransition> GetTransitions(NpcTransitionProfile profile, NpcDecisionState state, NpcActionType action)
    {
        List<StateTransition> result = new List<StateTransition>();

        if (profile == null)
        {
            result.Add(new StateTransition(state, 1f));
            return result;
        }

        NpcActionTransitionEntry entry = FindEntry(profile, action);
        if (entry == null)
        {
            result.Add(new StateTransition(state, 1f));
            return result;
        }

        float worsen = entry.worsenProbability;
        float keep = entry.keepProbability;
        float improve = entry.improveProbability;

        float total = worsen + keep + improve;
        if (total <= 0f)
        {
            result.Add(new StateTransition(state, 1f));
            return result;
        }

        worsen /= total;
        keep /= total;
        improve /= total;

        if (worsen > 0f)
            result.Add(new StateTransition(SetRelationship(state, WorsenRelationship(state.relationship)), worsen));

        if (keep > 0f)
            result.Add(new StateTransition(state, keep));

        if (improve > 0f)
            result.Add(new StateTransition(SetRelationship(state, ImproveRelationship(state.relationship)), improve));

        return result;
    }

    private static NpcActionTransitionEntry FindEntry(NpcTransitionProfile profile, NpcActionType action)
    {
        var entries = profile.Transitions;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].action == action)
                return (NpcActionTransitionEntry)entries[i];
        }

        return null;
    }

    private static NpcDecisionState SetRelationship(NpcDecisionState state, RelationshipBucket relationship)
    {
        state.relationship = relationship;
        return state;
    }

    private static RelationshipBucket ImproveRelationship(RelationshipBucket current)
    {
        switch (current)
        {
            case RelationshipBucket.Bad:
                return RelationshipBucket.Neutral;
            case RelationshipBucket.Neutral:
                return RelationshipBucket.Good;
            default:
                return RelationshipBucket.Good;
        }
    }

    private static RelationshipBucket WorsenRelationship(RelationshipBucket current)
    {
        switch (current)
        {
            case RelationshipBucket.Good:
                return RelationshipBucket.Neutral;
            case RelationshipBucket.Neutral:
                return RelationshipBucket.Bad;
            default:
                return RelationshipBucket.Bad;
        }
    }
}
