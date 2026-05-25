using System;
using System.Collections.Generic;
using UnityEngine;

public enum NpcToneDialogueRoutingKind
{
    Missing = 0,
    ExactToneVariant = 1,
    FallbackToneVariant = 2,
    FixedOnlySingleDialogue = 3
}

public enum NpcToneDialogueBooleanSource
{
    Fixed = 0,
    PlayerPrefFlag = 1,
    DialogueFlagAsset = 2
}

[Serializable]
public class NpcToneDialogueBooleanSignal
{
    public NpcToneDialogueBooleanSource source = NpcToneDialogueBooleanSource.Fixed;
    public bool fixedValue;
    public string playerPrefKey;
    public DialogueFlag dialogueFlag;

    public bool Resolve()
    {
        switch (source)
        {
            case NpcToneDialogueBooleanSource.PlayerPrefFlag:
                return !string.IsNullOrWhiteSpace(playerPrefKey) && PlayerPrefs.GetInt(playerPrefKey, 0) == 1;

            case NpcToneDialogueBooleanSource.DialogueFlagAsset:
                return dialogueFlag != null && dialogueFlag.WasTriggered;

            default:
                return fixedValue;
        }
    }

    public string Describe()
    {
        switch (source)
        {
            case NpcToneDialogueBooleanSource.PlayerPrefFlag:
                return "PlayerPref(" + playerPrefKey + ")";

            case NpcToneDialogueBooleanSource.DialogueFlagAsset:
                return dialogueFlag != null ? "DialogueFlag(" + dialogueFlag.name + ")" : "DialogueFlag(None)";

            default:
                return "Fixed(" + fixedValue + ")";
        }
    }
}

[Serializable]
public class NpcToneDialogueCondition
{
    public string label;
    public NpcToneDialogueBooleanSignal signal = new NpcToneDialogueBooleanSignal();
    public bool expectedValue = true;

    public bool Matches()
    {
        return signal != null && signal.Resolve() == expectedValue;
    }

    public string Describe()
    {
        string prefix = string.IsNullOrWhiteSpace(label) ? string.Empty : label + "=";
        return prefix + (signal != null ? signal.Describe() : "None") + " expected=" + expectedValue;
    }
}

[Serializable]
public class NpcToneDialogueInteractionBlocker
{
    public string failureReason = "interaction_blocked";
    public List<NpcToneDialogueCondition> conditions = new List<NpcToneDialogueCondition>();

    public bool IsBlocked()
    {
        if (conditions == null || conditions.Count == 0)
            return false;

        for (int i = 0; i < conditions.Count; i++)
        {
            NpcToneDialogueCondition condition = conditions[i];
            if (condition == null || !condition.Matches())
                return false;
        }

        return true;
    }
}

[Serializable]
public class NpcToneDialoguePhaseDefinition
{
    public string phaseId = "Intro";
    [TextArea] public string notes;
    public List<NpcToneDialogueCondition> conditions = new List<NpcToneDialogueCondition>();

    [Header("Current State Matching")]
    public bool matchCurrentReadUnknownText;
    public bool expectedReadUnknownText;
    public bool matchCurrentIntroDone;
    public bool expectedIntroDone;
    public bool matchCurrentHasDrink;
    public bool expectedHasDrink;

    [Header("Legacy Drink Matching (Optional)")]
    public bool matchCurrentPendingDrink;
    public bool matchCurrentPendingAcknowledged;
    public bool matchCurrentHeldDrink;

    [Header("Runtime State")]
    public NpcToneDialogueBooleanSignal readUnknownText = new NpcToneDialogueBooleanSignal();
    public NpcToneDialogueBooleanSignal introDone = new NpcToneDialogueBooleanSignal();
    public bool hasDrink;

    [Header("Legacy Drink Runtime (Optional)")]
    public BaristaDrinkType pendingDrink = BaristaDrinkType.None;
    public bool pendingDrinkAcknowledged;
    public BaristaDrinkType heldDrink = BaristaDrinkType.None;

    [Header("Dialogues")]
    public DialogueData fixedDialogue;
    public DialogueData neutralDialogue;
    public DialogueData warmDialogue;
    public DialogueData mischievousDialogue;

    public readonly struct DialogueResolution
    {
        public readonly DialogueData dialogue;
        public readonly NpcToneDialogueRoutingKind routingKind;
        public readonly string sourceToneLabel;

        public DialogueResolution(
            DialogueData dialogue,
            NpcToneDialogueRoutingKind routingKind,
            string sourceToneLabel)
        {
            this.dialogue = dialogue;
            this.routingKind = routingKind;
            this.sourceToneLabel = sourceToneLabel;
        }
    }

    public string PhaseIdOrDefault
    {
        get
        {
            return string.IsNullOrWhiteSpace(phaseId) ? "UnknownPhase" : phaseId;
        }
    }

    public bool Matches()
    {
        return Matches(null, string.Empty);
    }

    public bool Matches(NpcFactContext factContext, string introDoneFlagKeyOverride)
    {
        if (!MatchesConditions())
            return false;

        return MatchesCurrentState(factContext, introDoneFlagKeyOverride);
    }

    public NpcTonePlanningRuntimeState BuildRuntimeState(
        int creativity,
        int corruption,
        int empathy,
        int relationship)
    {
        return new NpcTonePlanningRuntimeState
        {
            readUnknownText = readUnknownText != null && readUnknownText.Resolve(),
            creativity = creativity,
            corruption = corruption,
            empathy = empathy,
            relationship = relationship,
            introDone = introDone != null && introDone.Resolve(),
            hasDrink = hasDrink || heldDrink != BaristaDrinkType.None || pendingDrink != BaristaDrinkType.None,
            pendingDrink = pendingDrink,
            pendingDrinkAcknowledged = pendingDrinkAcknowledged,
            heldDrink = heldDrink
        };
    }

    public NpcTonePlanningRuntimeState BuildRuntimeState(
        NpcFactContext factContext,
        string introDoneFlagKeyOverride)
    {
        if (factContext == null)
        {
            return BuildRuntimeState(
                CreativeStatScale.DefaultCreativity,
                CreativeStatScale.DefaultCorruption,
                CreativeStatScale.DefaultEmpathy,
                0);
        }

        return factContext.ToRuntimeState(this, introDoneFlagKeyOverride);
    }

    public DialogueData ResolveDialogue(BaristaIntroTone tone)
    {
        return ResolveDialogueWithRouting(tone).dialogue;
    }

    public DialogueResolution ResolveDialogueWithRouting(BaristaIntroTone tone)
    {
        DialogueData exactDialogue = GetExactDialogueForTone(tone);
        if (exactDialogue != null)
        {
            return new DialogueResolution(
                exactDialogue,
                NpcToneDialogueRoutingKind.ExactToneVariant,
                ToneLabel(tone));
        }

        DialogueData fallbackDialogue = GetFallbackDialogueForTone(tone, out string fallbackLabel);
        if (fallbackDialogue != null)
        {
            return new DialogueResolution(
                fallbackDialogue,
                NpcToneDialogueRoutingKind.FallbackToneVariant,
                fallbackLabel);
        }

        if (fixedDialogue != null)
        {
            return new DialogueResolution(
                fixedDialogue,
                NpcToneDialogueRoutingKind.FixedOnlySingleDialogue,
                "only_one");
        }

        return new DialogueResolution(null, NpcToneDialogueRoutingKind.Missing, "missing");
    }

    private static DialogueData FirstAssigned(params DialogueData[] options)
    {
        if (options == null)
            return null;

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null)
                return options[i];
        }

        return null;
    }

    public bool HasAnyToneVariants()
    {
        return neutralDialogue != null || warmDialogue != null || mischievousDialogue != null;
    }

    public bool HasCompleteToneVariants()
    {
        return neutralDialogue != null && warmDialogue != null && mischievousDialogue != null;
    }

    private DialogueData GetExactDialogueForTone(BaristaIntroTone tone)
    {
        switch (tone)
        {
            case BaristaIntroTone.Warm:
                return warmDialogue;

            case BaristaIntroTone.Mischievous:
                return mischievousDialogue;

            default:
                return neutralDialogue;
        }
    }

    private DialogueData GetFallbackDialogueForTone(BaristaIntroTone tone, out string fallbackLabel)
    {
        DialogueData fallbackDialogue;

        switch (tone)
        {
            case BaristaIntroTone.Warm:
                fallbackDialogue = FirstAssigned(neutralDialogue, mischievousDialogue);
                fallbackLabel = DetectDialogueLabel(fallbackDialogue);
                return fallbackDialogue;

            case BaristaIntroTone.Mischievous:
                fallbackDialogue = FirstAssigned(neutralDialogue, warmDialogue);
                fallbackLabel = DetectDialogueLabel(fallbackDialogue);
                return fallbackDialogue;

            default:
                fallbackDialogue = FirstAssigned(warmDialogue, mischievousDialogue);
                fallbackLabel = DetectDialogueLabel(fallbackDialogue);
                return fallbackDialogue;
        }
    }

    private string DetectDialogueLabel(DialogueData dialogue)
    {
        if (dialogue == null)
            return "missing";

        if (dialogue == neutralDialogue)
            return "Neutral";

        if (dialogue == warmDialogue)
            return "Warm";

        if (dialogue == mischievousDialogue)
            return "Mischievous";

        if (dialogue == fixedDialogue)
            return "only_one";

        return "unknown";
    }

    private static string ToneLabel(BaristaIntroTone tone)
    {
        switch (tone)
        {
            case BaristaIntroTone.Warm:
                return "Warm";

            case BaristaIntroTone.Mischievous:
                return "Mischievous";

            default:
                return "Neutral";
        }
    }

    private bool MatchesConditions()
    {
        if (conditions == null || conditions.Count == 0)
            return true;

        for (int i = 0; i < conditions.Count; i++)
        {
            NpcToneDialogueCondition condition = conditions[i];
            if (condition == null || !condition.Matches())
                return false;
        }

        return true;
    }

    private bool MatchesCurrentState(NpcFactContext factContext, string introDoneFlagKeyOverride)
    {
        NpcFactContext liveFacts = factContext ?? NpcFactContext.BuildLive("barista");

        if (matchCurrentReadUnknownText)
        {
            bool currentReadUnknownText = liveFacts.readUnknownText;
            if (currentReadUnknownText != expectedReadUnknownText)
                return false;
        }

        if (matchCurrentIntroDone)
        {
            bool currentIntroDone = ResolveLiveBoolean(
                readSignal: introDone,
                fallbackPlayerPrefKey: introDoneFlagKeyOverride,
                fallbackValue: liveFacts.introDone);
            if (currentIntroDone != expectedIntroDone)
                return false;
        }

        if (matchCurrentHasDrink && liveFacts.hasDrink != expectedHasDrink)
            return false;

        if (matchCurrentPendingDrink && liveFacts.pendingDrink != pendingDrink)
            return false;

        if (matchCurrentPendingAcknowledged &&
            liveFacts.pendingDrinkAcknowledged != pendingDrinkAcknowledged)
            return false;

        if (matchCurrentHeldDrink && liveFacts.heldDrink != heldDrink)
            return false;

        return true;
    }

    private static bool ResolveLiveBoolean(
        NpcToneDialogueBooleanSignal readSignal,
        string fallbackPlayerPrefKey,
        bool fallbackValue)
    {
        if (readSignal != null)
        {
            switch (readSignal.source)
            {
                case NpcToneDialogueBooleanSource.PlayerPrefFlag:
                    return !string.IsNullOrWhiteSpace(readSignal.playerPrefKey) &&
                           PlayerPrefs.GetInt(readSignal.playerPrefKey, 0) == 1;

                case NpcToneDialogueBooleanSource.DialogueFlagAsset:
                    return readSignal.dialogueFlag != null && readSignal.dialogueFlag.WasTriggered;
            }
        }

        if (!string.IsNullOrWhiteSpace(fallbackPlayerPrefKey))
            return PlayerPrefs.GetInt(fallbackPlayerPrefKey, 0) == 1;

        return fallbackValue;
    }
}
