using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NpcAIDevTools : MonoBehaviour
{
    private static readonly string[] SharedPresentationFlagKeys =
    {
        "anticariat_intro_done",
        "AnticarLeft",
        "barista_tarot_followup_done",
        "Lichenia_Intro",
        "tarot",
        NarrativeFlagKeys.TarotReadingCompleted,
        NarrativeFlagKeys.MadameSentToAnticar,
        NarrativeFlagKeys.AnticarSharedBaristaSecret,
        NarrativeFlagKeys.BaristaSecretClosureDone,
        "lichenia_def",
        "barista_intro"
    };

    [Header("Reset")]
    [SerializeField] private int resetCreativity = 40;
    [SerializeField] private int resetEmpathy = 0;
    [SerializeField] private int resetCorruption = 0;
    [SerializeField] private string[] npcIdsToReset = { "barista", "anticar", "madame_lichenia" };
    [SerializeField] private string[] extraPlayerPrefFlagsToReset;
    [SerializeField] private DialogueFlag[] dialogueFlagsToReset;

    [Header("Planner coverage")]
    [SerializeField] private NpcPlannerConfig plannerConfig;
    [SerializeField] private bool logEachPlannerState = true;

    [ContextMenu("ROAE/AI Dev/Reset runtime state and planner cache")]
    public void ResetRuntimeStateAndPlannerCache()
    {
        ResetRuntimeState(resetCreativity, resetEmpathy, resetCorruption, npcIdsToReset);
        ResetPlayerPrefFlags(extraPlayerPrefFlagsToReset);
        ResetDialogueFlags(dialogueFlagsToReset);
        PlayerPrefs.Save();
    }

    [ContextMenu("ROAE/AI Dev/Test all planner states")]
    public void TestAllPlannerStates()
    {
        PrintPlannerStateMatrix(plannerConfig, logEachPlannerState);
    }

    public static void ResetRuntimeState(
        int creativity,
        int empathy,
        int corruption,
        IEnumerable<string> npcIds)
    {
        CreativeCore core = CreativeCore.Instance ?? Object.FindFirstObjectByType<CreativeCore>();
        if (core != null)
            core.ForceSetStats(creativity, empathy, corruption);

        PlayerPrefs.SetInt("creativity", creativity);
        PlayerPrefs.SetInt("empathy", empathy);
        PlayerPrefs.SetInt("plantCorruption", corruption);
        ResetPlayerPrefFlags(SharedPresentationFlagKeys);

        BaristaWelcomeState.ResetAll();
        NarrativeProgressState.SetCurrentChapterId(string.Empty);
        NarrativeProgressState.SetCurrentMomentId(string.Empty);
        NarrativeProgressState.ClearSceneOverride();

        if (npcIds != null)
        {
            foreach (string npcId in npcIds)
            {
                if (!string.IsNullOrWhiteSpace(npcId))
                    NpcRelationshipState.ResetRelationship(npcId.Trim());
            }
        }

        NpcPolicySolver.ClearCache();
        NpcTonePlanningSolvers.ClearCache();
        PlayerPrefs.Save();

        Debug.Log(
            "[ROAE][AI][DevReset][SUCCESS] creativity=" + creativity +
            " empathy=" + empathy +
            " corruption=" + corruption +
            " npcPlannerCache=cleared" +
            " tonePlannerCache=cleared" +
            " baristaState=reset" +
            " narrativeProgress=reset" +
            " presentationFlags=reset");
    }

    public static void ResetPlayerPrefFlags(IEnumerable<string> flagKeys)
    {
        if (flagKeys == null)
            return;

        foreach (string flagKey in flagKeys)
        {
            if (string.IsNullOrWhiteSpace(flagKey))
                continue;

            PlayerPrefs.SetInt(flagKey.Trim(), 0);
        }
    }

    public static void ResetDialogueFlags(IEnumerable<DialogueFlag> flags)
    {
        if (flags == null)
            return;

        foreach (DialogueFlag flag in flags)
        {
            if (flag != null)
                flag.ResetFlag();
        }
    }

    public static void PrintPlannerStateMatrix(
        NpcPlannerConfig config,
        bool logEachState)
    {
        if (config == null)
        {
            Debug.LogWarning("[ROAE][AI][DevTest][FAIL] reason=missing_planner_config");
            return;
        }

        NpcPolicySolution solution = NpcPolicySolver.GetOrBuildPolicy(config, false, true);
        IReadOnlyList<NpcDecisionState> states = NpcStateSpaceGenerator.GetAllStates();
        Dictionary<NpcActionType, int> actionCounts = new Dictionary<NpcActionType, int>();

        int covered = 0;
        int missing = 0;

        for (int i = 0; i < states.Count; i++)
        {
            NpcDecisionState state = states[i];
            if (!solution.policy.TryGetValue(state, out NpcActionType action))
            {
                missing++;
                if (logEachState)
                    Debug.LogWarning("[ROAE][AI][DevTest][STATE_FAIL] config=" + config.name + " state=" + state);
                continue;
            }

            covered++;
            if (!actionCounts.ContainsKey(action))
                actionCounts[action] = 0;
            actionCounts[action]++;

            if (!logEachState)
                continue;

            string valueText = solution.values.TryGetValue(state, out float value)
                ? value.ToString("0.000")
                : "NULL";

            Debug.Log(
                "[ROAE][AI][DevTest][STATE] config=" + config.name +
                " state=" + state +
                " action=" + action +
                " value=" + valueText);
        }

        bool success = missing == 0;
        string level = success ? "[SUCCESS]" : "[FAIL]";
        string summary =
            "[ROAE][AI][DevTest]" + level +
            " config=" + config.name +
            " mode=" + solution.plannerMode +
            " expectedStates=" + states.Count +
            " coveredStates=" + covered +
            " missingStates=" + missing +
            " policyEntries=" + solution.policy.Count +
            " actionDistribution={" + FormatActionCounts(actionCounts) + "}";

        if (success)
            Debug.Log(summary);
        else
            Debug.LogWarning(summary);
    }

    private static string FormatActionCounts(Dictionary<NpcActionType, int> counts)
    {
        if (counts == null || counts.Count == 0)
            return string.Empty;

        StringBuilder builder = new StringBuilder();
        bool first = true;

        foreach (NpcActionType action in System.Enum.GetValues(typeof(NpcActionType)))
        {
            if (!counts.TryGetValue(action, out int count))
                continue;

            if (!first)
                builder.Append(" | ");

            builder.Append(action).Append("=").Append(count);
            first = false;
        }

        return builder.ToString();
    }
}
