using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcChoiceEffect", menuName = "ROAE/NPC/Choice Effect")]
public class NpcChoiceEffect : DialogueChoiceEffect
{
    [Header("Target NPC")]
    [SerializeField] private string npcId = "";

    [Header("Relationship")]
    [SerializeField] private int relationshipDelta;

    [Header("Stats")]
    [SerializeField] private int creativityDelta;
    [SerializeField] private int empathyDelta;
    [SerializeField] private int corruptionDelta;

    [Header("Flags")]
    [SerializeField] private List<string> setTrueFlags = new List<string>();
    [SerializeField] private List<string> setFalseFlags = new List<string>();

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public override void Apply()
    {
        int appliedEmpathyDelta = CreativeStatScale.ConvertLegacyEmpathyDelta(empathyDelta);
        int appliedCorruptionDelta = CreativeStatScale.ConvertLegacyCorruptionDelta(corruptionDelta);

        if (!string.IsNullOrWhiteSpace(npcId) && relationshipDelta != 0)
            NpcRelationshipState.AdjustRelationship(npcId, relationshipDelta);

        ApplyStats(appliedEmpathyDelta, appliedCorruptionDelta);
        ApplyFlags();

        if (debugLogs)
        {
            Debug.Log(
                "[ROAE][NpcChoiceEffect] npcId=" + npcId +
                " relationshipDelta=" + relationshipDelta +
                " creativityDelta=" + creativityDelta +
                " empathyDelta=" + empathyDelta + "->" + appliedEmpathyDelta +
                " corruptionDelta=" + corruptionDelta + "->" + appliedCorruptionDelta +
                " trueFlags=" + string.Join(",", setTrueFlags) +
                " falseFlags=" + string.Join(",", setFalseFlags));
        }
    }

    private void ApplyStats(int appliedEmpathyDelta, int appliedCorruptionDelta)
    {
        CreativeCore core = CreativeCore.Instance ?? Object.FindFirstObjectByType<CreativeCore>();
        if (core == null)
            return;

        CreativeHUD hud = CreativeHUD.Instance;

        if (creativityDelta != 0)
        {
            core.AdjustCreativity(creativityDelta);
            if (hud != null)
                hud.ShowStatChange("creativity", creativityDelta);
        }

        if (empathyDelta != 0)
        {
            core.AdjustEmpathy(appliedEmpathyDelta);
            if (hud != null)
                hud.ShowStatChange("empathy", appliedEmpathyDelta);
        }

        if (corruptionDelta != 0)
        {
            core.AdjustCorruption(appliedCorruptionDelta);
            if (hud != null)
                hud.ShowStatChange("plantCorruption", appliedCorruptionDelta);
        }
    }

    private void ApplyFlags()
    {
        for (int i = 0; i < setTrueFlags.Count; i++)
            SetFlag(setTrueFlags[i], true);

        for (int i = 0; i < setFalseFlags.Count; i++)
            SetFlag(setFalseFlags[i], false);

        PlayerPrefs.Save();
    }

    private static void SetFlag(string key, bool value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }
}
