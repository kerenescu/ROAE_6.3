using System;

[System.Serializable]
public class NpcRewardRule
{
    public NpcActionType action;
    public float rewardDelta;

    public bool useEmpathy;
    public EmpathyBucket empathy;

    public bool useCreativity;
    public CreativityBucket creativity;

    public bool useCorruption;
    public CorruptionBucket corruption;

    public bool useRelationship;
    public RelationshipBucket relationship;

    public bool Matches(NpcDecisionState state, NpcActionType candidateAction)
    {
        if (candidateAction != action)
            return false;

        if (useEmpathy && state.empathy != empathy)
            return false;

        if (useCreativity && state.creativity != creativity)
            return false;

        if (useCorruption && state.corruption != corruption)
            return false;

        if (useRelationship && state.relationship != relationship)
            return false;

        return true;
    }
}
