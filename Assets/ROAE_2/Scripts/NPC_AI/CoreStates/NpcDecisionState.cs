using System;

[Serializable]
public struct NpcDecisionState
{
    public EmpathyBucket empathy;
    public CreativityBucket creativity;
    public CorruptionBucket corruption;
    public RelationshipBucket relationship;

    public override string ToString()
    {
        return empathy + "_" + creativity + "_" + corruption + "_" + relationship;
    }
}
