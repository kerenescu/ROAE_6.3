using UnityEngine;

public class NpcModularBrain : MonoBehaviour
{
    [SerializeField] private NpcDefinition definition;
    [SerializeField] private bool debugLogs = true;

    public NpcDefinition Definition => definition;

    public NpcDecisionResult Decide()
    {
        NpcDecisionResult result = NpcDecisionService.Decide(definition, debugLogs);

        if (debugLogs)
            Debug.Log("[ROAE][NpcModularBrain] " + result.ToDebugString());

        return result;
    }

    public NpcActionType DecideAction()
    {
        return Decide().action;
    }

    public DialogueData ResolveDialogue()
    {
        return Decide().dialogue;
    }

    [ContextMenu("ROAE/NPC/Print Decision")]
    public void PrintDecision()
    {
        NpcDecisionResult result = Decide();
        Debug.Log("[ROAE][NpcModularBrain][PrintDecision] " + result.ToDebugString());
    }
}
