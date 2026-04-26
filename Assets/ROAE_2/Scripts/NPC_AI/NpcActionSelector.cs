using UnityEngine;

public class NpcActionSelector : MonoBehaviour
{
    [SerializeField] private string npcId = "npc";
    [SerializeField] private NpcPlannerConfig plannerConfig;
    [SerializeField] private MonoBehaviour customStateProvider;
    [SerializeField] private NpcActionType fallbackAction = NpcActionType.Neutral;
    [SerializeField] private bool debugLog = true;
    [SerializeField] private bool verbosePolicyLogs = false;

    public string NpcId => npcId;
    public NpcPlannerConfig PlannerConfig => plannerConfig;

    public NpcActionType GetAction()
    {
        NpcDecisionState state = BuildState();

        if (plannerConfig == null)
        {
            if (debugLog)
                Debug.LogWarning("[ROAE][NpcActionSelector] Missing planner config on " + name + ". fallback=" + fallbackAction);

            return fallbackAction;
        }

        NpcActionType action = NpcPolicySolver.GetBestAction(plannerConfig, state, verbosePolicyLogs);

        if (debugLog)
        {
            Debug.Log(
                "[ROAE][NpcActionSelector] npcId=" + npcId +
                " state=" + state +
                " action=" + action +
                " planner=" + plannerConfig.name +
                " mode=" + plannerConfig.ToSettings().plannerMode);
        }

        return action;
    }

    [ContextMenu("ROAE/NPC/Rebuild Planner Cache")]
    public void RebuildPlannerCache()
    {
        if (plannerConfig == null)
        {
            Debug.LogWarning("[ROAE][NpcActionSelector] Cannot rebuild cache without planner config.");
            return;
        }

        NpcPolicySolver.Invalidate(plannerConfig);
        NpcPolicySolution solution = NpcPolicySolver.GetOrBuildPolicy(plannerConfig, verbosePolicyLogs);

        if (debugLog)
        {
            Debug.Log(
                "[ROAE][NpcActionSelector] Rebuilt planner cache config=" + plannerConfig.name +
                " states=" + solution.policy.Count +
                " mode=" + solution.plannerMode);
        }
    }

    private NpcDecisionState BuildState()
    {
        if (customStateProvider != null)
        {
            if (customStateProvider is INpcStateProvider provider)
                return provider.BuildDecisionState(npcId);

            if (debugLog)
                Debug.LogWarning("[ROAE][NpcActionSelector] Custom state provider does not implement INpcStateProvider on " + name);
        }

        return NpcStateDiscretizer.Build(npcId);
    }
}
