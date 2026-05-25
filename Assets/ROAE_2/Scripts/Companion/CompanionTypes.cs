using System;
using System.Collections.Generic;
using UnityEngine;

public enum CompanionPresenceState
{
    Hidden = 0,
    Summoning = 1,
    Manifested = 2,
    Withdrawing = 3
}

public enum CompanionEmotionalState
{
    Numb = 0,
    Curious = 1,
    Comforting = 2,
    Nervous = 3,
    Exhausted = 4,
    Avoidant = 5,
    Healthy = 6,
    Malicious = 7,
    Sad = 8
}

public enum CompanionPlannerMode
{
    ValueIteration = 0,
    PolicyIteration = 1
}

public enum CompanionSocialTone
{
    None = 0,
    Warm = 1,
    Neutral = 2,
    Mischievous = 3,
    Mixed = 4
}

public enum CompanionSummonPointType
{
    Generic = 0,
    Mirror = 1,
    Water = 2,
    GlowingPlant = 3,
    QuietSpace = 4
}

public enum CompanionDialogueIntent
{
    Ambient = 0,
    Arrival = 1,
    Hint = 2,
    Reaction = 3,
    Comfort = 4,
    Warning = 5,
    Departure = 6,
    Contradiction = 7
}

public enum CompanionThreatLevel
{
    None = 0,
    Uneasy = 1,
    Threatened = 2,
    Overwhelming = 3
}

public enum CompanionNarrativeReliability
{
    Grounded = 0,
    Ambiguous = 1,
    Contradictory = 2,
    SelfAware = 3
}

public enum CompanionEnvironmentTag
{
    None = 0,
    Sadness = 1,
    Danger = 2,
    Nostalgia = 3,
    Falsehood = 4,
    Loneliness = 5,
    Warmth = 6,
    Reflection = 7,
    Decay = 8,
    Bloom = 9,
    Water = 10,
    Silence = 11,
    Shelter = 12,
    Memory = 13,
    Threshold = 14
}

[Serializable]
public struct CompanionDialogueMemoryEntry
{
    public string entryId;
    public long lastUsedUtcTicks;
    public int heardCount;
}

[Serializable]
public sealed class CompanionSaveState
{
    public int totalSummons;
    public long lastSummonUtcTicks;
    public string lastSummonSceneId = string.Empty;
    public string lastActivePointId = string.Empty;
    public CompanionEmotionalState lastKnownEmotion = CompanionEmotionalState.Healthy;
    public CompanionPresenceState lastPresenceState = CompanionPresenceState.Hidden;
    public List<long> summonHistoryUtcTicks = new List<long>();
    public List<string> unlockedDialoguePools = new List<string>();
    public List<string> discoveredInteractionIds = new List<string>();
    public List<string> narrativeFlags = new List<string>();
    public List<CompanionDialogueMemoryEntry> dialogueMemory = new List<CompanionDialogueMemoryEntry>();
    public float warmSignal;
    public float neutralSignal;
    public float mischievousSignal;
    public long socialSignalUpdatedUtcTicks;
    public string lastNpcSourceId = string.Empty;
    public CompanionSocialTone lastSocialTone = CompanionSocialTone.None;
}

[Serializable]
public sealed class CompanionContextSnapshot
{
    public int creativity;
    public int empathy;
    public int corruption;
    public bool isSafeSpace;
    public bool isCompanionVisible;
    public CompanionThreatLevel threatLevel;
    public CompanionPresenceState presenceState;
    public CompanionEmotionalState currentEmotion;
    public CompanionSummonPointType pointType;
    public string chapterId = string.Empty;
    public string sceneId = string.Empty;
    public string momentId = string.Empty;
    public string summonPointId = string.Empty;
    public float warmSignal;
    public float neutralSignal;
    public float mischievousSignal;
    public CompanionSocialTone dominantSocialTone;
    public string lastNpcSourceId = string.Empty;
    public List<CompanionEnvironmentTag> tags = new List<CompanionEnvironmentTag>();

    public bool HasTag(CompanionEnvironmentTag tag)
    {
        return tags != null && tags.Contains(tag);
    }

    public bool HasSocialSignal()
    {
        return warmSignal > 0.01f || neutralSignal > 0.01f || mischievousSignal > 0.01f;
    }
}

public readonly struct CompanionAxisScores
{
    public CompanionAxisScores(int numb, int sad, int malicious, int healthy)
    {
        Numb = Mathf.Clamp(numb, 0, 100);
        Sad = Mathf.Clamp(sad, 0, 100);
        Malicious = Mathf.Clamp(malicious, 0, 100);
        Healthy = Mathf.Clamp(healthy, 0, 100);
    }

    public int Numb { get; }
    public int Sad { get; }
    public int Malicious { get; }
    public int Healthy { get; }

    public CompanionEmotionalState GetDominantState()
    {
        if (Malicious >= Sad && Malicious >= Healthy && Malicious >= Numb)
            return CompanionEmotionalState.Malicious;

        if (Sad >= Healthy && Sad >= Numb)
            return CompanionEmotionalState.Sad;

        if (Numb >= Healthy)
            return CompanionEmotionalState.Numb;

        return CompanionEmotionalState.Healthy;
    }

    public int GetIntensity(CompanionEmotionalState state)
    {
        switch (state)
        {
            case CompanionEmotionalState.Malicious:
                return Malicious;

            case CompanionEmotionalState.Sad:
                return Sad;

            case CompanionEmotionalState.Numb:
                return Numb;

            case CompanionEmotionalState.Healthy:
                return Healthy;

            default:
                return Mathf.Max(Mathf.Max(Healthy, Sad), Mathf.Max(Malicious, Numb));
        }
    }
}

public static class CompanionEmotionStateUtility
{
    public static CompanionEmotionalState Normalize(CompanionEmotionalState state)
    {
        switch (state)
        {
            case CompanionEmotionalState.Curious:
            case CompanionEmotionalState.Comforting:
                return CompanionEmotionalState.Healthy;

            case CompanionEmotionalState.Nervous:
                return CompanionEmotionalState.Sad;

            case CompanionEmotionalState.Exhausted:
            case CompanionEmotionalState.Avoidant:
                return CompanionEmotionalState.Numb;

            default:
                return state;
        }
    }
}

public sealed class CompanionEvaluationContext
{
    private readonly HashSet<string> narrativeFlags;
    private readonly HashSet<string> unlockedPools;
    private readonly HashSet<string> discoveredInteractions;

    public CompanionEvaluationContext(
        CompanionContextSnapshot snapshot,
        IEnumerable<string> narrativeFlags,
        IEnumerable<string> unlockedPools,
        IEnumerable<string> discoveredInteractions)
    {
        Snapshot = snapshot ?? new CompanionContextSnapshot();
        this.narrativeFlags = BuildSet(narrativeFlags);
        this.unlockedPools = BuildSet(unlockedPools);
        this.discoveredInteractions = BuildSet(discoveredInteractions);
    }

    public CompanionContextSnapshot Snapshot { get; }

    public bool HasNarrativeFlag(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && narrativeFlags.Contains(key.Trim());
    }

    public bool HasUnlockedPool(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && unlockedPools.Contains(key.Trim());
    }

    public bool HasDiscoveredInteraction(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && discoveredInteractions.Contains(key.Trim());
    }

    private static HashSet<string> BuildSet(IEnumerable<string> items)
    {
        HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (items == null)
            return set;

        foreach (string item in items)
        {
            if (!string.IsNullOrWhiteSpace(item))
                set.Add(item.Trim());
        }

        return set;
    }
}

[Serializable]
public sealed class CompanionConditionSet
{
    [Header("Safety")]
    public bool requireSafeSpace;
    public bool requireVisibleCompanion;
    public CompanionThreatLevel maxThreatLevel = CompanionThreatLevel.Overwhelming;

    [Header("Ranges")]
    public Vector2Int creativityRange = new Vector2Int(0, 100);
    public Vector2Int empathyRange = new Vector2Int(0, 100);
    public Vector2Int corruptionRange = new Vector2Int(0, 100);

    [Header("World Tags")]
    public List<CompanionEnvironmentTag> requiredTags = new List<CompanionEnvironmentTag>();
    public List<CompanionEnvironmentTag> blockedTags = new List<CompanionEnvironmentTag>();
    public List<CompanionSummonPointType> allowedPointTypes = new List<CompanionSummonPointType>();

    [Header("Narrative Flags")]
    public List<string> requiredTrueFlags = new List<string>();
    public List<string> requiredFalseFlags = new List<string>();
    public List<string> requiredUnlockedPools = new List<string>();
    public List<string> requiredDiscoveredInteractions = new List<string>();

    public bool Matches(CompanionEvaluationContext context)
    {
        if (context == null)
            return false;

        CompanionContextSnapshot snapshot = context.Snapshot;
        if (snapshot == null)
            return false;

        if (requireSafeSpace && !snapshot.isSafeSpace)
            return false;

        if (requireVisibleCompanion && !snapshot.isCompanionVisible)
            return false;

        if (snapshot.threatLevel > maxThreatLevel)
            return false;

        if (!InRange(snapshot.creativity, creativityRange) ||
            !InRange(snapshot.empathy, empathyRange) ||
            !InRange(snapshot.corruption, corruptionRange))
        {
            return false;
        }

        if (allowedPointTypes != null && allowedPointTypes.Count > 0 && !allowedPointTypes.Contains(snapshot.pointType))
            return false;

        if (!ContainsAllTags(snapshot.tags, requiredTags))
            return false;

        if (ContainsAnyTag(snapshot.tags, blockedTags))
            return false;

        if (!AllFlagsMatch(requiredTrueFlags, true, context))
            return false;

        if (!AllFlagsMatch(requiredFalseFlags, false, context))
            return false;

        if (!AllPoolsUnlocked(requiredUnlockedPools, context))
            return false;

        if (!AllInteractionsDiscovered(requiredDiscoveredInteractions, context))
            return false;

        return true;
    }

    private static bool InRange(int value, Vector2Int range)
    {
        return value >= range.x && value <= range.y;
    }

    private static bool ContainsAllTags(List<CompanionEnvironmentTag> available, List<CompanionEnvironmentTag> required)
    {
        if (required == null || required.Count == 0)
            return true;

        if (available == null || available.Count == 0)
            return false;

        for (int i = 0; i < required.Count; i++)
        {
            if (!available.Contains(required[i]))
                return false;
        }

        return true;
    }

    private static bool ContainsAnyTag(List<CompanionEnvironmentTag> available, List<CompanionEnvironmentTag> blocked)
    {
        if (blocked == null || blocked.Count == 0 || available == null || available.Count == 0)
            return false;

        for (int i = 0; i < blocked.Count; i++)
        {
            if (available.Contains(blocked[i]))
                return true;
        }

        return false;
    }

    private static bool AllFlagsMatch(List<string> flags, bool expected, CompanionEvaluationContext context)
    {
        if (flags == null)
            return true;

        for (int i = 0; i < flags.Count; i++)
        {
            string key = flags[i];
            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (context.HasNarrativeFlag(key) != expected)
                return false;
        }

        return true;
    }

    private static bool AllPoolsUnlocked(List<string> pools, CompanionEvaluationContext context)
    {
        if (pools == null)
            return true;

        for (int i = 0; i < pools.Count; i++)
        {
            string key = pools[i];
            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (!context.HasUnlockedPool(key))
                return false;
        }

        return true;
    }

    private static bool AllInteractionsDiscovered(List<string> interactions, CompanionEvaluationContext context)
    {
        if (interactions == null)
            return true;

        for (int i = 0; i < interactions.Count; i++)
        {
            string key = interactions[i];
            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (!context.HasDiscoveredInteraction(key))
                return false;
        }

        return true;
    }
}

[Serializable]
public sealed class CompanionSpeechRequest
{
    public CompanionDialogueIntent intent = CompanionDialogueIntent.Ambient;
    public List<CompanionEnvironmentTag> extraTags = new List<CompanionEnvironmentTag>();
    public string focusId = string.Empty;
}

public static class CompanionMath
{
    public static int ClampPercent(int value)
    {
        return Mathf.Clamp(value, 0, 100);
    }

    public static float SecondsSince(long utcTicks, DateTime utcNow)
    {
        if (utcTicks <= 0L)
            return float.MaxValue;

        return Mathf.Max(0f, (float)new TimeSpan(utcNow.Ticks - utcTicks).TotalSeconds);
    }
}
