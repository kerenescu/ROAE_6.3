using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

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
        Stopwatch stopwatch = Stopwatch.StartNew();
        NpcDecisionState state = BuildState();

        if (plannerConfig == null)
        {
            stopwatch.Stop();
            if (debugLog)
            {
                Debug.LogWarning(
                    "[ROAE][AI][NpcActionSelector][FAIL] npcId=" + npcId +
                    " reason=missing_planner_config" +
                    " state=" + state +
                    " fallback=" + fallbackAction +
                    " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
            }

            return fallbackAction;
        }

        NpcActionType action = NpcPolicySolver.GetBestAction(plannerConfig, state, verbosePolicyLogs, debugLog);
        stopwatch.Stop();

        if (debugLog)
        {
            Debug.Log(
                "[ROAE][AI][NpcActionSelector][SUCCESS] npcId=" + npcId +
                " state=" + state +
                " action=" + action +
                " planner=" + plannerConfig.name +
                " mode=" + plannerConfig.ToSettings().plannerMode +
                " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
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
        Stopwatch stopwatch = Stopwatch.StartNew();
        NpcPolicySolution solution = NpcPolicySolver.GetOrBuildPolicy(plannerConfig, verbosePolicyLogs, debugLog);
        stopwatch.Stop();

        if (debugLog)
        {
            Debug.Log(
                "[ROAE][AI][NpcActionSelector][SUCCESS] rebuiltPlannerCache config=" + plannerConfig.name +
                " states=" + solution.policy.Count +
                " mode=" + solution.plannerMode +
                " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
        }
    }

    [ContextMenu("ROAE/NPC/Reset AI Dev State")]
    public void ResetAIDevState()
    {
        NpcAIDevTools.ResetRuntimeState(40, 0, 0, new[] { npcId });
    }

    [ContextMenu("ROAE/NPC/Test All Planner States")]
    public void TestAllPlannerStates()
    {
        NpcAIDevTools.PrintPlannerStateMatrix(plannerConfig, true);
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
