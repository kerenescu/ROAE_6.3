using System.Collections.Generic;
using UnityEngine;

public class NpcRelationshipState : MonoBehaviour
{
    private const string PlayerPrefsPrefix = "npc_relationship_";

    private static readonly Dictionary<string, NpcRelationshipState> Registry =
        new Dictionary<string, NpcRelationshipState>();

    [SerializeField] private string npcId = "barista";
    [SerializeField] private int relationshipScore = 0;

    public string NpcId => npcId;
    public int RelationshipScore => relationshipScore;

    private void Awake()
    {
        if (!string.IsNullOrEmpty(npcId))
            relationshipScore = PlayerPrefs.GetInt(BuildPrefsKey(npcId), relationshipScore);

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
        int oldValue = relationshipScore;
        relationshipScore += amount;
        SaveRelationship();
        Debug.Log("Relationship " + npcId + ": " + oldValue + " -> " + relationshipScore);
    }

    public void SetRelationship(int value)
    {
        relationshipScore = value;
        SaveRelationship();
    }

    public static void AdjustRelationship(string targetNpcId, int amount)
    {
        if (string.IsNullOrEmpty(targetNpcId))
            return;

        if (Registry.TryGetValue(targetNpcId, out var state))
        {
            state.AdjustRelationship(amount);
            return;
        }

        SetStoredRelationship(targetNpcId, GetRelationshipScore(targetNpcId) + amount);
    }

    public static void SetRelationship(string targetNpcId, int value)
    {
        if (string.IsNullOrEmpty(targetNpcId))
            return;

        if (Registry.TryGetValue(targetNpcId, out var state))
        {
            state.SetRelationship(value);
            return;
        }

        SetStoredRelationship(targetNpcId, value);
    }

    public static void ResetRelationship(string targetNpcId)
    {
        SetRelationship(targetNpcId, 0);
    }

    public static int GetRelationshipScore(string targetNpcId)
    {
        if (string.IsNullOrEmpty(targetNpcId))
            return 0;

        if (Registry.TryGetValue(targetNpcId, out var state))
            return state.RelationshipScore;

        return PlayerPrefs.GetInt(BuildPrefsKey(targetNpcId), 0);
    }

    private void SaveRelationship()
    {
        if (string.IsNullOrEmpty(npcId))
            return;

        SetStoredRelationship(npcId, relationshipScore);
    }

    private static void SetStoredRelationship(string targetNpcId, int value)
    {
        PlayerPrefs.SetInt(BuildPrefsKey(targetNpcId), value);
        PlayerPrefs.Save();
    }

    private static string BuildPrefsKey(string targetNpcId)
    {
        return PlayerPrefsPrefix + targetNpcId;
    }
}
