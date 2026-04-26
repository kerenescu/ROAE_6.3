using UnityEngine;

public class BaristaActionSelector : MonoBehaviour
{
    [SerializeField] private string npcId = "barista";
    [SerializeField] private bool debugLog = true;

    public NpcActionType GetAction()
    {
        NpcDecisionState state = NpcStateDiscretizer.Build(npcId);
        if (debugLog)
            BaristaDebug.Log("BaristaActionSelector.GetAction", "npcId=" + npcId + " discretizedState=" + state);

        NpcActionType action = BaristaPolicy.GetBestAction(state);

        if (debugLog)
            BaristaDebug.Log("BaristaActionSelector.GetAction", "npcId=" + npcId + " resultAction=" + action + " source=BaristaPolicy.GetBestAction");

        return action;
    }
}
