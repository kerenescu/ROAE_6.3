using System;
using System.Collections.Generic;
using UnityEngine;

public static class CompanionPersistence
{
    private const string SaveKey = "roae_companion_state_v1";

    public static CompanionSaveState Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return CreateDefault();

        string json = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
            return CreateDefault();

        try
        {
            CompanionSaveState loaded = JsonUtility.FromJson<CompanionSaveState>(json);
            return Sanitize(loaded);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ROAE][CompanionPersistence] Failed to load state. Using defaults. " + ex.Message);
            return CreateDefault();
        }
    }

    public static void Save(CompanionSaveState state)
    {
        CompanionSaveState sanitized = Sanitize(state);
        string json = JsonUtility.ToJson(sanitized);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static CompanionSaveState CreateDefault()
    {
        return Sanitize(new CompanionSaveState());
    }

    private static CompanionSaveState Sanitize(CompanionSaveState state)
    {
        state ??= new CompanionSaveState();
        state.totalSummons = Mathf.Max(0, state.totalSummons);
        state.summonHistoryUtcTicks ??= new List<long>();
        state.unlockedDialoguePools ??= new List<string>();
        state.discoveredInteractionIds ??= new List<string>();
        state.narrativeFlags ??= new List<string>();
        state.dialogueMemory ??= new List<CompanionDialogueMemoryEntry>();
        state.warmSignal = Mathf.Clamp(state.warmSignal, 0f, 100f);
        state.neutralSignal = Mathf.Clamp(state.neutralSignal, 0f, 100f);
        state.mischievousSignal = Mathf.Clamp(state.mischievousSignal, 0f, 100f);
        state.lastNpcSourceId ??= string.Empty;
        return state;
    }
}
