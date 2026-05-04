using System.Collections.Generic;
using UnityEngine;

public readonly struct NpcDecisionContext
{
    public readonly string npcId;
    public readonly int creativity;
    public readonly int empathy;
    public readonly int corruption;
    public readonly int relationship;
    public readonly CreativityBucket creativityBucket;
    public readonly EmpathyBucket empathyBucket;
    public readonly CorruptionBucket corruptionBucket;
    public readonly RelationshipBucket relationshipBucket;

    public NpcDecisionContext(
        string npcId,
        int creativity,
        int empathy,
        int corruption,
        int relationship)
    {
        this.npcId = npcId ?? string.Empty;
        this.creativity = creativity;
        this.empathy = empathy;
        this.corruption = corruption;
        this.relationship = relationship;
        creativityBucket = BucketCreativity(creativity);
        empathyBucket = BucketEmpathy(empathy);
        corruptionBucket = BucketCorruption(corruption);
        relationshipBucket = BucketRelationship(relationship);
    }

    public string NpcId => npcId;
    public int Creativity => creativity;
    public int Empathy => empathy;
    public int Corruption => corruption;
    public int Relationship => relationship;
    public CreativityBucket CreativityBucket => creativityBucket;
    public EmpathyBucket EmpathyBucket => empathyBucket;
    public CorruptionBucket CorruptionBucket => corruptionBucket;
    public RelationshipBucket RelationshipBucket => relationshipBucket;

    public NpcDecisionState ToDecisionState()
    {
        return new NpcDecisionState
        {
            creativity = creativityBucket,
            empathy = empathyBucket,
            corruption = corruptionBucket,
            relationship = relationshipBucket
        };
    }

    public string ToDebugString()
    {
        return "npcId=" + npcId +
               " creativity=" + creativity + "(" + creativityBucket + ")" +
               " empathy=" + empathy + "(" + empathyBucket + ")" +
               " corruption=" + corruption + "(" + corruptionBucket + ")" +
               " relationship=" + relationship + "(" + relationshipBucket + ")" +
               " chapter=" + NarrativeProgressState.GetCurrentChapterId() +
               " scene=" + NarrativeProgressState.GetCurrentSceneId() +
               " moment=" + NarrativeProgressState.GetCurrentMomentId();
    }

    public static NpcDecisionContext Build(string npcId)
    {
        CreativeCore core = CreativeCore.Instance ?? Object.FindFirstObjectByType<CreativeCore>();
        int creativityValue = core != null ? core.Creativity : PlayerPrefs.GetInt("creativity", 50);
        int empathyValue = core != null ? core.Empathy : PlayerPrefs.GetInt("empathy", 0);
        int corruptionValue = core != null ? core.PlantCorruption : PlayerPrefs.GetInt("plantCorruption", 0);
        int relationshipValue = NpcRelationshipState.GetRelationshipScore(npcId);

        return new NpcDecisionContext(
            npcId,
            creativityValue,
            empathyValue,
            corruptionValue,
            relationshipValue);
    }

    private static CreativityBucket BucketCreativity(int value)
    {
        if (value < 35)
            return CreativityBucket.Low;

        if (value > 70)
            return CreativityBucket.High;

        return CreativityBucket.Medium;
    }

    private static EmpathyBucket BucketEmpathy(int value)
    {
        if (value <= -2)
            return EmpathyBucket.Low;

        if (value >= 2)
            return EmpathyBucket.High;

        return EmpathyBucket.Neutral;
    }

    private static CorruptionBucket BucketCorruption(int value)
    {
        if (value < 3)
            return CorruptionBucket.Low;

        if (value > 6)
            return CorruptionBucket.High;

        return CorruptionBucket.Medium;
    }

    private static RelationshipBucket BucketRelationship(int value)
    {
        if (value <= -2)
            return RelationshipBucket.Bad;

        if (value >= 2)
            return RelationshipBucket.Good;

        return RelationshipBucket.Neutral;
    }
}

public static class NpcDecisionContextFlags
{
    public static bool AllMatch(IReadOnlyList<string> keys, bool expectedValue)
    {
        if (keys == null)
            return true;

        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i];
            if (string.IsNullOrWhiteSpace(key))
                continue;

            bool value = PlayerPrefs.GetInt(key, 0) == 1;
            if (value != expectedValue)
                return false;
        }

        return true;
    }
}
