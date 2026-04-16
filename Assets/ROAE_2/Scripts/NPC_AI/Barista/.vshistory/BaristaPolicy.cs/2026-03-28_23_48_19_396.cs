using System.Collections.Generic;

public static class BaristaPolicy
{
    private static Dictionary<NpcDecisionState, NpcActionType> cachedPolicy;

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
        BuildPolicy();
    }

    private static void BuildPolicy()
    {
        List<NpcDecisionState> states = NpcStateSpaceGenerator.GenerateAllStates();
        NpcActionType[] actions = BaristaRewardModel.GetAvailableActions();

        cachedPolicy = ValueIterationSolver.Solve(
            states,
            actions,
            BaristaRewardModel.GetReward,
            GetTransitions,
            0.85f,
            0.0001f,
            100
        );
    }

    private static List<StateTransition> GetTransitions(NpcDecisionState state, NpcActionType action)
    {
        List<StateTransition> transitions = new List<StateTransition>();

        switch (action)
        {
            case NpcActionType.Warm:
                transitions.Add(new StateTransition(SetRelationship(state, ImproveRelationship(state.relationship)), 0.70f));
                transitions.Add(new StateTransition(state, 0.30f));
                break;

            case NpcActionType.Guarded:
                transitions.Add(new StateTransition(SetRelationship(state, WorsenRelationship(state.relationship)), 0.70f));
                transitions.Add(new StateTransition(state, 0.30f));
                break;

            case NpcActionType.Hint:
                transitions.Add(new StateTransition(state, 1.00f));
                break;

            case NpcActionType.WarmHint:
                transitions.Add(new StateTransition(SetRelationship(state, ImproveRelationship(state.relationship)), 0.80f));
                transitions.Add(new StateTransition(state, 0.20f));
                break;

            case NpcActionType.GuardedHint:
                transitions.Add(new StateTransition(SetRelationship(state, WorsenRelationship(state.relationship)), 0.60f));
                transitions.Add(new StateTransition(state, 0.40f));
                break;

            case NpcActionType.Neutral:
            default:
                transitions.Add(new StateTransition(state, 1.00f));
                break;
        }

        return transitions;
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

            case RelationshipBucket.Good:
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

            case RelationshipBucket.Bad:
            default:
                return RelationshipBucket.Bad;
        }
    }
}