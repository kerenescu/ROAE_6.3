using System;

[Serializable]
public struct NpcDecisionState : IEquatable<NpcDecisionState>
{
    public EmpathyBucket empathy;
    public CreativityBucket creativity;
    public CorruptionBucket corruption;
    public RelationshipBucket relationship;

    public bool Equals(NpcDecisionState other)
    {
        return empathy == other.empathy &&
               creativity == other.creativity &&
               corruption == other.corruption &&
               relationship == other.relationship;
    }

    public override bool Equals(object obj)
    {
        return obj is NpcDecisionState other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = (int) empathy;
            hash = (hash * 397) ^ (int) creativity;
            hash = (hash * 397) ^ (int) corruption;
            hash = (hash * 397) ^ (int) relationship;
            return hash;
        }
    }

    public override string ToString()
    {
        return empathy + "_" + creativity + "_" + corruption + "_" + relationship;
    }
}
