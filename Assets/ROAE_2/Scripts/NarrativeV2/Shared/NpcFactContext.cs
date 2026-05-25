using System.Collections.Generic;
using UnityEngine;

public sealed class NpcFactContext
{
    private readonly Dictionary<string, bool> boolFacts = new Dictionary<string, bool>();
    private readonly Dictionary<string, int> intFacts = new Dictionary<string, int>();
    private readonly Dictionary<string, string> textFacts = new Dictionary<string, string>();

    public string npcId;
    public int creativity;
    public int empathy;
    public int corruption;
    public int relationship;
    public bool readUnknownText;
    public bool introDone;
    public bool hasDrink;
    public BaristaDrinkType pendingDrink;
    public bool pendingDrinkAcknowledged;
    public BaristaDrinkType heldDrink;
    public string chapterId;
    public string sceneId;
    public string momentId;

    public static NpcFactContext BuildLive(string npcId)
    {
        CreativeCore core = CreativeCore.Instance ?? UnityEngine.Object.FindFirstObjectByType<CreativeCore>();
        string resolvedNpcId = string.IsNullOrWhiteSpace(npcId) ? "npc" : npcId.Trim();

        NpcFactContext context = new NpcFactContext
        {
            npcId = resolvedNpcId,
            creativity = core != null ? core.Creativity : PlayerPrefs.GetInt("creativity", CreativeStatScale.DefaultCreativity),
            empathy = core != null ? core.Empathy : PlayerPrefs.GetInt("empathy", CreativeStatScale.DefaultEmpathy),
            corruption = core != null ? core.PlantCorruption : PlayerPrefs.GetInt("plantCorruption", CreativeStatScale.DefaultCorruption),
            relationship = NpcRelationshipState.GetRelationshipScore(resolvedNpcId),
            readUnknownText = BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.ReadUnknownText01),
            introDone = ResolveIntroDone(),
            hasDrink = BaristaWelcomeState.HasDrink(),
            pendingDrink = BaristaWelcomeState.GetPendingDrink(),
            pendingDrinkAcknowledged = BaristaWelcomeState.HasAcknowledgedPendingDrink(),
            heldDrink = BaristaWelcomeState.GetHeldDrink(),
            chapterId = NarrativeProgressState.GetCurrentChapterId(),
            sceneId = NarrativeProgressState.GetCurrentSceneId(),
            momentId = NarrativeProgressState.GetCurrentMomentId()
        };

        context.SetBool("read_unknown_text", context.readUnknownText);
        context.SetBool("intro_done", context.introDone);
        context.SetBool("has_drink", context.hasDrink);
        context.SetBool("pending_drink_acknowledged", context.pendingDrinkAcknowledged);

        context.SetInt("creativity", context.creativity);
        context.SetInt("empathy", context.empathy);
        context.SetInt("corruption", context.corruption);
        context.SetInt("relationship", context.relationship);
        context.SetInt("pending_drink", (int) context.pendingDrink);
        context.SetInt("held_drink", (int) context.heldDrink);

        context.SetText("npc_id", context.npcId);
        context.SetText("chapter_id", context.chapterId);
        context.SetText("scene_id", context.sceneId);
        context.SetText("moment_id", context.momentId);

        return context;
    }

    public NpcTonePlanningRuntimeState ToRuntimeState(
        NpcToneDialoguePhaseDefinition phase,
        string introDoneFlagKeyOverride = "")
    {
        bool resolvedReadUnknownText = phase.readUnknownText != null
            ? phase.readUnknownText.Resolve()
            : readUnknownText;
        bool resolvedIntroDone = phase.introDone != null
            ? phase.introDone.Resolve()
            : ResolveBooleanFact(introDoneFlagKeyOverride, introDone);

        return new NpcTonePlanningRuntimeState
        {
            readUnknownText = resolvedReadUnknownText,
            creativity = creativity,
            corruption = corruption,
            empathy = empathy,
            relationship = relationship,
            introDone = resolvedIntroDone,
            hasDrink = phase.hasDrink || phase.heldDrink != BaristaDrinkType.None || phase.pendingDrink != BaristaDrinkType.None,
            pendingDrink = phase.pendingDrink,
            pendingDrinkAcknowledged = phase.pendingDrinkAcknowledged,
            heldDrink = phase.heldDrink
        };
    }

    public bool GetBool(string key, bool fallbackValue = false)
    {
        return boolFacts.TryGetValue(key, out bool value) ? value : fallbackValue;
    }

    public int GetInt(string key, int fallbackValue = 0)
    {
        return intFacts.TryGetValue(key, out int value) ? value : fallbackValue;
    }

    public string GetText(string key, string fallbackValue = "")
    {
        return textFacts.TryGetValue(key, out string value) ? value : fallbackValue;
    }

    public void SetBool(string key, bool value)
    {
        if (!string.IsNullOrWhiteSpace(key))
            boolFacts[key] = value;
    }

    public void SetInt(string key, int value)
    {
        if (!string.IsNullOrWhiteSpace(key))
            intFacts[key] = value;
    }

    public void SetText(string key, string value)
    {
        if (!string.IsNullOrWhiteSpace(key))
            textFacts[key] = value ?? string.Empty;
    }

    public string ToDebugString()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.Append("npc=").Append(npcId)
            .Append(" creativity=").Append(creativity)
            .Append(" empathy=").Append(empathy)
            .Append(" corruption=").Append(corruption)
            .Append(" relationship=").Append(relationship)
            .Append(" readUnknownText=").Append(readUnknownText)
            .Append(" introDone=").Append(introDone)
            .Append(" hasDrink=").Append(hasDrink)
            .Append(" pendingDrink=").Append(pendingDrink)
            .Append(" pendingAcknowledged=").Append(pendingDrinkAcknowledged)
            .Append(" heldDrink=").Append(heldDrink)
            .Append(" chapter=").Append(chapterId)
            .Append(" scene=").Append(sceneId)
            .Append(" moment=").Append(momentId);
        return builder.ToString();
    }

    private bool ResolveBooleanFact(string playerPrefKeyOverride, bool fallbackValue)
    {
        if (!string.IsNullOrWhiteSpace(playerPrefKeyOverride))
            return PlayerPrefs.GetInt(playerPrefKeyOverride, 0) == 1;

        return fallbackValue;
    }

    private static bool ResolveIntroDone()
    {
        return PlayerPrefs.GetInt(BaristaWelcomeKeys.BaristaIntroDone, 0) == 1;
    }
}
