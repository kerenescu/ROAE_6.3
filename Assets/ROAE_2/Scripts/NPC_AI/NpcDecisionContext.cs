using System.Collections.Generic;
using UnityEngine;

public readonly struct NpcDecisionContext
{
    public readonly string npcId;
    public readonly int rawCreativity;
    public readonly int rawEmpathy;
    public readonly int rawCorruption;
    public readonly int rawRelationship;
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
        int rawCreativity,
        int rawEmpathy,
        int rawCorruption,
        int rawRelationship,
        int creativity,
        int empathy,
        int corruption,
        int relationship,
        CreativityBucket creativityBucket,
        EmpathyBucket empathyBucket,
        CorruptionBucket corruptionBucket,
        RelationshipBucket relationshipBucket)
    {
        this.npcId = npcId ?? string.Empty;
        this.rawCreativity = rawCreativity;
        this.rawEmpathy = rawEmpathy;
        this.rawCorruption = rawCorruption;
        this.rawRelationship = rawRelationship;
        this.creativity = creativity;
        this.empathy = empathy;
        this.corruption = corruption;
        this.relationship = relationship;
        this.creativityBucket = creativityBucket;
        this.empathyBucket = empathyBucket;
        this.corruptionBucket = corruptionBucket;
        this.relationshipBucket = relationshipBucket;
    }

    public static NpcDecisionContext CreateIdentity(
    string npcId,
    int creativity,
    int empathy,
    int corruption,
    int relationship)
    {
        return new NpcDecisionContext(
            npcId,
            creativity,
            empathy,
            corruption,
            relationship,
            creativity,
            empathy,
            corruption,
            relationship,
            BucketCreativity(creativity),
            BucketEmpathy(empathy),
            BucketCorruption(corruption),
            BucketRelationship(relationship));
    }

    public string NpcId => npcId;
    public int RawCreativity => rawCreativity;
    public int RawEmpathy => rawEmpathy;
    public int RawCorruption => rawCorruption;
    public int RawRelationship => rawRelationship;
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
               " creativity=" + rawCreativity + "->" + creativity + "(" + creativityBucket + ")" +
               " empathy=" + rawEmpathy + "->" + empathy + "(" + empathyBucket + ")" +
               " corruption=" + rawCorruption + "->" + corruption + "(" + corruptionBucket + ")" +
               " relationship=" + rawRelationship + "->" + relationship + "(" + relationshipBucket + ")" +
               " chapter=" + NarrativeProgressState.GetCurrentChapterId() +
               " scene=" + NarrativeProgressState.GetCurrentSceneId() +
               " moment=" + NarrativeProgressState.GetCurrentMomentId();
    }

    public static NpcDecisionContext Build(string npcId)
    {
        CreativeCore core = CreativeCore.Instance ?? Object.FindFirstObjectByType<CreativeCore>();
        int creativityValue = core != null ? core.Creativity : PlayerPrefs.GetInt("creativity", CreativeStatScale.DefaultCreativity);
        int empathyValue = core != null ? core.Empathy : PlayerPrefs.GetInt("empathy", CreativeStatScale.DefaultEmpathy);
        int corruptionValue = core != null ? core.PlantCorruption : PlayerPrefs.GetInt("plantCorruption", CreativeStatScale.DefaultCorruption);
        int relationshipValue = NpcRelationshipState.GetRelationshipScore(npcId);

        return CreateIdentity(
            npcId,
            creativityValue,
            empathyValue,
            corruptionValue,
            relationshipValue);
    }

    public static NpcDecisionContext Build(NpcDefinition definition)
    {
        if (definition == null)
            return Build(string.Empty);

        string resolvedNpcId = definition.NpcId;
        CreativeCore core = CreativeCore.Instance ?? Object.FindFirstObjectByType<CreativeCore>();
        int rawCreativityValue = core != null ? core.Creativity : PlayerPrefs.GetInt("creativity", CreativeStatScale.DefaultCreativity);
        int rawEmpathyValue = core != null ? core.Empathy : PlayerPrefs.GetInt("empathy", CreativeStatScale.DefaultEmpathy);
        int rawCorruptionValue = core != null ? core.PlantCorruption : PlayerPrefs.GetInt("plantCorruption", CreativeStatScale.DefaultCorruption);
        int rawRelationshipValue = NpcRelationshipState.GetRelationshipScore(resolvedNpcId);

        NpcAffineBiasResult biasResult = definition.ApplyStatAffineBias(
            rawCreativityValue,
            rawEmpathyValue,
            rawCorruptionValue,
            rawRelationshipValue);

        return new NpcDecisionContext(
            resolvedNpcId,
            biasResult.rawCreativity,
            biasResult.rawEmpathy,
            biasResult.rawCorruption,
            biasResult.rawRelationship,
            biasResult.creativity,
            biasResult.empathy,
            biasResult.corruption,
            biasResult.relationship,
            BucketCreativity(biasResult.creativity),
            BucketEmpathy(biasResult.empathy),
            BucketCorruption(biasResult.corruption),
            BucketRelationship(biasResult.relationship));
    }



    private static CreativityBucket BucketCreativity(int value)
    {
        return CreativeStatScale.BucketCreativity(value);
    }

    private static EmpathyBucket BucketEmpathy(int value)
    {
        return CreativeStatScale.BucketEmpathy(value);
    }

    private static CorruptionBucket BucketCorruption(int value)
    {
        return CreativeStatScale.BucketCorruption(value);
    }

    private static RelationshipBucket BucketRelationship(int value)
    {
        return CreativeStatScale.BucketRelationship(value);
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
