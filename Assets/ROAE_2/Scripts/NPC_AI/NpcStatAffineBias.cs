using UnityEngine;

[System.Serializable]
public struct NpcAffineBiasResult
{
    public readonly int rawCreativity;
    public readonly int rawEmpathy;
    public readonly int rawCorruption;
    public readonly int rawRelationship;
    public readonly int creativity;
    public readonly int empathy;
    public readonly int corruption;
    public readonly int relationship;

    public NpcAffineBiasResult(
        int rawCreativity,
        int rawEmpathy,
        int rawCorruption,
        int rawRelationship,
        int creativity,
        int empathy,
        int corruption,
        int relationship)
    {
        this.rawCreativity = rawCreativity;
        this.rawEmpathy = rawEmpathy;
        this.rawCorruption = rawCorruption;
        this.rawRelationship = rawRelationship;
        this.creativity = creativity;
        this.empathy = empathy;
        this.corruption = corruption;
        this.relationship = relationship;
    }

    public static NpcAffineBiasResult Identity(
        int creativity,
        int empathy,
        int corruption,
        int relationship)
    {
        return new NpcAffineBiasResult(
            creativity,
            empathy,
            corruption,
            relationship,
            creativity,
            empathy,
            corruption,
            relationship);
    }

    public string ToDebugString()
    {
        return "creativity=" + rawCreativity + "->" + creativity +
               " empathy=" + rawEmpathy + "->" + empathy +
               " corruption=" + rawCorruption + "->" + corruption +
               " relationship=" + rawRelationship + "->" + relationship;
    }
}

[System.Serializable]
public struct NpcStatAffineChannel
{
    [SerializeField] private float bias;
    [SerializeField] private float creativityWeight;
    [SerializeField] private float empathyWeight;
    [SerializeField] private float corruptionWeight;
    [SerializeField] private float relationshipWeight;

    public float Evaluate(
        int creativity,
        int empathy,
        int corruption,
        int relationship)
    {
        return bias +
               (creativity * creativityWeight) +
               (empathy * empathyWeight) +
               (corruption * corruptionWeight) +
               (relationship * relationshipWeight);
    }
}

[System.Serializable]
public sealed class NpcStatAffineBias
{
    [SerializeField] private bool enabled;
    [SerializeField] private NpcStatAffineChannel creativity;
    [SerializeField] private NpcStatAffineChannel empathy;
    [SerializeField] private NpcStatAffineChannel corruption;
    [SerializeField] private NpcStatAffineChannel relationship;

    public bool Enabled => enabled;

    public NpcAffineBiasResult Apply(
        int rawCreativity,
        int rawEmpathy,
        int rawCorruption,
        int rawRelationship)
    {
        if (!enabled)
            return NpcAffineBiasResult.Identity(rawCreativity, rawEmpathy, rawCorruption, rawRelationship);

        int biasedCreativity = CreativeStatScale.ClampCreativity(
            Mathf.RoundToInt(creativity.Evaluate(rawCreativity, rawEmpathy, rawCorruption, rawRelationship)));
        int biasedEmpathy = CreativeStatScale.ClampEmpathy(
            Mathf.RoundToInt(empathy.Evaluate(rawCreativity, rawEmpathy, rawCorruption, rawRelationship)));
        int biasedCorruption = CreativeStatScale.ClampCorruption(
            Mathf.RoundToInt(corruption.Evaluate(rawCreativity, rawEmpathy, rawCorruption, rawRelationship)));
        int biasedRelationship = Mathf.Clamp(
            Mathf.RoundToInt(relationship.Evaluate(rawCreativity, rawEmpathy, rawCorruption, rawRelationship)),
            -100,
            100);

        return new NpcAffineBiasResult(
            rawCreativity,
            rawEmpathy,
            rawCorruption,
            rawRelationship,
            biasedCreativity,
            biasedEmpathy,
            biasedCorruption,
            biasedRelationship);
    }
}
