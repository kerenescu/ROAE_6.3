using System.Collections.Generic;
using UnityEngine;

public enum BaristaMomentMode
{
    SingleDialogue = 0,
    StandardBaristaLoop = 1
}

public enum BaristaMomentToneMode
{
    UseBrainOnIntroAndStoredLoop = 0,
    ForceNeutral = 1,
    ForceWarm = 2,
    ForceMischievous = 3
}

[CreateAssetMenu(fileName = "BaristaMomentDefinition", menuName = "ROAE/Barista/Moment Definition")]
public class BaristaMomentDefinition : ScriptableObject, INarrativeMomentDefinition, IBaristaDialogueCollection
{
    [Header("Identity")]
    [SerializeField] private string momentId = "barista_moment";
    [SerializeField] private bool isEnabled = true;
    [SerializeField] private int priority = 0;

    [Header("Activation gates")]
    [SerializeField] private string requiredChapterId = "";
    [SerializeField] private string requiredSceneId = "";
    [SerializeField] private string requiredNarrativeMomentId = "";
    [SerializeField] private List<string> requiredTrueFlags = new List<string>();
    [SerializeField] private List<string> requiredFalseFlags = new List<string>();
    [SerializeField] private bool skipWhenCompleted = false;
    [SerializeField] private string completedFlagKey = "";
    [SerializeField] private string introCompletionFlagKey = "";

    [Header("Behavior")]
    [SerializeField] private BaristaMomentMode mode = BaristaMomentMode.StandardBaristaLoop;
    [SerializeField] private BaristaMomentToneMode toneMode = BaristaMomentToneMode.UseBrainOnIntroAndStoredLoop;

    [Header("Single dialogue mode")]
    [SerializeField] private DialogueData singleDialogue;

    [Header("Intro loop")]
    [SerializeField] private DialogueData neutralIntroDialogue;
    [SerializeField] private DialogueData warmIntroDialogue;
    [SerializeField] private DialogueData mischievousIntroDialogue;

    [Header("Order loop")]
    [SerializeField] private DialogueData neutralOrderMenuDialogue;
    [SerializeField] private DialogueData warmOrderMenuDialogue;
    [SerializeField] private DialogueData mischievousOrderMenuDialogue;

    [Header("Preparing loop")]
    [SerializeField] private DialogueData neutralPreparingDialogue;
    [SerializeField] private DialogueData warmPreparingDialogue;
    [SerializeField] private DialogueData mischievousPreparingDialogue;

    [Header("Preparing reminder loop")]
    [SerializeField] private DialogueData neutralPendingReminderDialogue;
    [SerializeField] private DialogueData warmPendingReminderDialogue;
    [SerializeField] private DialogueData mischievousPendingReminderDialogue;

    [Header("Held drink loop")]
    [SerializeField] private DialogueData alreadyHasColaDialogue;
    [SerializeField] private DialogueData alreadyHasSapDialogue;
    [SerializeField] private DialogueData genericAlreadyHasDrinkDialogue;

    public string MomentId => momentId;
    public bool IsEnabled => isEnabled;
    public int Priority => priority;
    public string RequiredChapterId => requiredChapterId;
    public string RequiredSceneId => requiredSceneId;
    public string RequiredNarrativeMomentId => requiredNarrativeMomentId;
    public IReadOnlyList<string> RequiredTrueFlags => requiredTrueFlags;
    public IReadOnlyList<string> RequiredFalseFlags => requiredFalseFlags;
    public bool SkipWhenCompleted => skipWhenCompleted;
    public string CompletedFlagKey => completedFlagKey;
    public string IntroCompletionFlagKey => introCompletionFlagKey;
    public BaristaMomentMode Mode => mode;
    public BaristaMomentToneMode ToneMode => toneMode;

    public DialogueData SingleDialogue => singleDialogue;

    public DialogueData NeutralIntroDialogue => neutralIntroDialogue;
    public DialogueData WarmIntroDialogue => warmIntroDialogue;
    public DialogueData MischievousIntroDialogue => mischievousIntroDialogue;

    public DialogueData NeutralOrderMenuDialogue => neutralOrderMenuDialogue;
    public DialogueData WarmOrderMenuDialogue => warmOrderMenuDialogue;
    public DialogueData MischievousOrderMenuDialogue => mischievousOrderMenuDialogue;

    public DialogueData NeutralPreparingDialogue => neutralPreparingDialogue;
    public DialogueData WarmPreparingDialogue => warmPreparingDialogue;
    public DialogueData MischievousPreparingDialogue => mischievousPreparingDialogue;

    public DialogueData NeutralPendingReminderDialogue => neutralPendingReminderDialogue;
    public DialogueData WarmPendingReminderDialogue => warmPendingReminderDialogue;
    public DialogueData MischievousPendingReminderDialogue => mischievousPendingReminderDialogue;

    public DialogueData AlreadyHasColaDialogue => alreadyHasColaDialogue;
    public DialogueData AlreadyHasSapDialogue => alreadyHasSapDialogue;
    public DialogueData GenericAlreadyHasDrinkDialogue => genericAlreadyHasDrinkDialogue;
}
