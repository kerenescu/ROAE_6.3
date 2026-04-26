
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class ROAEBaristaPlannerUpgradeEditor : EditorWindow
{
    private const string ScriptsRoot = "Assets/ROAE_2/Scripts";
    private const string BrainPath = ScriptsRoot + "/NarrativeV2/BaristaWelcome/AI/BaristaWelcomeBrain.cs";
    private const string SolversPath = ScriptsRoot + "/NarrativeV2/BaristaWelcome/AI/BaristaIntroPlanningSolvers.cs";
    private const string PlannerModePath = ScriptsRoot + "/NarrativeV2/BaristaWelcome/Core/BaristaPlannerMode.cs";
    private const string ArchiveSelectorPath = ScriptsRoot + "/_Archive/NPC_AI/Barista/BaristaActionSelector.cs";
    private const string ArchiveRelationshipDebugPath = ScriptsRoot + "/_Archive/NPC_AI/Barista/BaristaRelationshipDebug.cs";
    private const string BackupRoot = ScriptsRoot + "/_GeneratedBackups";

    [MenuItem("Tools/ROAE/Barista/Open Planner Upgrade Window")]
    public static void OpenWindow()
    {
        var window = GetWindow<ROAEBaristaPlannerUpgradeEditor>("ROAE Planner Upgrade");
        window.minSize = new Vector2(560f, 360f);
        window.Show();
    }

    [MenuItem("Tools/ROAE/Barista/Apply Narrative VI + PI Upgrade")]
    public static void ApplyUpgradeMenu()
    {
        ApplyUpgrade();
    }

    [MenuItem("Tools/ROAE/Barista/Audit Archived Planner References")]
    public static void AuditArchiveMenu()
    {
        AuditArchiveReferences(false);
    }

    [MenuItem("Tools/ROAE/Barista/Remove Archived Planner References")]
    public static void RemoveArchiveMenu()
    {
        AuditArchiveReferences(true);
    }

    [MenuItem("Tools/ROAE/Barista/Restore Last Planner Backup")]
    public static void RestoreMenu()
    {
        RestoreLastBackup();
    }

    private Vector2 scroll;

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        GUILayout.Space(8f);
        EditorGUILayout.LabelField("ROAE Barista Planner Upgrade", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool rewrites the NarrativeV2 Barista planner to support both Value Iteration and Policy Iteration, " +
            "creates backups, and can scan/remove archived planner components from scenes and prefabs.",
            MessageType.Info);

        GUILayout.Space(6f);

        if (GUILayout.Button("1. Apply Narrative VI + PI Upgrade", GUILayout.Height(34f)))
        {
            ApplyUpgrade();
        }

        if (GUILayout.Button("2. Audit Archived Planner References", GUILayout.Height(28f)))
        {
            AuditArchiveReferences(false);
        }

        if (GUILayout.Button("3. Remove Archived Planner References", GUILayout.Height(28f)))
        {
            AuditArchiveReferences(true);
        }

        if (GUILayout.Button("4. Restore Last Planner Backup", GUILayout.Height(28f)))
        {
            RestoreLastBackup();
        }

        GUILayout.Space(10f);

        EditorGUILayout.LabelField("Files touched", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(
            PlannerModePath + Environment.NewLine +
            SolversPath + Environment.NewLine +
            BrainPath,
            GUILayout.MinHeight(72f));

        GUILayout.Space(8f);

        EditorGUILayout.LabelField("Archived scripts scanned", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(
            ArchiveSelectorPath + Environment.NewLine +
            ArchiveRelationshipDebugPath,
            GUILayout.MinHeight(52f));

        EditorGUILayout.EndScrollView();
    }

    private static void ApplyUpgrade()
    {
        try
        {
            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupDir = BackupRoot + "/BaristaPlannerUpgrade_" + stamp;
            EnsureDirectory(BackupRoot);
            EnsureDirectory(backupDir);

            Log("Upgrade", "Starting planner upgrade.");
            Log("Upgrade", "Backup folder: " + backupDir);

            BackupAndWrite(PlannerModePath, BuildPlannerModeFile(), backupDir);
            BackupAndWrite(SolversPath, BuildSolverFile(), backupDir);
            BackupAndWrite(BrainPath, BuildBrainFile(), backupDir);

            AssetDatabase.Refresh();
            Log("Upgrade", "AssetDatabase.Refresh complete.");
            Log("Upgrade", "Planner upgrade completed successfully.");
            Log("Upgrade", "Next recommended step: Tools/ROAE/Barista/Remove Archived Planner References");
        }
        catch (Exception ex)
        {
            Debug.LogError("[ROAE][Editor][Upgrade][ERROR] " + ex);
            throw;
        }
    }

    private static void RestoreLastBackup()
    {
        if (!AssetDatabase.IsValidFolder(BackupRoot))
        {
            Debug.LogWarning("[ROAE][Editor][Restore] No backup root found: " + BackupRoot);
            return;
        }

        var absoluteBackupRoot = ToAbsolutePath(BackupRoot);
        var directories = new DirectoryInfo(absoluteBackupRoot)
            .GetDirectories("BaristaPlannerUpgrade_*", SearchOption.TopDirectoryOnly)
            .OrderByDescending(d => d.Name)
            .ToArray();

        if (directories.Length == 0)
        {
            Debug.LogWarning("[ROAE][Editor][Restore] No planner backups found.");
            return;
        }

        var latest = directories[0];
        Log("Restore", "Restoring from: " + latest.FullName);

        RestoreFileIfPresent(latest.FullName, PlannerModePath);
        RestoreFileIfPresent(latest.FullName, SolversPath);
        RestoreFileIfPresent(latest.FullName, BrainPath);

        AssetDatabase.Refresh();
        Log("Restore", "Restore complete.");
    }

    private static void RestoreFileIfPresent(string backupDirAbsolute, string projectRelativePath)
    {
        var targetAbsolute = ToAbsolutePath(projectRelativePath);
        var backupFileName = SanitizeBackupFileName(projectRelativePath);
        var backupAbsolute = Path.Combine(backupDirAbsolute, backupFileName);

        if (!File.Exists(backupAbsolute))
        {
            Log("Restore", "Missing backup file, skipped: " + backupAbsolute);
            return;
        }

        EnsureDirectory(Path.GetDirectoryName(targetAbsolute));
        File.Copy(backupAbsolute, targetAbsolute, true);
        Log("Restore", "Restored: " + projectRelativePath);
    }

    private static void AuditArchiveReferences(bool remove)
    {
        var scriptPaths = new[]
        {
            ArchiveSelectorPath,
            ArchiveRelationshipDebugPath
        };

        var totalHits = 0;
        var totalRemoved = 0;

        foreach (var scriptPath in scriptPaths)
        {
            if (!File.Exists(ToAbsolutePath(scriptPath)))
            {
                Log("ArchiveAudit", "Script not found, skipped: " + scriptPath);
                continue;
            }

            Log("ArchiveAudit", (remove ? "Scanning + removing" : "Scanning") + " for script: " + scriptPath);

            totalHits += ProcessPrefabs(scriptPath, remove, ref totalRemoved);
            totalHits += ProcessScenes(scriptPath, remove, ref totalRemoved);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Log("ArchiveAudit", "Finished. totalHits=" + totalHits + " totalRemoved=" + totalRemoved + " remove=" + remove);
    }

    private static int ProcessPrefabs(string scriptPath, bool remove, ref int totalRemoved)
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        var hits = 0;

        foreach (var guid in prefabGuids)
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var matches = FindMatchingBehaviours(root, scriptPath);
                if (matches.Count == 0)
                {
                    continue;
                }

                hits += matches.Count;
                Log("ArchiveAudit", "Prefab hit count=" + matches.Count + " path=" + prefabPath);

                if (remove)
                {
                    foreach (var match in matches)
                    {
                        Log("ArchiveAudit", "Removing component from prefab: " + GetHierarchyPath(match.transform));
                        UnityEngine.Object.DestroyImmediate(match, true);
                        totalRemoved++;
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        return hits;
    }

    private static int ProcessScenes(string scriptPath, bool remove, ref int totalRemoved)
    {
        var sceneGuids = AssetDatabase.FindAssets("t:Scene");
        var hits = 0;

        foreach (var guid in sceneGuids)
        {
            var scenePath = AssetDatabase.GUIDToAssetPath(guid);
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var dirty = false;

            foreach (var root in scene.GetRootGameObjects())
            {
                var matches = FindMatchingBehaviours(root, scriptPath);
                if (matches.Count == 0)
                {
                    continue;
                }

                hits += matches.Count;
                Log("ArchiveAudit", "Scene hit count=" + matches.Count + " path=" + scenePath);

                if (remove)
                {
                    foreach (var match in matches)
                    {
                        Log("ArchiveAudit", "Removing component from scene: " + GetHierarchyPath(match.transform));
                        UnityEngine.Object.DestroyImmediate(match, true);
                        totalRemoved++;
                        dirty = true;
                    }
                }
            }

            if (remove && dirty)
            {
                EditorSceneManager.SaveScene(scene);
                Log("ArchiveAudit", "Saved cleaned scene: " + scenePath);
            }
        }

        return hits;
    }

    private static List<MonoBehaviour> FindMatchingBehaviours(GameObject root, string scriptPath)
    {
        var result = new List<MonoBehaviour>();
        var all = root.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var mb in all)
        {
            if (mb == null)
            {
                continue;
            }

            var monoScript = MonoScript.FromMonoBehaviour(mb);
            if (monoScript == null)
            {
                continue;
            }

            var currentPath = AssetDatabase.GetAssetPath(monoScript);
            if (string.Equals(currentPath, scriptPath, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(mb);
            }
        }

        return result;
    }

    private static void BackupAndWrite(string projectRelativePath, string newContent, string backupDirProjectRelative)
    {
        var absolutePath = ToAbsolutePath(projectRelativePath);
        EnsureDirectory(Path.GetDirectoryName(absolutePath));

        if (File.Exists(absolutePath))
        {
            var backupAbsolute = Path.Combine(ToAbsolutePath(backupDirProjectRelative), SanitizeBackupFileName(projectRelativePath));
            File.Copy(absolutePath, backupAbsolute, true);
            Log("Backup", "Backed up: " + projectRelativePath);
        }
        else
        {
            Log("Backup", "File did not exist, writing fresh: " + projectRelativePath);
        }

        File.WriteAllText(absolutePath, newContent);
        Log("Write", "Wrote: " + projectRelativePath);
    }

    private static void EnsureDirectory(string projectRelativeOrAbsolutePath)
    {
        if (string.IsNullOrEmpty(projectRelativeOrAbsolutePath))
        {
            return;
        }

        var absolutePath = projectRelativeOrAbsolutePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)
            ? ToAbsolutePath(projectRelativeOrAbsolutePath)
            : projectRelativeOrAbsolutePath;

        if (!Directory.Exists(absolutePath))
        {
            Directory.CreateDirectory(absolutePath);
        }
    }

    private static string ToAbsolutePath(string projectRelativePath)
    {
        var dataPath = Application.dataPath.Replace("\\", "/");
        var projectRoot = dataPath.Substring(0, dataPath.Length - "Assets".Length);
        return Path.Combine(projectRoot, projectRelativePath).Replace("\\", "/");
    }

    private static string SanitizeBackupFileName(string projectRelativePath)
    {
        return projectRelativePath
            .Replace("Assets/", string.Empty)
            .Replace("/", "__")
            .Replace("\\", "__")
            + ".bak";
    }

    private static string GetHierarchyPath(Transform current)
    {
        var parts = new List<string>();
        while (current != null)
        {
            parts.Add(current.name);
            current = current.parent;
        }

        parts.Reverse();
        return string.Join("/", parts);
    }

    private static void Log(string channel, string message)
    {
        Debug.Log("[ROAE][Editor][" + channel + "] " + message);
    }

    private static string BuildPlannerModeFile()
    {
        return NormalizeNewLines(@"
public enum BaristaPlannerMode
{
    ValueIteration = 0,
    PolicyIteration = 1
}
");
    }

    private static string BuildSolverFile()
    {
        return NormalizeNewLines(@"
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BaristaNarrativeAction
{
    ObserveNeutral = 0,
    InviteOrder = 1,
    GuardedCheck = 2,
    MischievousProbe = 3,
    OfferSafe = 4,
    OfferWeird = 5,
    RevealHint = 6,
    Deflect = 7
}

public enum BaristaNarrativePhase
{
    Intro = 0,
    Order = 1,
    PreparingDrink = 2,
    AlreadyHasDrink = 3
}

[Serializable]
public struct BaristaIntroPlanningRuntimeState
{
    public bool readUnknownText;
    public int corruption;
    public bool introDone;
    public bool acceptedFirstDrink;
    public string heldDrink;

    public BaristaNarrativePhase Phase
    {
        get
        {
            if (!string.IsNullOrEmpty(heldDrink) && !string.Equals(heldDrink, ""None"", StringComparison.OrdinalIgnoreCase))
            {
                return BaristaNarrativePhase.AlreadyHasDrink;
            }

            if (acceptedFirstDrink)
            {
                return BaristaNarrativePhase.PreparingDrink;
            }

            if (introDone)
            {
                return BaristaNarrativePhase.Order;
            }

            return BaristaNarrativePhase.Intro;
        }
    }

    public int CorruptionTier
    {
        get
        {
            if (corruption <= 1) return 0;
            if (corruption <= 3) return 1;
            return 2;
        }
    }

    public int HeldDrinkTier
    {
        get
        {
            if (string.IsNullOrEmpty(heldDrink) || string.Equals(heldDrink, ""None"", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (heldDrink.IndexOf(""Photo"", StringComparison.OrdinalIgnoreCase) >= 0 ||
                heldDrink.IndexOf(""Sap"", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 2;
            }

            return 1;
        }
    }

    public string ToDebugString()
    {
        return ""phase="" + Phase +
               "" readUnknownText="" + readUnknownText +
               "" corruption="" + corruption +
               "" corruptionTier="" + CorruptionTier +
               "" introDone="" + introDone +
               "" acceptedFirstDrink="" + acceptedFirstDrink +
               "" heldDrink="" + (string.IsNullOrEmpty(heldDrink) ? ""None"" : heldDrink) +
               "" heldDrinkTier="" + HeldDrinkTier;
    }
}

internal struct BaristaPlanningStateKey : IEquatable<BaristaPlanningStateKey>
{
    public int phase;
    public int corruptionTier;
    public int knowledgeTier;
    public int acceptanceTier;
    public int possessionTier;

    public bool Equals(BaristaPlanningStateKey other)
    {
        return phase == other.phase &&
               corruptionTier == other.corruptionTier &&
               knowledgeTier == other.knowledgeTier &&
               acceptanceTier == other.acceptanceTier &&
               possessionTier == other.possessionTier;
    }

    public override bool Equals(object obj)
    {
        return obj is BaristaPlanningStateKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = phase;
            hash = (hash * 397) ^ corruptionTier;
            hash = (hash * 397) ^ knowledgeTier;
            hash = (hash * 397) ^ acceptanceTier;
            hash = (hash * 397) ^ possessionTier;
            return hash;
        }
    }

    public string ToDebugString()
    {
        return ""phase="" + (BaristaNarrativePhase)phase +
               "" corruptionTier="" + corruptionTier +
               "" knowledgeTier="" + knowledgeTier +
               "" acceptanceTier="" + acceptanceTier +
               "" possessionTier="" + possessionTier;
    }
}

internal struct BaristaStateTransition
{
    public BaristaPlanningStateKey nextState;
    public float probability;

    public BaristaStateTransition(BaristaPlanningStateKey nextState, float probability)
    {
        this.nextState = nextState;
        this.probability = probability;
    }
}

internal struct BaristaPlannerDecision
{
    public BaristaNarrativeAction bestAction;
    public Dictionary<BaristaNarrativeAction, float> actionScores;
    public Dictionary<BaristaPlanningStateKey, BaristaNarrativeAction> policy;
    public Dictionary<BaristaPlanningStateKey, float> values;
}

public static class BaristaIntroPlanningSolvers
{
    private const float Gamma = 0.87f;
    private const float EvaluationEpsilon = 0.0001f;
    private const int MaxValueIterations = 96;
    private const int MaxPolicyIterations = 24;
    private const int MaxPolicyEvaluationSweeps = 96;

    private static readonly BaristaNarrativeAction[] Actions =
    {
        BaristaNarrativeAction.ObserveNeutral,
        BaristaNarrativeAction.InviteOrder,
        BaristaNarrativeAction.GuardedCheck,
        BaristaNarrativeAction.MischievousProbe,
        BaristaNarrativeAction.OfferSafe,
        BaristaNarrativeAction.OfferWeird,
        BaristaNarrativeAction.RevealHint,
        BaristaNarrativeAction.Deflect
    };

    public static BaristaNarrativeAction DecideAction(
        BaristaIntroPlanningRuntimeState runtimeState,
        BaristaPlannerMode plannerMode,
        bool verboseLogs)
    {
        var stateSpace = GenerateStateSpace();
        var currentState = ToStateKey(runtimeState);

        if (verboseLogs)
        {
            Debug.Log(
                ""[ROAE][Planner]["" + plannerMode + ""][START] states="" + stateSpace.Count +
                "" actions="" + Actions.Length +
                "" currentState={"" + currentState.ToDebugString() + ""}"");
        }

        var decision = plannerMode == BaristaPlannerMode.PolicyIteration
            ? RunPolicyIteration(stateSpace, currentState, verboseLogs)
            : RunValueIteration(stateSpace, currentState, verboseLogs);

        if (verboseLogs)
        {
            Debug.Log(
                ""[ROAE][Planner]["" + plannerMode + ""][RESULT] state={"" + currentState.ToDebugString() +
                ""} selectedAction="" + decision.bestAction +
                "" mappedTone="" + MapActionToTone(decision.bestAction) +
                "" scores{"" + FormatActionScores(decision.actionScores) + ""}"");
        }

        return decision.bestAction;
    }

    public static BaristaIntroTone MapActionToTone(BaristaNarrativeAction action)
    {
        switch (action)
        {
            case BaristaNarrativeAction.MischievousProbe:
            case BaristaNarrativeAction.OfferWeird:
            case BaristaNarrativeAction.RevealHint:
            case BaristaNarrativeAction.Deflect:
                return BaristaIntroTone.Mischievous;

            case BaristaNarrativeAction.GuardedCheck:
            case BaristaNarrativeAction.ObserveNeutral:
            case BaristaNarrativeAction.InviteOrder:
            case BaristaNarrativeAction.OfferSafe:
            default:
                return BaristaIntroTone.Neutral;
        }
    }

    private static List<BaristaPlanningStateKey> GenerateStateSpace()
    {
        var states = new List<BaristaPlanningStateKey>();

        for (var phase = 0; phase < 4; phase++)
        {
            for (var corruptionTier = 0; corruptionTier < 3; corruptionTier++)
            {
                for (var knowledgeTier = 0; knowledgeTier < 2; knowledgeTier++)
                {
                    for (var acceptanceTier = 0; acceptanceTier < 2; acceptanceTier++)
                    {
                        for (var possessionTier = 0; possessionTier < 3; possessionTier++)
                        {
                            states.Add(new BaristaPlanningStateKey
                            {
                                phase = phase,
                                corruptionTier = corruptionTier,
                                knowledgeTier = knowledgeTier,
                                acceptanceTier = acceptanceTier,
                                possessionTier = possessionTier
                            });
                        }
                    }
                }
            }
        }

        return states;
    }

    private static BaristaPlanningStateKey ToStateKey(BaristaIntroPlanningRuntimeState runtimeState)
    {
        return new BaristaPlanningStateKey
        {
            phase = (int)runtimeState.Phase,
            corruptionTier = runtimeState.CorruptionTier,
            knowledgeTier = runtimeState.readUnknownText ? 1 : 0,
            acceptanceTier = runtimeState.acceptedFirstDrink ? 1 : 0,
            possessionTier = runtimeState.HeldDrinkTier
        };
    }

    private static BaristaPlannerDecision RunValueIteration(
        List<BaristaPlanningStateKey> states,
        BaristaPlanningStateKey currentState,
        bool verboseLogs)
    {
        var values = states.ToDictionary(state => state, state => 0f);

        for (var iteration = 0; iteration < MaxValueIterations; iteration++)
        {
            var nextValues = new Dictionary<BaristaPlanningStateKey, float>(values.Count);
            var delta = 0f;

            foreach (var state in states)
            {
                var best = float.NegativeInfinity;

                foreach (var action in Actions)
                {
                    var q = ComputeActionValue(state, action, values);
                    if (q > best)
                    {
                        best = q;
                    }
                }

                nextValues[state] = best;
                delta = Mathf.Max(delta, Mathf.Abs(best - values[state]));
            }

            values = nextValues;

            if (verboseLogs)
            {
                Debug.Log(""[ROAE][Planner][VI] iteration="" + iteration + "" delta="" + delta);
            }

            if (delta < EvaluationEpsilon)
            {
                if (verboseLogs)
                {
                    Debug.Log(""[ROAE][Planner][VI] converged iteration="" + iteration + "" delta="" + delta);
                }
                break;
            }
        }

        var policy = new Dictionary<BaristaPlanningStateKey, BaristaNarrativeAction>(states.Count);
        foreach (var state in states)
        {
            policy[state] = SelectBestAction(state, values).bestAction;
        }

        return new BaristaPlannerDecision
        {
            bestAction = policy[currentState],
            actionScores = ComputeActionScores(currentState, values),
            policy = policy,
            values = values
        };
    }

    private static BaristaPlannerDecision RunPolicyIteration(
        List<BaristaPlanningStateKey> states,
        BaristaPlanningStateKey currentState,
        bool verboseLogs)
    {
        var values = states.ToDictionary(state => state, state => 0f);
        var policy = new Dictionary<BaristaPlanningStateKey, BaristaNarrativeAction>(states.Count);

        foreach (var state in states)
        {
            policy[state] = DefaultActionForState(state);
        }

        for (var iteration = 0; iteration < MaxPolicyIterations; iteration++)
        {
            for (var sweep = 0; sweep < MaxPolicyEvaluationSweeps; sweep++)
            {
                var delta = 0f;

                foreach (var state in states)
                {
                    var action = policy[state];
                    var newValue = ComputeActionValue(state, action, values);
                    delta = Mathf.Max(delta, Mathf.Abs(newValue - values[state]));
                    values[state] = newValue;
                }

                if (verboseLogs)
                {
                    Debug.Log(""[ROAE][Planner][PI][Eval] policyIteration="" + iteration + "" sweep="" + sweep + "" delta="" + delta);
                }

                if (delta < EvaluationEpsilon)
                {
                    break;
                }
            }

            var stable = true;
            var changed = 0;

            foreach (var state in states)
            {
                var oldAction = policy[state];
                var best = SelectBestAction(state, values).bestAction;
                if (best != oldAction)
                {
                    stable = false;
                    changed++;
                    policy[state] = best;
                }
            }

            if (verboseLogs)
            {
                Debug.Log(""[ROAE][Planner][PI][Improve] iteration="" + iteration + "" changedActions="" + changed + "" stable="" + stable);
            }

            if (stable)
            {
                break;
            }
        }

        return new BaristaPlannerDecision
        {
            bestAction = policy[currentState],
            actionScores = ComputeActionScores(currentState, values),
            policy = policy,
            values = values
        };
    }

    private static (BaristaNarrativeAction bestAction, float bestScore) SelectBestAction(
        BaristaPlanningStateKey state,
        Dictionary<BaristaPlanningStateKey, float> values)
    {
        var bestAction = Actions[0];
        var bestScore = float.NegativeInfinity;

        foreach (var action in Actions)
        {
            var q = ComputeActionValue(state, action, values);
            if (q > bestScore)
            {
                bestScore = q;
                bestAction = action;
            }
        }

        return (bestAction, bestScore);
    }

    private static Dictionary<BaristaNarrativeAction, float> ComputeActionScores(
        BaristaPlanningStateKey state,
        Dictionary<BaristaPlanningStateKey, float> values)
    {
        var result = new Dictionary<BaristaNarrativeAction, float>();
        foreach (var action in Actions)
        {
            result[action] = ComputeActionValue(state, action, values);
        }

        return result;
    }

    private static float ComputeActionValue(
        BaristaPlanningStateKey state,
        BaristaNarrativeAction action,
        Dictionary<BaristaPlanningStateKey, float> values)
    {
        var immediateReward = EvaluateReward(state, action);
        var transitions = EvaluateTransitions(state, action);

        var future = 0f;
        for (var i = 0; i < transitions.Count; i++)
        {
            var transition = transitions[i];
            future += transition.probability * values[transition.nextState];
        }

        return immediateReward + Gamma * future;
    }

    private static float EvaluateReward(BaristaPlanningStateKey state, BaristaNarrativeAction action)
    {
        var phase = (BaristaNarrativePhase)state.phase;
        var reward = 0f;

        switch (phase)
        {
            case BaristaNarrativePhase.Intro:
                reward += RewardIntro(action);
                break;

            case BaristaNarrativePhase.Order:
                reward += RewardOrder(action);
                break;

            case BaristaNarrativePhase.PreparingDrink:
                reward += RewardPreparing(action);
                break;

            case BaristaNarrativePhase.AlreadyHasDrink:
                reward += RewardAlreadyHasDrink(action);
                break;
        }

        if (state.knowledgeTier == 1)
        {
            if (action == BaristaNarrativeAction.RevealHint) reward += 1.40f;
            if (action == BaristaNarrativeAction.MischievousProbe) reward += 0.65f;
            if (action == BaristaNarrativeAction.ObserveNeutral) reward -= 0.25f;
        }

        if (state.corruptionTier == 0)
        {
            if (action == BaristaNarrativeAction.OfferSafe) reward += 0.80f;
            if (action == BaristaNarrativeAction.OfferWeird) reward -= 1.10f;
            if (action == BaristaNarrativeAction.MischievousProbe) reward -= 0.40f;
        }
        else if (state.corruptionTier == 1)
        {
            if (action == BaristaNarrativeAction.MischievousProbe) reward += 0.45f;
            if (action == BaristaNarrativeAction.Deflect) reward += 0.35f;
            if (action == BaristaNarrativeAction.OfferWeird) reward += 0.75f;
        }
        else
        {
            if (action == BaristaNarrativeAction.MischievousProbe) reward += 1.10f;
            if (action == BaristaNarrativeAction.Deflect) reward += 0.80f;
            if (action == BaristaNarrativeAction.OfferWeird) reward += 1.30f;
            if (action == BaristaNarrativeAction.OfferSafe) reward -= 0.50f;
        }

        if (state.acceptanceTier == 1)
        {
            if (action == BaristaNarrativeAction.RevealHint) reward += 0.75f;
            if (action == BaristaNarrativeAction.Deflect) reward += 0.30f;
        }

        if (state.possessionTier == 2)
        {
            if (action == BaristaNarrativeAction.Deflect) reward += 1.00f;
            if (action == BaristaNarrativeAction.RevealHint) reward += 0.50f;
        }

        if (state.possessionTier == 1)
        {
            if (action == BaristaNarrativeAction.InviteOrder) reward -= 1.20f;
            if (action == BaristaNarrativeAction.OfferSafe) reward -= 1.40f;
            if (action == BaristaNarrativeAction.OfferWeird) reward -= 1.60f;
        }

        return reward;
    }

    private static float RewardIntro(BaristaNarrativeAction action)
    {
        switch (action)
        {
            case BaristaNarrativeAction.ObserveNeutral: return 2.40f;
            case BaristaNarrativeAction.InviteOrder: return 2.00f;
            case BaristaNarrativeAction.GuardedCheck: return 1.25f;
            case BaristaNarrativeAction.MischievousProbe: return 1.65f;
            case BaristaNarrativeAction.OfferSafe: return 0.10f;
            case BaristaNarrativeAction.OfferWeird: return -0.90f;
            case BaristaNarrativeAction.RevealHint: return 1.40f;
            case BaristaNarrativeAction.Deflect: return 0.80f;
            default: return 0f;
        }
    }

    private static float RewardOrder(BaristaNarrativeAction action)
    {
        switch (action)
        {
            case BaristaNarrativeAction.ObserveNeutral: return 0.60f;
            case BaristaNarrativeAction.InviteOrder: return 1.10f;
            case BaristaNarrativeAction.GuardedCheck: return 1.20f;
            case BaristaNarrativeAction.MischievousProbe: return 1.50f;
            case BaristaNarrativeAction.OfferSafe: return 2.60f;
            case BaristaNarrativeAction.OfferWeird: return 2.80f;
            case BaristaNarrativeAction.RevealHint: return 1.85f;
            case BaristaNarrativeAction.Deflect: return 1.35f;
            default: return 0f;
        }
    }

    private static float RewardPreparing(BaristaNarrativeAction action)
    {
        switch (action)
        {
            case BaristaNarrativeAction.ObserveNeutral: return 0.30f;
            case BaristaNarrativeAction.InviteOrder: return -0.80f;
            case BaristaNarrativeAction.GuardedCheck: return 0.85f;
            case BaristaNarrativeAction.MischievousProbe: return 1.00f;
            case BaristaNarrativeAction.OfferSafe: return -1.40f;
            case BaristaNarrativeAction.OfferWeird: return -1.55f;
            case BaristaNarrativeAction.RevealHint: return 2.00f;
            case BaristaNarrativeAction.Deflect: return 1.80f;
            default: return 0f;
        }
    }

    private static float RewardAlreadyHasDrink(BaristaNarrativeAction action)
    {
        switch (action)
        {
            case BaristaNarrativeAction.ObserveNeutral: return 0.20f;
            case BaristaNarrativeAction.InviteOrder: return -2.25f;
            case BaristaNarrativeAction.GuardedCheck: return 1.80f;
            case BaristaNarrativeAction.MischievousProbe: return 1.40f;
            case BaristaNarrativeAction.OfferSafe: return -2.50f;
            case BaristaNarrativeAction.OfferWeird: return -2.80f;
            case BaristaNarrativeAction.RevealHint: return 1.75f;
            case BaristaNarrativeAction.Deflect: return 2.35f;
            default: return 0f;
        }
    }

    private static List<BaristaStateTransition> EvaluateTransitions(
        BaristaPlanningStateKey state,
        BaristaNarrativeAction action)
    {
        var phase = (BaristaNarrativePhase)state.phase;
        var transitions = new List<BaristaStateTransition>();

        switch (phase)
        {
            case BaristaNarrativePhase.Intro:
                BuildIntroTransitions(state, action, transitions);
                break;

            case BaristaNarrativePhase.Order:
                BuildOrderTransitions(state, action, transitions);
                break;

            case BaristaNarrativePhase.PreparingDrink:
                BuildPreparingTransitions(state, action, transitions);
                break;

            case BaristaNarrativePhase.AlreadyHasDrink:
                BuildAlreadyHasDrinkTransitions(state, action, transitions);
                break;
        }

        NormalizeTransitions(transitions);
        return transitions;
    }

    private static void BuildIntroTransitions(
        BaristaPlanningStateKey state,
        BaristaNarrativeAction action,
        List<BaristaStateTransition> output)
    {
        switch (action)
        {
            case BaristaNarrativeAction.ObserveNeutral:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Intro), 0.55f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order), 0.45f);
                break;

            case BaristaNarrativeAction.InviteOrder:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order), 0.80f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Intro), 0.20f);
                break;

            case BaristaNarrativeAction.GuardedCheck:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Intro), 0.45f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order), 0.45f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Intro, corruptionDelta: +1), 0.10f);
                break;

            case BaristaNarrativeAction.MischievousProbe:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order), 0.50f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order, corruptionDelta: +1), 0.35f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Intro, knowledgeTier: 1), 0.15f);
                break;

            case BaristaNarrativeAction.OfferSafe:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.PreparingDrink, acceptanceTier: 1, possessionTier: 1), 0.70f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order), 0.30f);
                break;

            case BaristaNarrativeAction.OfferWeird:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.PreparingDrink, acceptanceTier: 1, possessionTier: 2, corruptionDelta: +1), 0.60f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order, corruptionDelta: +1), 0.25f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Intro), 0.15f);
                break;

            case BaristaNarrativeAction.RevealHint:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1), 0.65f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Intro, knowledgeTier: 1), 0.35f);
                break;

            case BaristaNarrativeAction.Deflect:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Intro), 0.50f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order), 0.35f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Intro, corruptionDelta: +1), 0.15f);
                break;
        }
    }

    private static void BuildOrderTransitions(
        BaristaPlanningStateKey state,
        BaristaNarrativeAction action,
        List<BaristaStateTransition> output)
    {
        switch (action)
        {
            case BaristaNarrativeAction.ObserveNeutral:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order), 0.60f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.PreparingDrink, acceptanceTier: 1, possessionTier: 1), 0.40f);
                break;

            case BaristaNarrativeAction.InviteOrder:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order), 0.35f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.PreparingDrink, acceptanceTier: 1, possessionTier: 1), 0.65f);
                break;

            case BaristaNarrativeAction.GuardedCheck:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order), 0.65f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order, corruptionDelta: +1), 0.20f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.PreparingDrink, acceptanceTier: 1, possessionTier: 1), 0.15f);
                break;

            case BaristaNarrativeAction.MischievousProbe:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order, corruptionDelta: +1), 0.45f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.PreparingDrink, acceptanceTier: 1, possessionTier: 2, corruptionDelta: +1), 0.40f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1), 0.15f);
                break;

            case BaristaNarrativeAction.OfferSafe:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.PreparingDrink, acceptanceTier: 1, possessionTier: 1), 0.85f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order), 0.15f);
                break;

            case BaristaNarrativeAction.OfferWeird:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.PreparingDrink, acceptanceTier: 1, possessionTier: 2, corruptionDelta: +1), 0.75f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order, corruptionDelta: +1), 0.25f);
                break;

            case BaristaNarrativeAction.RevealHint:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order, knowledgeTier: 1), 0.55f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.PreparingDrink, knowledgeTier: 1, acceptanceTier: 1, possessionTier: Mathf.Max(state.possessionTier, 1)), 0.45f);
                break;

            case BaristaNarrativeAction.Deflect:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.Order), 0.60f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, possessionTier: Mathf.Max(state.possessionTier, 1)), 0.40f);
                break;
        }
    }

    private static void BuildPreparingTransitions(
        BaristaPlanningStateKey state,
        BaristaNarrativeAction action,
        List<BaristaStateTransition> output)
    {
        switch (action)
        {
            case BaristaNarrativeAction.RevealHint:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, knowledgeTier: 1, possessionTier: Mathf.Max(state.possessionTier, 1)), 0.75f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.PreparingDrink, knowledgeTier: 1), 0.25f);
                break;

            case BaristaNarrativeAction.Deflect:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, possessionTier: Mathf.Max(state.possessionTier, 1)), 0.80f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.PreparingDrink), 0.20f);
                break;

            case BaristaNarrativeAction.MischievousProbe:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, possessionTier: 2, corruptionDelta: +1), 0.70f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.PreparingDrink, corruptionDelta: +1), 0.30f);
                break;

            default:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, possessionTier: Mathf.Max(state.possessionTier, 1)), 0.65f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.PreparingDrink), 0.35f);
                break;
        }
    }

    private static void BuildAlreadyHasDrinkTransitions(
        BaristaPlanningStateKey state,
        BaristaNarrativeAction action,
        List<BaristaStateTransition> output)
    {
        switch (action)
        {
            case BaristaNarrativeAction.RevealHint:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, knowledgeTier: 1), 0.85f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, knowledgeTier: 1, corruptionDelta: +1), 0.15f);
                break;

            case BaristaNarrativeAction.Deflect:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink), 0.90f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, corruptionDelta: +1), 0.10f);
                break;

            case BaristaNarrativeAction.MischievousProbe:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, corruptionDelta: +1), 0.60f);
                Add(output, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink, knowledgeTier: 1), 0.40f);
                break;

            default:
                Add(output, Mutate(state, phase: BaristaNarrativePhase.AlreadyHasDrink), 1.0f);
                break;
        }
    }

    private static BaristaPlanningStateKey Mutate(
        BaristaPlanningStateKey state,
        BaristaNarrativePhase? phase = null,
        int? knowledgeTier = null,
        int? acceptanceTier = null,
        int? possessionTier = null,
        int corruptionDelta = 0)
    {
        return new BaristaPlanningStateKey
        {
            phase = phase.HasValue ? (int)phase.Value : state.phase,
            corruptionTier = Mathf.Clamp(state.corruptionTier + corruptionDelta, 0, 2),
            knowledgeTier = knowledgeTier.HasValue ? Mathf.Clamp(knowledgeTier.Value, 0, 1) : state.knowledgeTier,
            acceptanceTier = acceptanceTier.HasValue ? Mathf.Clamp(acceptanceTier.Value, 0, 1) : state.acceptanceTier,
            possessionTier = possessionTier.HasValue ? Mathf.Clamp(possessionTier.Value, 0, 2) : state.possessionTier
        };
    }

    private static void Add(List<BaristaStateTransition> output, BaristaPlanningStateKey nextState, float probability)
    {
        output.Add(new BaristaStateTransition(nextState, probability));
    }

    private static void NormalizeTransitions(List<BaristaStateTransition> transitions)
    {
        var grouped = new Dictionary<BaristaPlanningStateKey, float>();

        for (var i = 0; i < transitions.Count; i++)
        {
            var transition = transitions[i];
            if (!grouped.ContainsKey(transition.nextState))
            {
                grouped[transition.nextState] = 0f;
            }

            grouped[transition.nextState] += transition.probability;
        }

        transitions.Clear();

        var sum = grouped.Values.Sum();
        if (sum <= 0f)
        {
            return;
        }

        foreach (var pair in grouped)
        {
            transitions.Add(new BaristaStateTransition(pair.Key, pair.Value / sum));
        }
    }

    private static BaristaNarrativeAction DefaultActionForState(BaristaPlanningStateKey state)
    {
        var phase = (BaristaNarrativePhase)state.phase;

        switch (phase)
        {
            case BaristaNarrativePhase.Intro:
                return state.corruptionTier >= 1 ? BaristaNarrativeAction.MischievousProbe : BaristaNarrativeAction.ObserveNeutral;

            case BaristaNarrativePhase.Order:
                return state.corruptionTier >= 1 ? BaristaNarrativeAction.OfferWeird : BaristaNarrativeAction.OfferSafe;

            case BaristaNarrativePhase.PreparingDrink:
                return BaristaNarrativeAction.RevealHint;

            case BaristaNarrativePhase.AlreadyHasDrink:
            default:
                return BaristaNarrativeAction.Deflect;
        }
    }

    private static string FormatActionScores(Dictionary<BaristaNarrativeAction, float> scores)
    {
        return string.Join(
            "" | "",
            scores.OrderByDescending(pair => pair.Value).Select(pair => pair.Key + ""="" + pair.Value.ToString(""0.000"")));
    }
}
");
    }

    private static string BuildBrainFile()
    {
        return NormalizeNewLines(@"
using System;
using System.Reflection;
using UnityEngine;

public sealed class BaristaWelcomeBrain : MonoBehaviour
{
    [SerializeField] private BaristaPlannerMode plannerMode = BaristaPlannerMode.ValueIteration;
    [SerializeField] private bool verbosePlannerLogs = true;
    [SerializeField] private bool verboseStateExtraction = false;
    [SerializeField] private MonoBehaviour explicitStateSource;

    public BaristaIntroTone DecideOpeningTone()
    {
        var runtimeState = ExtractRuntimeState();
        var action = BaristaIntroPlanningSolvers.DecideAction(runtimeState, plannerMode, verbosePlannerLogs);
        var tone = BaristaIntroPlanningSolvers.MapActionToTone(action);

        Debug.Log(
            ""[ROAE][BaristaWelcomeBrain] planner="" + plannerMode +
            "" extractedState={"" + runtimeState.ToDebugString() + ""}"" +
            "" selectedAction="" + action +
            "" resultTone="" + tone);

        return tone;
    }

    public string DebugDecideActionLabel()
    {
        var runtimeState = ExtractRuntimeState();
        var action = BaristaIntroPlanningSolvers.DecideAction(runtimeState, plannerMode, verbosePlannerLogs);
        return action.ToString();
    }

    private BaristaIntroPlanningRuntimeState ExtractRuntimeState()
    {
        var stateSource = ResolvePrimaryStateSource();

        var readUnknownText = ReadBool(
            stateSource,
            new[] { ""readUnknownText"", ""_readUnknownText"", ""ReadUnknownText"" },
            PlayerPrefs.GetInt(""read_unknown_text_01"", 0) == 1);

        var introDone = ReadBool(
            stateSource,
            new[] { ""introDone"", ""_introDone"", ""IntroDone"" },
            false);

        var acceptedFirstDrink = ReadBool(
            stateSource,
            new[] { ""accepted"", ""acceptedFirstDrink"", ""_accepted"", ""Accepted"" },
            false);

        var heldDrink = ReadString(
            stateSource,
            new[] { ""heldDrink"", ""_heldDrink"", ""HeldDrink"" },
            ""None"");

        var corruption = ExtractCorruption();

        var runtimeState = new BaristaIntroPlanningRuntimeState
        {
            readUnknownText = readUnknownText,
            corruption = corruption,
            introDone = introDone,
            acceptedFirstDrink = acceptedFirstDrink,
            heldDrink = string.IsNullOrEmpty(heldDrink) ? ""None"" : heldDrink
        };

        if (verboseStateExtraction)
        {
            Debug.Log(""[ROAE][BaristaWelcomeBrain][StateExtraction] source="" + DescribeObject(stateSource) + "" runtimeState={"" + runtimeState.ToDebugString() + ""}"");
        }

        return runtimeState;
    }

    private object ResolvePrimaryStateSource()
    {
        if (explicitStateSource != null)
        {
            return explicitStateSource;
        }

        var controllerType = Type.GetType(""BaristaWelcomeController"");
        if (controllerType != null)
        {
            var component = GetComponent(controllerType);
            if (component != null)
            {
                return component;
            }
        }

        return this;
    }

    private int ExtractCorruption()
    {
        var creativeCoreType = Type.GetType(""CreativeCore"");
        if (creativeCoreType == null)
        {
            return PlayerPrefs.GetInt(""plantCorruption"", 0);
        }

        var instance = UnityEngine.Object.FindObjectOfType(creativeCoreType);
        if (instance == null)
        {
            return PlayerPrefs.GetInt(""plantCorruption"", 0);
        }

        var boxedValue = ReadMember(instance, new[] { ""plantCorruption"", ""_plantCorruption"", ""corruption"", ""_corruption"" });
        if (boxedValue == null)
        {
            return PlayerPrefs.GetInt(""plantCorruption"", 0);
        }

        try
        {
            return Convert.ToInt32(boxedValue);
        }
        catch
        {
            return PlayerPrefs.GetInt(""plantCorruption"", 0);
        }
    }

    private static bool ReadBool(object source, string[] memberNames, bool fallback)
    {
        var boxedValue = ReadMember(source, memberNames);
        if (boxedValue == null)
        {
            return fallback;
        }

        if (boxedValue is bool boolValue)
        {
            return boolValue;
        }

        try
        {
            return Convert.ToInt32(boxedValue) != 0;
        }
        catch
        {
            return fallback;
        }
    }

    private static string ReadString(object source, string[] memberNames, string fallback)
    {
        var boxedValue = ReadMember(source, memberNames);
        if (boxedValue == null)
        {
            return fallback;
        }

        if (boxedValue is Enum enumValue)
        {
            return enumValue.ToString();
        }

        return boxedValue.ToString();
    }

    private static object ReadMember(object source, string[] memberNames)
    {
        if (source == null)
        {
            return null;
        }

        var type = source.GetType();

        for (var i = 0; i < memberNames.Length; i++)
        {
            var memberName = memberNames[i];

            var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                return field.GetValue(source);
            }

            var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.CanRead)
            {
                return property.GetValue(source, null);
            }

            var method = type.GetMethod(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (method != null)
            {
                return method.Invoke(source, null);
            }
        }

        return null;
    }

    private static string DescribeObject(object source)
    {
        if (source == null)
        {
            return ""<null>"";
        }

        var unityObject = source as UnityEngine.Object;
        if (unityObject != null)
        {
            return unityObject.name + "" ("" + source.GetType().Name + "")"";
        }

        return source.GetType().Name;
    }
}
");
    }

    private static string NormalizeNewLines(string input)
    {
        return input.Trim() + Environment.NewLine;
    }
}
#endif
