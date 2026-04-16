using System.Collections.Generic;

public static class NpcRelationshipState
{
    private static Dictionary<string, int> relationshipScores = new Dictionary<string, int>();

    public static int GetRelationshipScore(string npcId)
    {
        if (relationshipScores.TryGetValue(npcId, out int score))
            return score;

        return 0;
    }

    public static void AddRelationshipScore(string npcId, int delta)
    {
        int current = GetRelationshipScore(npcId);
        relationshipScores[npcId] = current + delta;
    }

    public static void SetRelationshipScore(string npcId, int value)
    {
        relationshipScores[npcId] = value;
    }

    public static void ResetRelationshipScore(string npcId)
    {
        relationshipScores[npcId] = 0;
    }
}