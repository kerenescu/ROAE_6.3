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

        int value = CreativeCore.Instance.empathy;

        if (value <= -2)
            return EmpathyBucket.Low;

        if (value >= 2)
            return EmpathyBucket.High;

        return EmpathyBucket.Neutral;
    }

    private static CreativityBucket GetCreativityBucket()
    {
        if (CreativeCore.Instance == null)
            return CreativityBucket.Medium;

        int value = CreativeCore.Instance.creativity;

        if (value < 35)
            return CreativityBucket.Low;

        if (value > 70)
            return CreativityBucket.High;

        return CreativityBucket.Medium;
    }

    private static CorruptionBucket GetCorruptionBucket()
    {
        if (CreativeCore.Instance == null)
            return CorruptionBucket.Low;

        int value = CreativeCore.Instance.plantCorruption;

        if (value < 3)
            return CorruptionBucket.Low;

        if (value > 6)
            return CorruptionBucket.High;

        return CorruptionBucket.Medium;
    }

    private static RelationshipBucket GetRelationshipBucket(string npcId)
    {
        int value = NpcRelationshipState.GetRelationshipScore(npcId);

        if (value <= -2)
            return RelationshipBucket.Bad;

        if (value >= 2)
            return RelationshipBucket.Good;

        return RelationshipBucket.Neutral;
    }
}
