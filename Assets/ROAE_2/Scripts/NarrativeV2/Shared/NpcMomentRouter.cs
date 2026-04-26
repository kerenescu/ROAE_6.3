using System.Collections.Generic;
using UnityEngine;

public class NpcMomentRouter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private BaristaWelcomeBrain baristaBrain;

    [Header("Moments")]
    [SerializeField] private List<BaristaMomentDefinition> baristaMoments = new List<BaristaMomentDefinition>();

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    public void TriggerDialogue()
    {
        if (dialogueManager == null)
            dialogueManager = Object.FindFirstObjectByType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogWarning("[ROAE][NpcMomentRouter] Missing DialogueManager.");
            return;
        }

        BaristaMomentDefinition activeMoment = ResolveActiveBaristaMoment();
        if (activeMoment == null)
        {
            Debug.LogWarning("[ROAE][NpcMomentRouter] No active barista moment matched.");
            return;
        }

        DialogueData selectedDialogue;
        BaristaDialogueResolution resolution;
        if (activeMoment.Mode == BaristaMomentMode.SingleDialogue)
        {
            selectedDialogue = activeMoment.SingleDialogue;
            resolution = new BaristaDialogueResolution(
                BaristaDialogueLoop.Intro,
                BaristaDialogueResolver.NormalizeTone(BaristaWelcomeState.GetIntroTone()),
                selectedDialogue,
                "single_dialogue_mode");
        }
        else
        {
            resolution = BaristaDialogueResolver.Resolve(activeMoment, baristaBrain, activeMoment.ToneMode);
            selectedDialogue = resolution.dialogue;
        }

        if (selectedDialogue == null)
        {
            Debug.LogWarning("[ROAE][NpcMomentRouter] Active moment '" + activeMoment.MomentId + "' returned NULL dialogue.");
            return;
        }

        Log(
            "TriggerDialogue | moment=" + activeMoment.MomentId +
            " | mode=" + activeMoment.Mode +
            " | loop=" + resolution.loop +
            " | tone=" + resolution.tone +
            " | reason=" + resolution.reason +
            " | dialogue=" + selectedDialogue.name +
            " | chapter=" + NarrativeProgressState.GetCurrentChapterId() +
            " | scene=" + NarrativeProgressState.GetCurrentSceneId() +
            " | narrativeMoment=" + NarrativeProgressState.GetCurrentMomentId() +
            " | heldDrink=" + BaristaWelcomeState.GetHeldDrink() +
            " | pendingDrink=" + BaristaWelcomeState.GetPendingDrink() +
            " | introDone=" + BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.BaristaIntroDone) +
            " | accepted=" + BaristaWelcomeState.HasAcceptedFirstDrink() +
            " | storedTone=" + BaristaWelcomeState.GetIntroTone()
        );

        dialogueManager.StartDialogue(selectedDialogue);
    }

    private BaristaMomentDefinition ResolveActiveBaristaMoment()
    {
        BaristaMomentDefinition best = NarrativeMomentSelection.ResolveHighestPriorityActiveMoment(
            baristaMoments,
            ReadBoolFlag);

        if (best != null)
            Log("Active moment resolved: " + best.MomentId + " | priority=" + best.Priority);

        return best;
    }

    private static bool ReadBoolFlag(string key)
    {
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    private void Log(string message)
    {
        if (!debugLog)
            return;

        Debug.Log("[ROAE][NpcMomentRouter] " + message);
    }
}
