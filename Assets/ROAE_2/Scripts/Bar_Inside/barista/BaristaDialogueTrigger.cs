using UnityEngine;

public class BaristaDialogueTrigger : MonoBehaviour, IBaristaDialogueCollection
{
    [Header("Dependencies")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private BaristaWelcomeBrain brain;

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

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    public DialogueData NeutralIntroDialogue => neutralIntroDialogue;
    public DialogueData WarmIntroDialogue => warmIntroDialogue;
    public DialogueData MischievousIntroDialogue => mischievousIntroDialogue;

    public DialogueData NeutralOrderMenuDialogue => neutralOrderMenuDialogue;
    public DialogueData WarmOrderMenuDialogue => warmOrderMenuDialogue;
    public DialogueData MischievousOrderMenuDialogue => mischievousOrderMenuDialogue;

    public DialogueData NeutralPreparingDialogue => neutralPreparingDialogue;
    public DialogueData WarmPreparingDialogue => warmPreparingDialogue;
    public DialogueData MischievousPreparingDialogue => mischievousPreparingDialogue;

    public DialogueData NeutralPendingReminderDialogue => neutralPendingReminderDialogue != null ? neutralPendingReminderDialogue : neutralPreparingDialogue;
    public DialogueData WarmPendingReminderDialogue => warmPendingReminderDialogue != null ? warmPendingReminderDialogue : warmPreparingDialogue;
    public DialogueData MischievousPendingReminderDialogue => mischievousPendingReminderDialogue != null ? mischievousPendingReminderDialogue : mischievousPreparingDialogue;

    public DialogueData AlreadyHasColaDialogue => alreadyHasColaDialogue;
    public DialogueData AlreadyHasSapDialogue => alreadyHasSapDialogue;
    public DialogueData GenericAlreadyHasDrinkDialogue => genericAlreadyHasDrinkDialogue;

    private void Awake()
    {
        if (dialogueManager == null)
            dialogueManager = Object.FindFirstObjectByType<DialogueManager>();

        if (debugLog)
        {
            Debug.Log(
                "[ROAE][BaristaDialogueTrigger] Awake | dialogueManager=" +
                (dialogueManager != null ? dialogueManager.name : "NULL") +
                " | brain=" +
                (brain != null ? brain.name : "NULL"));
        }
    }

    public void TriggerDialogue()
    {
        NpcMomentRouter router = GetComponent<NpcMomentRouter>();
        if (router != null && router.enabled)
        {
            if (debugLog)
                Debug.Log("[ROAE][BaristaDialogueTrigger] Delegating to NpcMomentRouter.");

            router.TriggerDialogue();
            return;
        }

        if (dialogueManager == null)
            dialogueManager = Object.FindFirstObjectByType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogWarning("[ROAE][BaristaDialogueTrigger] Missing DialogueManager.");
            return;
        }

        BaristaDialogueResolution resolution = BaristaDialogueResolver.Resolve(
            this,
            brain,
            BaristaMomentToneMode.UseBrainOnIntroAndStoredLoop);

        DialogueData selected = resolution.dialogue;
        if (selected == null)
        {
            Debug.LogWarning("[ROAE][BaristaDialogueTrigger] No dialogue selected.");
            return;
        }

        if (debugLog)
        {
            Debug.Log(
                "[ROAE][BaristaDialogueTrigger] TriggerDialogue -> " + selected.name +
                " | loop=" + resolution.loop +
                " | tone=" + resolution.tone +
                " | reason=" + resolution.reason +
                " | introDone=" + BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.BaristaIntroDone) +
                " | acceptedFirstDrink=" + BaristaWelcomeState.HasAcceptedFirstDrink() +
                " | heldDrink=" + BaristaWelcomeState.GetHeldDrink() +
                " | pendingDrink=" + BaristaWelcomeState.GetPendingDrink() +
                " | storedTone=" + BaristaWelcomeState.GetIntroTone());
        }

        dialogueManager.StartDialogue(selected);
    }
}
