using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class RoaeValueIterationAuditRunner
{
    private const string MenuRoot = "Tools/ROAE/NPC/Audits/";
    private const string ResultsFolderRelative = "Temp/ROAE_AI_Audits";
    private const int DefaultCreativity = 51;
    private const int DefaultEmpathy = 1;
    private const int DefaultCorruption = 1;
    private const int DefaultRelationship = 0;

    private static readonly string[] ProfileFolders =
    {
        "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Profiles",
        "Assets/ROAE_2/Data/NarrativeV2/MadameLichenia/Profiles",
        "Assets/ROAE_2/Data/NarrativeV2/Anticariat/Profiles"
    };

    private static readonly string[] KnownNpcIds =
    {
        "barista",
        "madame_lichenia",
        "anticar"
    };

    [MenuItem(MenuRoot + "Run ValueIteration Narrative Audit")]
    public static void RunFromMenu()
    {
        RunAudit(false);
    }

    [MenuItem(MenuRoot + "Reveal Last ValueIteration Audit")]
    public static void RevealLastAudit()
    {
        Directory.CreateDirectory(ResultsFolderAbsolute);
        EditorUtility.RevealInFinder(ResultsFolderAbsolute);
    }

    public static void RunValueIterationAuditBatch()
    {
        RunAudit(true);
    }

    private static void RunAudit(bool exitEditorOnFinish)
    {
        List<NpcToneDialogueProfile> profiles = LoadProfiles();
        if (profiles.Count == 0)
        {
            Debug.LogWarning("[ROAE][AI][ValueIterationAudit][FAIL] reason=no_profiles_found");
            if (exitEditorOnFinish)
                EditorApplication.Exit(1);
            return;
        }

        AuditPlayerPrefsSnapshot snapshot = AuditPlayerPrefsSnapshot.Capture(BuildTrackedIntKeys(profiles));

        try
        {
            List<ValueIterationAuditResult> results = BuildResults(profiles);
            ValueIterationAuditSummary summary = BuildSummary(results);

            Directory.CreateDirectory(ResultsFolderAbsolute);
            string htmlPath = Path.Combine(ResultsFolderAbsolute, "roae-value-iteration-audit.html");
            string jsonPath = Path.Combine(ResultsFolderAbsolute, "roae-value-iteration-audit.json");

            File.WriteAllText(htmlPath, BuildHtmlReport(results, summary), Encoding.UTF8);
            File.WriteAllText(jsonPath, BuildJsonReport(results, summary), Encoding.UTF8);

            Debug.Log(
                "[ROAE][AI][ValueIterationAudit][SUCCESS] scenarios=" + summary.totalScenarios +
                " phaseMatches=" + summary.phaseMatches +
                " toneExactMatches=" + summary.valueExactToneMatches +
                " nativeVsViToneDiffs=" + summary.nativeVsValueToneDifferences +
                " fallbackReplies=" + summary.valueFallbackCount +
                " html=" + htmlPath +
                " json=" + jsonPath);

            if (!exitEditorOnFinish)
                EditorUtility.RevealInFinder(htmlPath);

            if (exitEditorOnFinish)
                EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogError("[ROAE][AI][ValueIterationAudit][FAIL] " + ex);
            if (exitEditorOnFinish)
                EditorApplication.Exit(1);
        }
        finally
        {
            snapshot.Restore();
        }
    }

    private static List<NpcToneDialogueProfile> LoadProfiles()
    {
        var loaded = new List<NpcToneDialogueProfile>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string[] guids = AssetDatabase.FindAssets("t:NpcToneDialogueProfile", ProfileFolders);
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (string.IsNullOrWhiteSpace(path) || !seen.Add(path))
                continue;

            NpcToneDialogueProfile profile = AssetDatabase.LoadAssetAtPath<NpcToneDialogueProfile>(path);
            if (profile == null)
                continue;

            if (!KnownNpcIds.Contains(profile.NpcIdOrDefault))
                continue;

            loaded.Add(profile);
        }

        return loaded
            .OrderBy(p => p.NpcIdOrDefault)
            .ThenByDescending(p => p.priority)
            .ThenBy(p => p.MomentIdOrDefault)
            .ThenBy(p => p.name)
            .ToList();
    }

    private static List<ValueIterationAuditResult> BuildResults(List<NpcToneDialogueProfile> profiles)
    {
        var results = new List<ValueIterationAuditResult>();

        for (int i = 0; i < profiles.Count; i++)
        {
            NpcToneDialogueProfile profile = profiles[i];
            if (profile.phaseDefinitions == null || profile.phaseDefinitions.Count == 0)
                continue;

            for (int phaseIndex = 0; phaseIndex < profile.phaseDefinitions.Count; phaseIndex++)
            {
                NpcToneDialoguePhaseDefinition phase = profile.phaseDefinitions[phaseIndex];
                if (phase == null)
                    continue;

                ApplyBaselineState();
                ApplyProfileActivation(profile);
                ApplyPhaseScenario(profile, phase);

                bool momentActive = NarrativeMomentSelection.IsActive(profile, ReadBoolFlag);
                bool resolved = profile.TryResolvePhase(out NpcToneDialoguePhaseDefinition resolvedPhase);
                bool phaseMatched = resolved && resolvedPhase == phase;

                NpcTonePlanningRuntimeState runtimeState = phase.BuildRuntimeState(
                    DefaultCreativity,
                    DefaultCorruption,
                    DefaultEmpathy,
                    DefaultRelationship);

                NpcTonePlannerEvaluation nativeEvaluation = NpcTonePlanningSolvers.Evaluate(
                    runtimeState,
                    profile.plannerMode,
                    profile.ToPlannerSettings(),
                    false,
                    false);

                NpcTonePlannerEvaluation valueEvaluation = NpcTonePlanningSolvers.Evaluate(
                    runtimeState,
                    BaristaPlannerMode.ValueIteration,
                    profile.ToPlannerSettings(),
                    false,
                    false);

                BaristaIntroTone nativeResolvedTone = ResolveTone(profile, nativeEvaluation);
                BaristaIntroTone valueResolvedTone = ResolveTone(profile, valueEvaluation);

                DialogueData nativeDialogue = phase.ResolveDialogue(nativeResolvedTone);
                DialogueData valueDialogue = phase.ResolveDialogue(valueResolvedTone);

                string nativeToneRouting = DescribeToneRouting(phase, nativeResolvedTone, nativeDialogue);
                string valueToneRouting = DescribeToneRouting(phase, valueResolvedTone, valueDialogue);

                results.Add(new ValueIterationAuditResult
                {
                    npcId = profile.NpcIdOrDefault,
                    profileName = profile.name,
                    momentId = profile.MomentIdOrDefault,
                    targetPhaseId = phase.PhaseIdOrDefault,
                    resolvedPhaseId = resolvedPhase != null ? resolvedPhase.PhaseIdOrDefault : "None",
                    phaseMatched = phaseMatched,
                    momentActive = momentActive,
                    nativePlanner = profile.plannerMode.ToString(),
                    nativeAction = nativeEvaluation.bestAction.ToString(),
                    nativeMappedTone = nativeEvaluation.mappedTone.ToString(),
                    nativeResolvedTone = nativeResolvedTone.ToString(),
                    nativeDialogue = nativeDialogue != null ? nativeDialogue.name : "None",
                    nativeToneRouting = nativeToneRouting,
                    valuePlanner = BaristaPlannerMode.ValueIteration.ToString(),
                    valueAction = valueEvaluation.bestAction.ToString(),
                    valueMappedTone = valueEvaluation.mappedTone.ToString(),
                    valueResolvedTone = valueResolvedTone.ToString(),
                    valueDialogue = valueDialogue != null ? valueDialogue.name : "None",
                    valueToneRouting = valueToneRouting,
                    firstSpeaker = GetFirstSpeaker(valueDialogue),
                    firstLine = GetFirstLine(valueDialogue),
                    lineCount = valueDialogue != null && valueDialogue.DialogueLines != null
                        ? valueDialogue.DialogueLines.Count
                        : 0,
                    nativeDebug = nativeEvaluation.BuildDebugString(),
                    valueDebug = valueEvaluation.BuildDebugString()
                });
            }
        }

        return results;
    }

    private static ValueIterationAuditSummary BuildSummary(List<ValueIterationAuditResult> results)
    {
        var summary = new ValueIterationAuditSummary();
        summary.totalScenarios = results.Count;
        summary.phaseMatches = results.Count(r => r.phaseMatched);
        summary.valueExactToneMatches = results.Count(r => string.Equals(r.valueToneRouting, "ExactToneMatch", StringComparison.Ordinal));
        summary.valueFallbackCount = results.Count(r => r.valueToneRouting.StartsWith("Fallback", StringComparison.Ordinal));
        summary.nativeVsValueToneDifferences = results.Count(r => !string.Equals(r.nativeResolvedTone, r.valueResolvedTone, StringComparison.Ordinal));
        return summary;
    }

    private static void ApplyBaselineState()
    {
        NarrativeProgressState.SetCurrentChapterId(string.Empty);
        NarrativeProgressState.SetCurrentMomentId(string.Empty);
        NarrativeProgressState.ClearSceneOverride();

        SetIntFlag(BaristaWelcomeKeys.ReadUnknownText01, false);
        SetIntFlag(BaristaWelcomeKeys.BaristaIntroDone, false);
        SetIntFlag(BaristaWelcomeKeys.AcceptedFirstDrink, false);
        SetIntFlag(BaristaWelcomeKeys.DrankPhotosyntheticDrink, false);
        SetIntFlag(BaristaWelcomeKeys.DrankCola, false);
        SetIntFlag(BaristaWelcomeKeys.HasAlreadyDrink, false);
        SetIntFlag(BaristaWelcomeKeys.DrinkDeliveryPending, false);
        SetIntFlag(BaristaWelcomeKeys.DrinkDeliveryAcknowledged, false);
        PlayerPrefs.SetInt(BaristaWelcomeKeys.PendingDrink, 0);
        PlayerPrefs.SetInt(BaristaWelcomeKeys.HeldDrink, 0);
        PlayerPrefs.SetInt(BaristaWelcomeKeys.BaristaIntroTone, 0);
        PlayerPrefs.SetInt(BaristaWelcomeKeys.BaristaRelationship, 0);
        PlayerPrefs.SetInt("npc_relationship_barista", 0);
        PlayerPrefs.SetInt("npc_relationship_anticar", 0);
        PlayerPrefs.SetInt("npc_relationship_madame_lichenia", 0);

        SetNarrativeFlags(false);
        PlayerPrefs.Save();
    }

    private static void ApplyProfileActivation(NpcToneDialogueProfile profile)
    {
        NarrativeProgressState.SetCurrentChapterId(profile.requiredChapterId ?? string.Empty);
        NarrativeProgressState.SetCurrentMomentId(profile.requiredNarrativeMomentId ?? string.Empty);

        string sceneId = !string.IsNullOrWhiteSpace(profile.requiredSceneId)
            ? profile.requiredSceneId
            : GuessSceneForNpc(profile.NpcIdOrDefault);
        NarrativeProgressState.SetSceneOverride(sceneId);

        ApplyFlagList(profile.requiredTrueFlags, true);
        ApplyFlagList(profile.requiredFalseFlags, false);

        if (profile.skipWhenCompleted && !string.IsNullOrWhiteSpace(profile.completedFlagKey))
            SetIntFlag(profile.completedFlagKey, false);
    }

    private static void ApplyPhaseScenario(
        NpcToneDialogueProfile profile,
        NpcToneDialoguePhaseDefinition phase)
    {
        ApplyConditions(phase.conditions);

        if (phase.matchCurrentReadUnknownText)
            SetIntFlag(BaristaWelcomeKeys.ReadUnknownText01, phase.expectedReadUnknownText);

        if (phase.matchCurrentIntroDone)
            SetIntFlag(BaristaWelcomeKeys.BaristaIntroDone, phase.expectedIntroDone);

        if (phase.matchCurrentPendingDrink || phase.matchCurrentHeldDrink)
        {
            BaristaDrinkType heldDrink = phase.matchCurrentHeldDrink ? phase.heldDrink : BaristaDrinkType.None;
            BaristaDrinkType pendingDrink = phase.matchCurrentPendingDrink ? phase.pendingDrink : BaristaDrinkType.None;
            BaristaWelcomeState.SetDrinkState(heldDrink, pendingDrink);
        }
        else
        {
            BaristaWelcomeState.SetDrinkState(BaristaDrinkType.None, BaristaDrinkType.None);
        }

        if (phase.matchCurrentPendingAcknowledged && phase.pendingDrinkAcknowledged)
            BaristaWelcomeState.AcknowledgePendingDrink();

        if (phase.readUnknownText != null && phase.readUnknownText.source == NpcToneDialogueBooleanSource.PlayerPrefFlag &&
            !string.IsNullOrWhiteSpace(phase.readUnknownText.playerPrefKey))
        {
            SetIntFlag(phase.readUnknownText.playerPrefKey, phase.readUnknownText.fixedValue);
        }

        if (phase.introDone != null && phase.introDone.source == NpcToneDialogueBooleanSource.PlayerPrefFlag &&
            !string.IsNullOrWhiteSpace(phase.introDone.playerPrefKey))
        {
            SetIntFlag(phase.introDone.playerPrefKey, phase.introDone.fixedValue);
        }

        if (phase.readUnknownText != null && phase.readUnknownText.source == NpcToneDialogueBooleanSource.DialogueFlagAsset &&
            phase.readUnknownText.dialogueFlag != null)
        {
            SetIntFlag(phase.readUnknownText.dialogueFlag.FlagKey, phase.readUnknownText.fixedValue);
        }

        if (phase.introDone != null && phase.introDone.source == NpcToneDialogueBooleanSource.DialogueFlagAsset &&
            phase.introDone.dialogueFlag != null)
        {
            SetIntFlag(phase.introDone.dialogueFlag.FlagKey, phase.introDone.fixedValue);
        }

        ApplyProfileSpecificDisambiguation(profile, phase);
        PlayerPrefs.Save();
    }

    private static void ApplyProfileSpecificDisambiguation(
        NpcToneDialogueProfile profile,
        NpcToneDialoguePhaseDefinition phase)
    {
        string npcId = profile.NpcIdOrDefault;
        string phaseId = phase.PhaseIdOrDefault;

        if (string.Equals(npcId, "madame_lichenia", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(phaseId, "Intro", StringComparison.OrdinalIgnoreCase))
                SetIntFlag(NarrativeFlagKeys.TarotReadingCompleted, false);

            if (string.Equals(phaseId, "AfterReadingReferral", StringComparison.OrdinalIgnoreCase))
            {
                SetIntFlag(NarrativeFlagKeys.TarotReadingCompleted, true);
                SetIntFlag(NarrativeFlagKeys.MadameSentToAnticar, false);
                SetIntFlag(NarrativeFlagKeys.BaristaSecretClosureDone, false);
            }

            if (string.Equals(phaseId, "ReferralPending", StringComparison.OrdinalIgnoreCase))
            {
                SetIntFlag(NarrativeFlagKeys.MadameSentToAnticar, true);
                SetIntFlag(NarrativeFlagKeys.BaristaSecretClosureDone, false);
            }

            if (string.Equals(phaseId, "CircuitComplete", StringComparison.OrdinalIgnoreCase))
                SetIntFlag(NarrativeFlagKeys.BaristaSecretClosureDone, true);
        }

        if (string.Equals(npcId, "anticar", StringComparison.OrdinalIgnoreCase))
        {
            SetIntFlag("AnticarLeft", false);

            if (string.Equals(phaseId, "TarotReferral", StringComparison.OrdinalIgnoreCase))
            {
                SetIntFlag(NarrativeFlagKeys.MadameSentToAnticar, true);
                SetIntFlag(NarrativeFlagKeys.AnticarSharedBaristaSecret, false);
                SetIntFlag(NarrativeFlagKeys.BaristaSecretClosureDone, false);
            }

            if (string.Equals(phaseId, "ReferralPending", StringComparison.OrdinalIgnoreCase))
            {
                SetIntFlag(NarrativeFlagKeys.AnticarSharedBaristaSecret, true);
                SetIntFlag(NarrativeFlagKeys.BaristaSecretClosureDone, false);
            }

            if (string.Equals(phaseId, "CircuitComplete", StringComparison.OrdinalIgnoreCase))
                SetIntFlag(NarrativeFlagKeys.BaristaSecretClosureDone, true);
        }

        if (string.Equals(npcId, "barista", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(profile.MomentIdOrDefault, "barista_secret_circuit", StringComparison.OrdinalIgnoreCase))
        {
            SetIntFlag(NarrativeFlagKeys.AnticarSharedBaristaSecret, true);
            SetIntFlag(NarrativeFlagKeys.BaristaSecretClosureDone, false);
            SetIntFlag(BaristaWelcomeKeys.BaristaIntroDone, true);
        }
    }

    private static void ApplyConditions(List<NpcToneDialogueCondition> conditions)
    {
        if (conditions == null)
            return;

        for (int i = 0; i < conditions.Count; i++)
        {
            NpcToneDialogueCondition condition = conditions[i];
            if (condition?.signal == null)
                continue;

            switch (condition.signal.source)
            {
                case NpcToneDialogueBooleanSource.PlayerPrefFlag:
                    if (!string.IsNullOrWhiteSpace(condition.signal.playerPrefKey))
                        SetIntFlag(condition.signal.playerPrefKey, condition.expectedValue);
                    break;

                case NpcToneDialogueBooleanSource.DialogueFlagAsset:
                    if (condition.signal.dialogueFlag != null && !string.IsNullOrWhiteSpace(condition.signal.dialogueFlag.FlagKey))
                        SetIntFlag(condition.signal.dialogueFlag.FlagKey, condition.expectedValue);
                    break;
            }
        }
    }

    private static void ApplyFlagList(IReadOnlyList<string> keys, bool value)
    {
        if (keys == null)
            return;

        for (int i = 0; i < keys.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(keys[i]))
                SetIntFlag(keys[i], value);
        }
    }

    private static void SetNarrativeFlags(bool value)
    {
        SetIntFlag(NarrativeFlagKeys.TarotReadingCompleted, value);
        SetIntFlag(NarrativeFlagKeys.BaristaTarotFollowupDone, value);
        SetIntFlag(NarrativeFlagKeys.BaristaTarotOpenedUp, value);
        SetIntFlag(NarrativeFlagKeys.BaristaTarotDismissed, value);
        SetIntFlag(NarrativeFlagKeys.BaristaTarotProvoked, value);
        SetIntFlag(NarrativeFlagKeys.MadameSentToAnticar, value);
        SetIntFlag(NarrativeFlagKeys.AnticarSharedBaristaSecret, value);
        SetIntFlag(NarrativeFlagKeys.BaristaSecretClosureDone, value);
        SetIntFlag("anticariat_intro_done", value);
        SetIntFlag("barista_tarot_followup_done", value);
        SetIntFlag("AnticarLeft", value);
    }

    private static void SetIntFlag(string key, bool value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

    private static string DescribeToneRouting(
        NpcToneDialoguePhaseDefinition phase,
        BaristaIntroTone resolvedTone,
        DialogueData selectedDialogue)
    {
        if (selectedDialogue == null)
            return "MissingDialogue";

        if (phase.fixedDialogue == selectedDialogue)
            return "FixedDialogue";

        DialogueData exact = GetExactToneDialogue(phase, resolvedTone);
        if (exact == selectedDialogue)
            return "ExactToneMatch";

        if (resolvedTone == BaristaIntroTone.Warm)
            return "FallbackFromWarm";

        if (resolvedTone == BaristaIntroTone.Mischievous)
            return "FallbackFromMischievous";

        return "FallbackFromNeutral";
    }

    private static DialogueData GetExactToneDialogue(
        NpcToneDialoguePhaseDefinition phase,
        BaristaIntroTone tone)
    {
        switch (tone)
        {
            case BaristaIntroTone.Warm:
                return phase.warmDialogue;

            case BaristaIntroTone.Mischievous:
                return phase.mischievousDialogue;

            default:
                return phase.neutralDialogue;
        }
    }

    private static BaristaIntroTone ResolveTone(
        NpcToneDialogueProfile profile,
        NpcTonePlannerEvaluation evaluation)
    {
        switch (profile.toneSelectionMode)
        {
            case NpcToneDialogueToneSelectionMode.ForceWarm:
                return BaristaIntroTone.Warm;

            case NpcToneDialogueToneSelectionMode.ForceMischievous:
                return BaristaIntroTone.Mischievous;

            case NpcToneDialogueToneSelectionMode.ForceNeutral:
                return BaristaIntroTone.Neutral;

            default:
                return BaristaDialogueResolver.NormalizeTone(evaluation.mappedTone);
        }
    }

    private static string GetFirstSpeaker(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.DialogueLines == null || dialogue.DialogueLines.Count == 0 || dialogue.DialogueLines[0] == null)
            return string.Empty;

        return dialogue.DialogueLines[0].Speaker ?? string.Empty;
    }

    private static string GetFirstLine(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.DialogueLines == null || dialogue.DialogueLines.Count == 0 || dialogue.DialogueLines[0] == null)
            return string.Empty;

        string text = dialogue.DialogueLines[0].Text ?? string.Empty;
        return text.Length <= 120 ? text : text.Substring(0, 120) + "...";
    }

    private static string GuessSceneForNpc(string npcId)
    {
        switch (npcId)
        {
            case "barista":
                return "Bar_Interior";

            case "anticar":
                return "Anticariat";

            case "madame_lichenia":
                return "Bar_Exterior";

            default:
                return string.Empty;
        }
    }

    private static bool ReadBoolFlag(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && PlayerPrefs.GetInt(key, 0) == 1;
    }

    private static HashSet<string> BuildTrackedIntKeys(List<NpcToneDialogueProfile> profiles)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);

        keys.Add(BaristaWelcomeKeys.ReadUnknownText01);
        keys.Add(BaristaWelcomeKeys.BaristaIntroDone);
        keys.Add(BaristaWelcomeKeys.AcceptedFirstDrink);
        keys.Add(BaristaWelcomeKeys.DrankPhotosyntheticDrink);
        keys.Add(BaristaWelcomeKeys.DrankCola);
        keys.Add(BaristaWelcomeKeys.HasAlreadyDrink);
        keys.Add(BaristaWelcomeKeys.DrinkDeliveryPending);
        keys.Add(BaristaWelcomeKeys.DrinkDeliveryAcknowledged);
        keys.Add(BaristaWelcomeKeys.PendingDrink);
        keys.Add(BaristaWelcomeKeys.HeldDrink);
        keys.Add(BaristaWelcomeKeys.BaristaIntroTone);
        keys.Add(BaristaWelcomeKeys.BaristaRelationship);
        keys.Add("npc_relationship_barista");
        keys.Add("npc_relationship_anticar");
        keys.Add("npc_relationship_madame_lichenia");
        keys.Add("anticariat_intro_done");
        keys.Add("barista_tarot_followup_done");
        keys.Add("AnticarLeft");

        AddNarrativeFlagKeys(keys);

        for (int i = 0; i < profiles.Count; i++)
        {
            NpcToneDialogueProfile profile = profiles[i];
            AddKeyIfPresent(keys, profile.completedFlagKey);
            AddKeyList(keys, profile.requiredTrueFlags);
            AddKeyList(keys, profile.requiredFalseFlags);
            AddKeyArray(keys, profile.playerPrefFlagsToReset);

            if (profile.dialogueFlagsToReset != null)
            {
                for (int j = 0; j < profile.dialogueFlagsToReset.Length; j++)
                    AddKeyIfPresent(keys, profile.dialogueFlagsToReset[j] != null ? profile.dialogueFlagsToReset[j].FlagKey : null);
            }

            if (profile.phaseDefinitions == null)
                continue;

            for (int phaseIndex = 0; phaseIndex < profile.phaseDefinitions.Count; phaseIndex++)
            {
                NpcToneDialoguePhaseDefinition phase = profile.phaseDefinitions[phaseIndex];
                if (phase == null)
                    continue;

                AddSignalKey(keys, phase.readUnknownText);
                AddSignalKey(keys, phase.introDone);

                if (phase.conditions == null)
                    continue;

                for (int conditionIndex = 0; conditionIndex < phase.conditions.Count; conditionIndex++)
                    AddSignalKey(keys, phase.conditions[conditionIndex] != null ? phase.conditions[conditionIndex].signal : null);
            }
        }

        return keys;
    }

    private static void AddNarrativeFlagKeys(HashSet<string> keys)
    {
        keys.Add(NarrativeFlagKeys.TarotReadingCompleted);
        keys.Add(NarrativeFlagKeys.BaristaTarotFollowupDone);
        keys.Add(NarrativeFlagKeys.BaristaTarotOpenedUp);
        keys.Add(NarrativeFlagKeys.BaristaTarotDismissed);
        keys.Add(NarrativeFlagKeys.BaristaTarotProvoked);
        keys.Add(NarrativeFlagKeys.MadameSentToAnticar);
        keys.Add(NarrativeFlagKeys.AnticarSharedBaristaSecret);
        keys.Add(NarrativeFlagKeys.BaristaSecretClosureDone);
    }

    private static void AddSignalKey(HashSet<string> keys, NpcToneDialogueBooleanSignal signal)
    {
        if (signal == null)
            return;

        if (signal.source == NpcToneDialogueBooleanSource.PlayerPrefFlag)
            AddKeyIfPresent(keys, signal.playerPrefKey);

        if (signal.source == NpcToneDialogueBooleanSource.DialogueFlagAsset && signal.dialogueFlag != null)
            AddKeyIfPresent(keys, signal.dialogueFlag.FlagKey);
    }

    private static void AddKeyList(HashSet<string> keys, IReadOnlyList<string> values)
    {
        if (values == null)
            return;

        for (int i = 0; i < values.Count; i++)
            AddKeyIfPresent(keys, values[i]);
    }

    private static void AddKeyArray(HashSet<string> keys, string[] values)
    {
        if (values == null)
            return;

        for (int i = 0; i < values.Length; i++)
            AddKeyIfPresent(keys, values[i]);
    }

    private static void AddKeyIfPresent(HashSet<string> keys, string key)
    {
        if (!string.IsNullOrWhiteSpace(key))
            keys.Add(key);
    }

    private static string BuildJsonReport(List<ValueIterationAuditResult> results, ValueIterationAuditSummary summary)
    {
        var builder = new StringBuilder();
        builder.AppendLine("{");
        builder.AppendLine("  \"summary\": {");
        builder.AppendLine("    \"totalScenarios\": " + summary.totalScenarios + ",");
        builder.AppendLine("    \"phaseMatches\": " + summary.phaseMatches + ",");
        builder.AppendLine("    \"valueExactToneMatches\": " + summary.valueExactToneMatches + ",");
        builder.AppendLine("    \"valueFallbackCount\": " + summary.valueFallbackCount + ",");
        builder.AppendLine("    \"nativeVsValueToneDifferences\": " + summary.nativeVsValueToneDifferences);
        builder.AppendLine("  },");
        builder.AppendLine("  \"results\": [");

        for (int i = 0; i < results.Count; i++)
        {
            ValueIterationAuditResult result = results[i];
            builder.AppendLine("    {");
            builder.AppendLine("      \"npcId\": " + ToJsonString(result.npcId) + ",");
            builder.AppendLine("      \"profileName\": " + ToJsonString(result.profileName) + ",");
            builder.AppendLine("      \"momentId\": " + ToJsonString(result.momentId) + ",");
            builder.AppendLine("      \"targetPhaseId\": " + ToJsonString(result.targetPhaseId) + ",");
            builder.AppendLine("      \"resolvedPhaseId\": " + ToJsonString(result.resolvedPhaseId) + ",");
            builder.AppendLine("      \"phaseMatched\": " + (result.phaseMatched ? "true" : "false") + ",");
            builder.AppendLine("      \"momentActive\": " + (result.momentActive ? "true" : "false") + ",");
            builder.AppendLine("      \"nativePlanner\": " + ToJsonString(result.nativePlanner) + ",");
            builder.AppendLine("      \"nativeAction\": " + ToJsonString(result.nativeAction) + ",");
            builder.AppendLine("      \"nativeMappedTone\": " + ToJsonString(result.nativeMappedTone) + ",");
            builder.AppendLine("      \"nativeResolvedTone\": " + ToJsonString(result.nativeResolvedTone) + ",");
            builder.AppendLine("      \"nativeDialogue\": " + ToJsonString(result.nativeDialogue) + ",");
            builder.AppendLine("      \"nativeToneRouting\": " + ToJsonString(result.nativeToneRouting) + ",");
            builder.AppendLine("      \"valuePlanner\": " + ToJsonString(result.valuePlanner) + ",");
            builder.AppendLine("      \"valueAction\": " + ToJsonString(result.valueAction) + ",");
            builder.AppendLine("      \"valueMappedTone\": " + ToJsonString(result.valueMappedTone) + ",");
            builder.AppendLine("      \"valueResolvedTone\": " + ToJsonString(result.valueResolvedTone) + ",");
            builder.AppendLine("      \"valueDialogue\": " + ToJsonString(result.valueDialogue) + ",");
            builder.AppendLine("      \"valueToneRouting\": " + ToJsonString(result.valueToneRouting) + ",");
            builder.AppendLine("      \"firstSpeaker\": " + ToJsonString(result.firstSpeaker) + ",");
            builder.AppendLine("      \"firstLine\": " + ToJsonString(result.firstLine) + ",");
            builder.AppendLine("      \"lineCount\": " + result.lineCount + ",");
            builder.AppendLine("      \"nativeDebug\": " + ToJsonString(result.nativeDebug) + ",");
            builder.AppendLine("      \"valueDebug\": " + ToJsonString(result.valueDebug));
            builder.Append("    }");
            builder.AppendLine(i < results.Count - 1 ? "," : string.Empty);
        }

        builder.AppendLine("  ]");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string BuildHtmlReport(List<ValueIterationAuditResult> results, ValueIterationAuditSummary summary)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!DOCTYPE html>");
        builder.AppendLine("<html lang=\"en\">");
        builder.AppendLine("<head>");
        builder.AppendLine("<meta charset=\"utf-8\" />");
        builder.AppendLine("<title>ROAE ValueIteration Audit</title>");
        builder.AppendLine("<style>");
        builder.AppendLine("body{font-family:Segoe UI,Arial,sans-serif;background:#111827;color:#e5e7eb;margin:24px;}");
        builder.AppendLine("h1,h2{margin:0 0 12px 0;}");
        builder.AppendLine(".summary{display:grid;grid-template-columns:repeat(5,minmax(140px,1fr));gap:12px;margin:16px 0 24px 0;}");
        builder.AppendLine(".card{background:#1f2937;border:1px solid #374151;border-radius:8px;padding:12px;}");
        builder.AppendLine(".label{font-size:12px;color:#9ca3af;margin-bottom:4px;}");
        builder.AppendLine(".value{font-size:22px;font-weight:600;}");
        builder.AppendLine("table{width:100%;border-collapse:collapse;background:#111827;}");
        builder.AppendLine("th,td{border:1px solid #374151;padding:8px;vertical-align:top;text-align:left;font-size:12px;}");
        builder.AppendLine("th{background:#1f2937;position:sticky;top:0;}");
        builder.AppendLine(".ok{color:#86efac;font-weight:600;}");
        builder.AppendLine(".warn{color:#fcd34d;font-weight:600;}");
        builder.AppendLine(".bad{color:#fca5a5;font-weight:600;}");
        builder.AppendLine(".mono{font-family:Consolas,monospace;}");
        builder.AppendLine("</style>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine("<h1>ROAE ValueIteration Audit</h1>");
        builder.AppendLine("<p>Exhaustive phase-by-phase audit that forces ValueIteration per NPC profile, compares it with the profile's native planner, and verifies the dialogue slot selected for the resolved tone.</p>");

        builder.AppendLine("<div class=\"summary\">");
        AppendSummaryCard(builder, "Scenarios", summary.totalScenarios.ToString());
        AppendSummaryCard(builder, "Phase Matches", summary.phaseMatches.ToString());
        AppendSummaryCard(builder, "Exact Tone Matches", summary.valueExactToneMatches.ToString());
        AppendSummaryCard(builder, "Fallback Replies", summary.valueFallbackCount.ToString());
        AppendSummaryCard(builder, "Native vs VI Tone Diffs", summary.nativeVsValueToneDifferences.ToString());
        builder.AppendLine("</div>");

        builder.AppendLine("<table>");
        builder.AppendLine("<thead><tr>");
        builder.AppendLine("<th>NPC</th>");
        builder.AppendLine("<th>Profile</th>");
        builder.AppendLine("<th>Moment</th>");
        builder.AppendLine("<th>Target Phase</th>");
        builder.AppendLine("<th>Resolved Phase</th>");
        builder.AppendLine("<th>Phase OK</th>");
        builder.AppendLine("<th>Moment Active</th>");
        builder.AppendLine("<th>Native Planner</th>");
        builder.AppendLine("<th>Native Tone</th>");
        builder.AppendLine("<th>Native Dialogue</th>");
        builder.AppendLine("<th>ValueIteration Tone</th>");
        builder.AppendLine("<th>ValueIteration Dialogue</th>");
        builder.AppendLine("<th>Tone Routing</th>");
        builder.AppendLine("<th>First Speaker</th>");
        builder.AppendLine("<th>First Line</th>");
        builder.AppendLine("<th>VI Debug</th>");
        builder.AppendLine("</tr></thead>");
        builder.AppendLine("<tbody>");

        for (int i = 0; i < results.Count; i++)
        {
            ValueIterationAuditResult result = results[i];
            string phaseClass = result.phaseMatched ? "ok" : "bad";
            string activeClass = result.momentActive ? "ok" : "warn";
            string routingClass = string.Equals(result.valueToneRouting, "ExactToneMatch", StringComparison.Ordinal)
                ? "ok"
                : (result.valueToneRouting.StartsWith("Fallback", StringComparison.Ordinal) ? "warn" : "bad");

            builder.AppendLine("<tr>");
            AppendCell(builder, result.npcId);
            AppendCell(builder, result.profileName);
            AppendCell(builder, result.momentId);
            AppendCell(builder, result.targetPhaseId);
            AppendCell(builder, result.resolvedPhaseId);
            AppendCell(builder, result.phaseMatched ? "YES" : "NO", phaseClass);
            AppendCell(builder, result.momentActive ? "YES" : "NO", activeClass);
            AppendCell(builder, EscapeHtml(result.nativePlanner) + "<br/><span class=\"mono\">" + EscapeHtml(result.nativeAction) + " / " + EscapeHtml(result.nativeResolvedTone) + "</span>", null, true);
            AppendCell(builder, result.nativeMappedTone + " -> " + result.nativeResolvedTone);
            AppendCell(builder, EscapeHtml(result.nativeDialogue) + "<br/><span class=\"mono\">" + EscapeHtml(result.nativeToneRouting) + "</span>", null, true);
            AppendCell(builder, result.valueMappedTone + " -> " + result.valueResolvedTone);
            AppendCell(builder, result.valueDialogue);
            AppendCell(builder, result.valueToneRouting, routingClass);
            AppendCell(builder, result.firstSpeaker);
            AppendCell(builder, result.firstLine);
            AppendCell(builder, "<span class=\"mono\">" + EscapeHtml(result.valueDebug) + "</span>", null, true);
            builder.AppendLine("</tr>");
        }

        builder.AppendLine("</tbody></table>");
        builder.AppendLine("</body></html>");
        return builder.ToString();
    }

    private static void AppendSummaryCard(StringBuilder builder, string label, string value)
    {
        builder.AppendLine("<div class=\"card\">");
        builder.AppendLine("<div class=\"label\">" + EscapeHtml(label) + "</div>");
        builder.AppendLine("<div class=\"value\">" + EscapeHtml(value) + "</div>");
        builder.AppendLine("</div>");
    }

    private static void AppendCell(StringBuilder builder, string text, string cssClass = null, bool rawHtml = false)
    {
        builder.Append("<td");
        if (!string.IsNullOrWhiteSpace(cssClass))
            builder.Append(" class=\"").Append(cssClass).Append("\"");
        builder.Append(">");
        builder.Append(rawHtml ? text : EscapeHtml(text));
        builder.AppendLine("</td>");
    }

    private static string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    private static string ToJsonString(string text)
    {
        if (text == null)
            return "null";

        return "\"" + text
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n") + "\"";
    }

    private static string ResultsFolderAbsolute
    {
        get
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.Combine(projectRoot, ResultsFolderRelative);
        }
    }

    private sealed class AuditPlayerPrefsSnapshot
    {
        private readonly Dictionary<string, StoredIntValue> intValues;
        private readonly string chapterId;
        private readonly string momentId;
        private readonly string sceneOverride;

        private AuditPlayerPrefsSnapshot(
            Dictionary<string, StoredIntValue> intValues,
            string chapterId,
            string momentId,
            string sceneOverride)
        {
            this.intValues = intValues;
            this.chapterId = chapterId;
            this.momentId = momentId;
            this.sceneOverride = sceneOverride;
        }

        public static AuditPlayerPrefsSnapshot Capture(IEnumerable<string> intKeys)
        {
            var values = new Dictionary<string, StoredIntValue>(StringComparer.Ordinal);

            foreach (string key in intKeys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                values[key] = new StoredIntValue(PlayerPrefs.HasKey(key), PlayerPrefs.GetInt(key, 0));
            }

            return new AuditPlayerPrefsSnapshot(
                values,
                NarrativeProgressState.GetCurrentChapterId(),
                NarrativeProgressState.GetCurrentMomentId(),
                PlayerPrefs.GetString("narrative_scene_override", string.Empty));
        }

        public void Restore()
        {
            foreach (KeyValuePair<string, StoredIntValue> pair in intValues)
            {
                if (pair.Value.hasValue)
                    PlayerPrefs.SetInt(pair.Key, pair.Value.value);
                else
                    PlayerPrefs.DeleteKey(pair.Key);
            }

            NarrativeProgressState.SetCurrentChapterId(chapterId);
            NarrativeProgressState.SetCurrentMomentId(momentId);
            NarrativeProgressState.SetSceneOverride(sceneOverride);
            PlayerPrefs.Save();
        }
    }

    private readonly struct StoredIntValue
    {
        public readonly bool hasValue;
        public readonly int value;

        public StoredIntValue(bool hasValue, int value)
        {
            this.hasValue = hasValue;
            this.value = value;
        }
    }

    private sealed class ValueIterationAuditSummary
    {
        public int totalScenarios;
        public int phaseMatches;
        public int valueExactToneMatches;
        public int valueFallbackCount;
        public int nativeVsValueToneDifferences;
    }

    private sealed class ValueIterationAuditResult
    {
        public string npcId;
        public string profileName;
        public string momentId;
        public string targetPhaseId;
        public string resolvedPhaseId;
        public bool phaseMatched;
        public bool momentActive;
        public string nativePlanner;
        public string nativeAction;
        public string nativeMappedTone;
        public string nativeResolvedTone;
        public string nativeDialogue;
        public string nativeToneRouting;
        public string valuePlanner;
        public string valueAction;
        public string valueMappedTone;
        public string valueResolvedTone;
        public string valueDialogue;
        public string valueToneRouting;
        public string firstSpeaker;
        public string firstLine;
        public int lineCount;
        public string nativeDebug;
        public string valueDebug;
    }
}
