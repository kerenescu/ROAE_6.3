using System.Collections.Generic;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public class NpcMomentRouter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private BaristaWelcomeBrain baristaBrain;

    [Header("Moments")]
    [SerializeField] private List<BaristaMomentDefinition> baristaMoments = new List<BaristaMomentDefinition>();

    [Header("Debug")]
    [SerializeField] private bool auditLogs = true;
    [SerializeField] private bool verboseLogs = false;

    public void TriggerDialogue()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        if (dialogueManager == null)
            dialogueManager = Object.FindFirstObjectByType<DialogueManager>();

        if (dialogueManager == null)
        {
            stopwatch.Stop();
            if (auditLogs)
            {
                Debug.LogWarning(
                    "[ROAE][AI][BaristaDialogue][FAIL] reason=missing_dialogue_manager" +
                    " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
            }
            return;
        }

        BaristaMomentDefinition activeMoment = ResolveActiveBaristaMoment();
        if (activeMoment == null)
        {
            stopwatch.Stop();
            if (auditLogs)
            {
                Debug.LogWarning(
                    "[ROAE][AI][BaristaDialogue][FAIL] reason=no_active_moment" +
                    " chapter=" + NarrativeProgressState.GetCurrentChapterId() +
                    " scene=" + NarrativeProgressState.GetCurrentSceneId() +
                    " narrativeMoment=" + NarrativeProgressState.GetCurrentMomentId() +
                    " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
            }
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
            stopwatch.Stop();
            if (auditLogs)
            {
                Debug.LogWarning(
                    "[ROAE][AI][BaristaDialogue][FAIL] reason=null_dialogue" +
                    " moment=" + activeMoment.MomentId +
                    " loop=" + resolution.loop +
                    " tone=" + resolution.tone +
                    " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
            }
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
        stopwatch.Stop();

        if (auditLogs)
        {
            Debug.Log(
                "[ROAE][AI][BaristaDialogue][SUCCESS]" +
                " moment=" + activeMoment.MomentId +
                " loop=" + resolution.loop +
                " tone=" + resolution.tone +
                " dialogue=" + selectedDialogue.name +
                " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
        }
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
        if (!verboseLogs)
            return;

        Debug.Log("[ROAE][NpcMomentRouter] " + message);
    }
}
