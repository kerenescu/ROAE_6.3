using System;
using System.Collections.Generic;

public interface INarrativeMomentDefinition
{
    string MomentId { get; }
    bool IsEnabled { get; }
    int Priority { get; }
    string RequiredChapterId { get; }
    string RequiredSceneId { get; }
    string RequiredNarrativeMomentId { get; }
    IReadOnlyList<string> RequiredTrueFlags { get; }
    IReadOnlyList<string> RequiredFalseFlags { get; }
    bool SkipWhenCompleted { get; }
    string CompletedFlagKey { get; }
}

public static class NarrativeMomentSelection
{
    public static T ResolveHighestPriorityActiveMoment<T>(
        IReadOnlyList<T> definitions,
        Func<string, bool> readBoolFlag)
        where T : class, INarrativeMomentDefinition
    {
        T best = null;
        int bestPriority = int.MinValue;

        if (definitions == null)
            return null;

        for (int i = 0; i < definitions.Count; i++)
        {
            T definition = definitions[i];
            if (!IsActive(definition, readBoolFlag))
                continue;

            if (best == null || definition.Priority > bestPriority)
            {
                best = definition;
                bestPriority = definition.Priority;
            }
        }

        return best;
    }

    public static bool IsActive(
        INarrativeMomentDefinition definition,
        Func<string, bool> readBoolFlag)
    {
        if (definition == null || !definition.IsEnabled)
            return false;

        if (!NarrativeProgressState.MatchesCurrentChapter(definition.RequiredChapterId))
            return false;

        if (!NarrativeProgressState.MatchesCurrentScene(definition.RequiredSceneId))
            return false;

        if (!NarrativeProgressState.MatchesCurrentMoment(definition.RequiredNarrativeMomentId))
            return false;

        if (definition.SkipWhenCompleted && !string.IsNullOrWhiteSpace(definition.CompletedFlagKey))
        {
            if (ReadFlag(definition.CompletedFlagKey, readBoolFlag))
                return false;
        }

        if (!AllFlagsMatch(definition.RequiredTrueFlags, true, readBoolFlag))
            return false;

        if (!AllFlagsMatch(definition.RequiredFalseFlags, false, readBoolFlag))
            return false;

        return true;
    }

    private static bool AllFlagsMatch(
        IReadOnlyList<string> flags,
        bool expectedValue,
        Func<string, bool> readBoolFlag)
    {
        if (flags == null)
            return true;

        for (int i = 0; i < flags.Count; i++)
        {
            string key = flags[i];
            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (ReadFlag(key, readBoolFlag) != expectedValue)
                return false;
        }

        return true;
    }

    private static bool ReadFlag(string key, Func<string, bool> readBoolFlag)
    {
        return readBoolFlag != null && readBoolFlag.Invoke(key);
    }
}
