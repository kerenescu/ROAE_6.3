using UnityEngine;

public static class NpcStateDiscretizer
{
    public static NpcDecisionState Build(string npcId)
    {
        NpcDecisionState state = new NpcDecisionState();

        state.empathy = GetEmpathyBucket();
        state.creativity = GetCreativityBucket();
        state.corruption = GetCorruptionBucket();
        state.relationship = GetRelationshipBucket(npcId);

        return state;
    }

    private static EmpathyBucket GetEmpathyBucket()
    {
        if (CreativeCore.Instance == null)
            return EmpathyBucket.Neutral;

        return CreativeStatScale.BucketEmpathy(CreativeCore.Instance.empathy);
    }

    private static CreativityBucket GetCreativityBucket()
    {
        if (CreativeCore.Instance == null)
            return CreativityBucket.Medium;

        return CreativeStatScale.BucketCreativity(CreativeCore.Instance.creativity);
    }

    private static CorruptionBucket GetCorruptionBucket()
    {
        if (CreativeCore.Instance == null)
            return CorruptionBucket.Low;

        return CreativeStatScale.BucketCorruption(CreativeCore.Instance.plantCorruption);
    }

    private static RelationshipBucket GetRelationshipBucket(string npcId)
    {
        int value = NpcRelationshipState.GetRelationshipScore(npcId);

        return CreativeStatScale.BucketRelationship(value);
    }
}
