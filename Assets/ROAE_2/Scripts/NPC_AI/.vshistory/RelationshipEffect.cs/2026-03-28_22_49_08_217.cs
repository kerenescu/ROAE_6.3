using UnityEngine;

[System.Serializable]
public class RelationshipEffect
{
    [SerializeField] private string npcId;
    [SerializeField] private int amount;

    public void Apply()
    {
        NpcRelationshipState.AdjustRelationship(npcId, amount);
    }
}