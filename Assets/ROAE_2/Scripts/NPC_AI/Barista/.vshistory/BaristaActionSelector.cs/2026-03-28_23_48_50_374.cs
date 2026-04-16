using UnityEngine;

public class BaristaActionSelector : MonoBehaviour
{
    [SerializeField] private string npcId = "barista";
    [SerializeField] private bool debugLog = false;

    public NpcActionType GetAction()
    {
        NpcDecisionState state = NpcStateDiscretizer.Build(npcId);
        NpcActionType action = BaristaPolicy.GetBestAction(state);

        if (debugLog)
            Debug.Log("Barista state: " + state + " | action: " + action);

        return action;
    }
}