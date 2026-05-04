using System;
using System.Collections.Generic;
using UnityEngine;

public enum NpcToneDialogueToneSelectionMode
{
    UsePlanner = 0,
    ForceNeutral = 1,
    ForceWarm = 2,
    ForceMischievous = 3
}

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
    public bool matchCurrentPendingDrink;
    public bool matchCurrentPendingAcknowledged;
    public bool matchCurrentHeldDrink;

    [Header("Runtime State")]
    public NpcToneDialogueBooleanSignal readUnknownText = new NpcToneDialogueBooleanSignal();
    public NpcToneDialogueBooleanSignal introDone = new NpcToneDialogueBooleanSignal();
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
        if (!MatchesConditions())
            return false;

        return MatchesCurrentState();
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
            pendingDrink = pendingDrink,
            pendingDrinkAcknowledged = pendingDrinkAcknowledged,
            heldDrink = heldDrink
        };
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

    private bool MatchesCurrentState()
    {
        if (matchCurrentReadUnknownText)
        {
            bool currentReadUnknownText = ResolveLiveBoolean(readUnknownText, BaristaWelcomeKeys.ReadUnknownText01);
            if (currentReadUnknownText != expectedReadUnknownText)
                return false;
        }

        if (matchCurrentIntroDone)
        {
            bool currentIntroDone = ResolveLiveBoolean(introDone, BaristaWelcomeKeys.BaristaIntroDone);
            if (currentIntroDone != expectedIntroDone)
                return false;
        }

        if (matchCurrentPendingDrink && BaristaWelcomeState.GetPendingDrink() != pendingDrink)
            return false;

        if (matchCurrentPendingAcknowledged &&
            BaristaWelcomeState.HasAcknowledgedPendingDrink() != pendingDrinkAcknowledged)
            return false;

        if (matchCurrentHeldDrink && BaristaWelcomeState.GetHeldDrink() != heldDrink)
            return false;

        return true;
    }

    private static bool ResolveLiveBoolean(NpcToneDialogueBooleanSignal signal, string fallbackPlayerPrefKey)
    {
        if (signal != null)
        {
            switch (signal.source)
            {
                case NpcToneDialogueBooleanSource.PlayerPrefFlag:
                    return !string.IsNullOrWhiteSpace(signal.playerPrefKey) &&
                           PlayerPrefs.GetInt(signal.playerPrefKey, 0) == 1;

                case NpcToneDialogueBooleanSource.DialogueFlagAsset:
                    return signal.dialogueFlag != null && signal.dialogueFlag.WasTriggered;
            }
        }

        return !string.IsNullOrWhiteSpace(fallbackPlayerPrefKey) &&
               PlayerPrefs.GetInt(fallbackPlayerPrefKey, 0) == 1;
    }
}

[CreateAssetMenu(fileName = "NpcToneDialogueProfile", menuName = "ROAE/Narrative V2/NPC Tone Dialogue Profile")]
public class NpcToneDialogueProfile : ScriptableObject, INarrativeMomentDefinition
{
    [Header("Identity")]
    public string npcId = "npc";
    public string momentId = "";
    public string outcomeLogTag = "NpcOutcome";
    public string dialogueLogTag = "NpcDialogue";
    public string devSummaryLogTag = "NpcDev";
    public string devResetLogTag = "NpcDevReset";

    [Header("Moment Activation")]
    public bool isEnabled = true;
    public int priority = 0;
    public string requiredChapterId = "";
    public string requiredSceneId = "";
    public string requiredNarrativeMomentId = "";
    public List<string> requiredTrueFlags = new List<string>();
    public List<string> requiredFalseFlags = new List<string>();
    public bool skipWhenCompleted = false;
    public string completedFlagKey = "";

    [Header("Planner")]
    public BaristaPlannerMode plannerMode = BaristaPlannerMode.PolicyIteration;
    [Range(0f, 0.99f)] public float gamma = 0.85f;
    public float epsilon = 0.0001f;
    [Min(1)] public int maxValueIterations = 96;
    [Min(1)] public int maxPolicyIterations = 24;
    [Min(1)] public int maxPolicyEvaluationSweeps = 96;
    public NpcToneDialogueToneSelectionMode toneSelectionMode = NpcToneDialogueToneSelectionMode.UsePlanner;

    [Header("Reset")]
    public string[] resetNpcIds = { "barista", "anticar", "madame_lichenia" };
    public string[] playerPrefFlagsToReset = new string[0];
    public DialogueFlag[] dialogueFlagsToReset = new DialogueFlag[0];

    [Header("Interaction")]
    public List<NpcToneDialogueInteractionBlocker> interactionBlockers = new List<NpcToneDialogueInteractionBlocker>();
    public List<NpcToneDialoguePhaseDefinition> phaseDefinitions = new List<NpcToneDialoguePhaseDefinition>();

    public string NpcIdOrDefault
    {
        get
        {
            return string.IsNullOrWhiteSpace(npcId) ? name : npcId.Trim();
        }
    }

    public string MomentIdOrDefault
    {
        get
        {
            return string.IsNullOrWhiteSpace(momentId) ? name : momentId.Trim();
        }
    }

    public string MomentId => MomentIdOrDefault;
    public bool IsEnabled => isEnabled;
    public int Priority => priority;
    public string RequiredChapterId => requiredChapterId;
    public string RequiredSceneId => requiredSceneId;
    public string RequiredNarrativeMomentId => requiredNarrativeMomentId;
    public IReadOnlyList<string> RequiredTrueFlags => requiredTrueFlags;
    public IReadOnlyList<string> RequiredFalseFlags => requiredFalseFlags;
    public bool SkipWhenCompleted => skipWhenCompleted;
    public string CompletedFlagKey => completedFlagKey;

    public string OutcomeLogTagOrDefault(string fallbackPrefix)
    {
        return string.IsNullOrWhiteSpace(outcomeLogTag) ? fallbackPrefix + "Outcome" : outcomeLogTag;
    }

    public string DialogueLogTagOrDefault(string fallbackPrefix)
    {
        return string.IsNullOrWhiteSpace(dialogueLogTag) ? fallbackPrefix + "Dialogue" : dialogueLogTag;
    }

    public string DevSummaryLogTagOrDefault(string fallbackPrefix)
    {
        return string.IsNullOrWhiteSpace(devSummaryLogTag) ? fallbackPrefix + "Dev" : devSummaryLogTag;
    }

    public string DevResetLogTagOrDefault(string fallbackPrefix)
    {
        return string.IsNullOrWhiteSpace(devResetLogTag) ? fallbackPrefix + "DevReset" : devResetLogTag;
    }

    public NpcTonePlannerSettings ToPlannerSettings()
    {
        return new NpcTonePlannerSettings(
            Mathf.Clamp(gamma, 0f, 0.99f),
            Mathf.Max(0.00001f, epsilon),
            Mathf.Max(1, maxValueIterations),
            Mathf.Max(1, maxPolicyIterations),
            Mathf.Max(1, maxPolicyEvaluationSweeps));
    }

    public bool TryGetBlockingReason(out string failureReason)
    {
        if (interactionBlockers != null)
        {
            for (int i = 0; i < interactionBlockers.Count; i++)
            {
                NpcToneDialogueInteractionBlocker blocker = interactionBlockers[i];
                if (blocker != null && blocker.IsBlocked())
                {
                    failureReason = string.IsNullOrWhiteSpace(blocker.failureReason)
                        ? "interaction_blocked"
                        : blocker.failureReason;
                    return true;
                }
            }
        }

        failureReason = string.Empty;
        return false;
    }

    public bool TryResolvePhase(out NpcToneDialoguePhaseDefinition phase)
    {
        if (phaseDefinitions != null)
        {
            for (int i = 0; i < phaseDefinitions.Count; i++)
            {
                NpcToneDialoguePhaseDefinition candidate = phaseDefinitions[i];
                if (candidate != null && candidate.Matches())
                {
                    phase = candidate;
                    return true;
                }
            }
        }

        phase = null;
        return false;
    }

    public bool MatchesActivation()
    {
        return NarrativeMomentSelection.IsActive(this, ReadPlayerPrefFlag);
    }

    private static bool ReadPlayerPrefFlag(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && PlayerPrefs.GetInt(key, 0) == 1;
    }
}
