using System.Collections.Generic;
using UnityEngine;

public class NpcRelationshipState : MonoBehaviour
{
    private static readonly Dictionary<string, NpcRelationshipState> Registry =
        new Dictionary<string, NpcRelationshipState>();

    [SerializeField] private string npcId = "barista";
    [SerializeField] private int relationshipScore = 0;

    public string NpcId => npcId;
    public int RelationshipScore => relationshipScore;

    private void Awake()
    {
        if (!string.IsNullOrEmpty(npcId))
            Registry[npcId] = this;
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(npcId) && Registry.TryGetValue(npcId, out var current) && current == this)
            Registry.Remove(npcId);
    }

    public void AdjustRelationship(int amount)
    {
        relationshipScore += amount;
    }

    public void SetRelationship(int value)
    {
        relationshipScore = value;
    }

    public static void AdjustRelationship(string targetNpcId, int amount)
    {
        if (string.IsNullOrEmpty(targetNpcId))
            return;

        if (Registry.TryGetValue(targetNpcId, out var state))
            state.AdjustRelationship(amount);
    }

    public static int GetRelationshipScore(string targetNpcId)
    {
        if (string.IsNullOrEmpty(targetNpcId))
            return 0;

        if (Registry.TryGetValue(targetNpcId, out var state))
            return state.RelationshipScore;

        return 0;
    }
}