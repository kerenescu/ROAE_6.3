using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public class NpcModularDialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private NpcModularBrain brain;
    [SerializeField] private NpcDefinition definitionOverride;
    [SerializeField] private bool debugLogs = true;

    public void TriggerDialogue()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        if (dialogueManager == null)
            dialogueManager = Object.FindFirstObjectByType<DialogueManager>();

        if (dialogueManager == null)
        {
            stopwatch.Stop();
            Debug.LogWarning(
                "[ROAE][AI][Dialogue][FAIL] reason=missing_dialogue_manager" +
                " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
            return;
        }

        NpcDecisionResult result = ResolveDecision();
        if (!result.HasDialogue)
        {
            stopwatch.Stop();
            Debug.LogWarning(
                "[ROAE][AI][Dialogue][FAIL] reason=no_dialogue" +
                " " + result.ToDebugString() +
                " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
            return;
        }

        if (debugLogs)
            Debug.Log("[ROAE][NpcModularDialogueTrigger] " + result.ToDebugString());

        dialogueManager.StartDialogue(result.dialogue);
        stopwatch.Stop();

        if (debugLogs)
        {
            Debug.Log(
                "[ROAE][AI][Dialogue][SUCCESS] npc=" +
                (result.definition != null ? result.definition.NpcId : "NULL") +
                " action=" + result.action +
                " dialogue=" + result.dialogue.name +
                " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
        }
    }

    private NpcDecisionResult ResolveDecision()
    {
        if (definitionOverride != null)
            return NpcDecisionService.Decide(definitionOverride, debugLogs);

        if (brain == null)
            brain = GetComponent<NpcModularBrain>();

        if (brain != null)
            return brain.Decide();

        return NpcDecisionService.Decide(null, debugLogs);
    }
}
