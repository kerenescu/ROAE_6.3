using UnityEngine;

public class NpcRelationshipState : MonoBehaviour
{
    [SerializeField] private string npcId = "barista";
    [SerializeField] private int relationshipScore = 0;

    public string NpcId => npcId;
    public int RelationshipScore => relationshipScore;

    public void AdjustRelationship(int amount)
    {
        relationshipScore += amount;
    }

    public void SetRelationship(int value)
    {
        relationshipScore = value;
    }
}