using UnityEngine;

public static class BaristaRelationshipDebug
{
    public static void LogApply(string npcId, int before, int delta, int after, string source)
    {
        if (npcId != "barista")
            return;

        BaristaDebug.Log(
            "RelationshipEffect.Apply",
            "npcId=" + npcId +
            " source=" + source +
            " before=" + before +
            " delta=" + delta +
            " after=" + after);
    }
}
