using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class NpcToneDialogueToneBackfillUtility
{
    private static readonly string[] ProfileFolders =
    {
        "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Profiles",
        "Assets/ROAE_2/Data/NarrativeV2/MadameLichenia/Profiles",
        "Assets/ROAE_2/Data/NarrativeV2/Anticariat/Profiles"
    };

    public readonly struct BackfillSummary
    {
        public readonly int profilesTouched;
        public readonly int phasesTouched;
        public readonly int assetsCreated;
        public readonly int toneAssignments;
        public readonly int fixedDialoguesCleared;

        public BackfillSummary(
            int profilesTouched,
            int phasesTouched,
            int assetsCreated,
            int toneAssignments,
            int fixedDialoguesCleared)
        {
            this.profilesTouched = profilesTouched;
            this.phasesTouched = phasesTouched;
            this.assetsCreated = assetsCreated;
            this.toneAssignments = toneAssignments;
            this.fixedDialoguesCleared = fixedDialoguesCleared;
        }
    }

    public static BackfillSummary BackfillAllProfiles(bool logSummary)
    {
        string[] guids = AssetDatabase.FindAssets("t:NpcToneDialogueProfile", ProfileFolders);
        int profilesTouched = 0;
        int phasesTouched = 0;
        int assetsCreated = 0;
        int toneAssignments = 0;
        int fixedDialoguesCleared = 0;

        for (int i = 0; i < guids.Length; i++)
        {
            string profilePath = AssetDatabase.GUIDToAssetPath(guids[i]);
            NpcToneDialogueProfile profile = AssetDatabase.LoadAssetAtPath<NpcToneDialogueProfile>(profilePath);
            if (profile == null)
                continue;

            bool profileChanged = false;
            if (BackfillProfile(
                    profile,
                    profilePath,
                    ref phasesTouched,
                    ref assetsCreated,
                    ref toneAssignments,
                    ref fixedDialoguesCleared))
            {
                profilesTouched++;
                profileChanged = true;
            }

            if (profileChanged)
            {
                EditorUtility.SetDirty(profile);
                Undo.RecordObject(profile, "Backfill NPC Tone Dialogue Variants");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        BackfillSummary summary = new BackfillSummary(
            profilesTouched,
            phasesTouched,
            assetsCreated,
            toneAssignments,
            fixedDialoguesCleared);

        if (logSummary)
        {
            Debug.Log(
                "[ROAE][AI][ToneBackfill][SUCCESS] profilesTouched=" + summary.profilesTouched +
                " phasesTouched=" + summary.phasesTouched +
                " assetsCreated=" + summary.assetsCreated +
                " toneAssignments=" + summary.toneAssignments +
                " fixedDialoguesCleared=" + summary.fixedDialoguesCleared);
        }

        return summary;
    }

    public static void RunBackfillBatch()
    {
        BackfillAllProfiles(true);
        EditorApplication.Exit(0);
    }

    private static bool BackfillProfile(
        NpcToneDialogueProfile profile,
        string profilePath,
        ref int phasesTouched,
        ref int assetsCreated,
        ref int toneAssignments,
        ref int fixedDialoguesCleared)
    {
        if (profile.phaseDefinitions == null || profile.phaseDefinitions.Count == 0)
            return false;

        bool changed = false;

        for (int i = 0; i < profile.phaseDefinitions.Count; i++)
        {
            NpcToneDialoguePhaseDefinition phase = profile.phaseDefinitions[i];
            if (phase == null)
                continue;

            DialogueData source = SelectBackfillSource(phase);
            if (source == null)
                continue;

            bool phaseChanged = false;

            phaseChanged |= EnsureToneDialogue(
                ref phase.neutralDialogue,
                source,
                profilePath,
                phase.PhaseIdOrDefault,
                "Neutral",
                ref assetsCreated,
                ref toneAssignments);

            phaseChanged |= EnsureToneDialogue(
                ref phase.warmDialogue,
                source,
                profilePath,
                phase.PhaseIdOrDefault,
                "Warm",
                ref assetsCreated,
                ref toneAssignments);

            phaseChanged |= EnsureToneDialogue(
                ref phase.mischievousDialogue,
                source,
                profilePath,
                phase.PhaseIdOrDefault,
                "Mischievous",
                ref assetsCreated,
                ref toneAssignments);

            if (phase.fixedDialogue != null &&
                phase.neutralDialogue != null &&
                phase.warmDialogue != null &&
                phase.mischievousDialogue != null)
            {
                phase.fixedDialogue = null;
                fixedDialoguesCleared++;
                phaseChanged = true;
            }

            if (phaseChanged)
            {
                phasesTouched++;
                changed = true;
            }
        }

        return changed;
    }

    private static DialogueData SelectBackfillSource(NpcToneDialoguePhaseDefinition phase)
    {
        if (phase.fixedDialogue != null)
            return phase.fixedDialogue;

        if (phase.neutralDialogue != null)
            return phase.neutralDialogue;

        if (phase.warmDialogue != null)
            return phase.warmDialogue;

        return phase.mischievousDialogue;
    }

    private static bool EnsureToneDialogue(
        ref DialogueData slot,
        DialogueData source,
        string profilePath,
        string phaseId,
        string toneLabel,
        ref int assetsCreated,
        ref int toneAssignments)
    {
        if (slot != null)
            return false;

        string sourcePath = AssetDatabase.GetAssetPath(source);
        if (string.IsNullOrWhiteSpace(sourcePath))
            return false;

        string targetPath = BuildTargetPath(sourcePath, profilePath, phaseId, toneLabel);
        DialogueData asset = AssetDatabase.LoadAssetAtPath<DialogueData>(targetPath);
        if (asset == null)
        {
            if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                return false;

            assetsCreated++;
            asset = AssetDatabase.LoadAssetAtPath<DialogueData>(targetPath);
        }

        if (asset == null)
            return false;

        slot = asset;
        toneAssignments++;
        return true;
    }

    private static string BuildTargetPath(
        string sourcePath,
        string profilePath,
        string phaseId,
        string toneLabel)
    {
        string sourceFolder = Path.GetDirectoryName(sourcePath)?.Replace('\\', '/');
        string profileName = Path.GetFileNameWithoutExtension(profilePath);
        string sourceName = StripToneSuffix(Path.GetFileNameWithoutExtension(sourcePath));
        string safePhaseId = SanitizeToken(phaseId);
        string safeTone = SanitizeToken(toneLabel);

        string fileName = sourceName + "__" + profileName + "__" + safePhaseId + "__" + safeTone + ".asset";
        return sourceFolder + "/" + fileName;
    }

    private static string StripToneSuffix(string sourceName)
    {
        if (sourceName.EndsWith("_Neutral", StringComparison.OrdinalIgnoreCase))
            return sourceName.Substring(0, sourceName.Length - "_Neutral".Length);

        if (sourceName.EndsWith("_Warm", StringComparison.OrdinalIgnoreCase))
            return sourceName.Substring(0, sourceName.Length - "_Warm".Length);

        if (sourceName.EndsWith("_Mischievous", StringComparison.OrdinalIgnoreCase))
            return sourceName.Substring(0, sourceName.Length - "_Mischievous".Length);

        return sourceName;
    }

    private static string SanitizeToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Unknown";

        char[] chars = value.Trim().ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_' && chars[i] != '-')
                chars[i] = '_';
        }

        return new string(chars);
    }
}
