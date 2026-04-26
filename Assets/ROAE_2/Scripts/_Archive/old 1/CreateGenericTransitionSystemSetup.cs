using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateGenericTransitionSystemStep
{
    private const string CoreFolder = "Assets/ROAE_2/Scripts/NPC_AI/CoreStates";
    private const string BaristaFolder = "Assets/ROAE_2/Scripts/NPC_AI/Barista";
    private const string ResourcesFolder = "Assets/Resources/NPC_AI/Barista";
    private const string TransitionAssetPath = "Assets/Resources/NPC_AI/Barista/BaristaTransitionProfile.asset";

    [MenuItem("Tools/ROAE/NPC AI/Step 3 - Create Generic Transition Code")]
    public static void CreateCode()
    {
        EnsureFolder(CoreFolder);
        EnsureFolder(BaristaFolder);
        EnsureFolder(ResourcesFolder);

        WriteScript(
            ResolveScriptPath("NpcActionTransitionEntry.cs", CoreFolder),
            @"using System;

[Serializable]
public class NpcActionTransitionEntry
{
    public NpcActionType action;
    public float worsenProbability;
    public float keepProbability = 1f;
    public float improveProbability;

    public void Normalize()
    {
        float total = worsenProbability + keepProbability + improveProbability;
        if (total <= 0f)
        {
            keepProbability = 1f;
            worsenProbability = 0f;
            improveProbability = 0f;
            return;
        }

        worsenProbability /= total;
        keepProbability /= total;
        improveProbability /= total;
    }
}
");

        WriteScript(
            ResolveScriptPath("NpcTransitionProfile.cs", CoreFolder),
            @"using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = ""NewNpcTransitionProfile"", menuName = ""Dialogue System/NPC Transition Profile"")]
public class NpcTransitionProfile : ScriptableObject
{
    [SerializeField] private List<NpcActionTransitionEntry> transitions = new List<NpcActionTransitionEntry>();

    public IReadOnlyList<NpcActionTransitionEntry> Transitions => transitions;
}
");

        WriteScript(
            ResolveScriptPath("NpcTransitionEvaluator.cs", CoreFolder),
            @"using System.Collections.Generic;

public static class NpcTransitionEvaluator
{
    public static List<StateTransition> GetTransitions(NpcTransitionProfile profile, NpcDecisionState state, NpcActionType action)
    {
        List<StateTransition> result = new List<StateTransition>();

        if (profile == null)
        {
            result.Add(new StateTransition(state, 1f));
            return result;
        }

        NpcActionTransitionEntry entry = FindEntry(profile, action);
        if (entry == null)
        {
            result.Add(new StateTransition(state, 1f));
            return result;
        }

        float worsen = entry.worsenProbability;
        float keep = entry.keepProbability;
        float improve = entry.improveProbability;

        float total = worsen + keep + improve;
        if (total <= 0f)
        {
            result.Add(new StateTransition(state, 1f));
            return result;
        }

        worsen /= total;
        keep /= total;
        improve /= total;

        if (worsen > 0f)
            result.Add(new StateTransition(SetRelationship(state, WorsenRelationship(state.relationship)), worsen));

        if (keep > 0f)
            result.Add(new StateTransition(state, keep));

        if (improve > 0f)
            result.Add(new StateTransition(SetRelationship(state, ImproveRelationship(state.relationship)), improve));

        return result;
    }

    private static NpcActionTransitionEntry FindEntry(NpcTransitionProfile profile, NpcActionType action)
    {
        var entries = profile.Transitions;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].action == action)
                return (NpcActionTransitionEntry)entries[i];
        }

        return null;
    }

    private static NpcDecisionState SetRelationship(NpcDecisionState state, RelationshipBucket relationship)
    {
        state.relationship = relationship;
        return state;
    }

    private static RelationshipBucket ImproveRelationship(RelationshipBucket current)
    {
        switch (current)
        {
            case RelationshipBucket.Bad:
                return RelationshipBucket.Neutral;
            case RelationshipBucket.Neutral:
                return RelationshipBucket.Good;
            default:
                return RelationshipBucket.Good;
        }
    }

    private static RelationshipBucket WorsenRelationship(RelationshipBucket current)
    {
        switch (current)
        {
            case RelationshipBucket.Good:
                return RelationshipBucket.Neutral;
            case RelationshipBucket.Neutral:
                return RelationshipBucket.Bad;
            default:
                return RelationshipBucket.Bad;
        }
    }
}
");

        WriteScript(
            ResolveScriptPath("BaristaPolicy.cs", BaristaFolder),
            @"using System.Collections.Generic;
using UnityEngine;

public static class BaristaPolicy
{
    private const string RewardProfilePath = ""NPC_AI/Barista/BaristaRewardProfile"";
    private const string TransitionProfilePath = ""NPC_AI/Barista/BaristaTransitionProfile"";

    private static Dictionary<NpcDecisionState, NpcActionType> cachedPolicy;
    private static NpcRewardProfile cachedRewardProfile;
    private static NpcTransitionProfile cachedTransitionProfile;

    public static NpcActionType GetBestAction(NpcDecisionState state)
    {
        if (cachedPolicy == null || cachedPolicy.Count == 0)
            BuildPolicy();

        if (cachedPolicy.TryGetValue(state, out NpcActionType action))
            return action;

        return NpcActionType.Neutral;
    }

    public static void RebuildPolicy()
    {
        cachedRewardProfile = null;
        cachedTransitionProfile = null;
        BuildPolicy();
    }

    private static void BuildPolicy()
    {
        if (cachedRewardProfile == null)
            cachedRewardProfile = Resources.Load<NpcRewardProfile>(RewardProfilePath);

        if (cachedTransitionProfile == null)
            cachedTransitionProfile = Resources.Load<NpcTransitionProfile>(TransitionProfilePath);

        List<NpcDecisionState> states = NpcStateSpaceGenerator.GenerateAllStates();
        NpcActionType[] actions = BaristaRewardModel.GetAvailableActions();

        if (cachedRewardProfile == null)
        {
            cachedPolicy = new Dictionary<NpcDecisionState, NpcActionType>();
            for (int i = 0; i < states.Count; i++)
                cachedPolicy[states[i]] = NpcActionType.Neutral;

            return;
        }

        cachedPolicy = ValueIterationSolver.Solve(
            states,
            actions,
            (s, a) => NpcRewardEvaluator.GetReward(cachedRewardProfile, s, a),
            (s, a) => NpcTransitionEvaluator.GetTransitions(cachedTransitionProfile, s, a),
            0.85f,
            0.0001f,
            100
        );
    }
}
");

        AssetDatabase.Refresh();
        Debug.Log("Step 3 done. Wait for compile, then run Step 4.");
    }

    [MenuItem("Tools/ROAE/NPC AI/Step 4 - Create Barista Transition Asset")]
    public static void CreateTransitionAsset()
    {
        EnsureFolder(ResourcesFolder);

        Object assetObject = AssetDatabase.LoadAssetAtPath<Object>(TransitionAssetPath);
        if (assetObject == null)
        {
            ScriptableObject newAsset = ScriptableObject.CreateInstance("NpcTransitionProfile");
            if (newAsset == null)
            {
                Debug.LogError("NpcTransitionProfile type not available yet. Run Step 3 and wait for compile.");
                return;
            }

            AssetDatabase.CreateAsset(newAsset, TransitionAssetPath);
            assetObject = newAsset;
        }

        SerializedObject so = new SerializedObject(assetObject);
        SerializedProperty transitionsProp = so.FindProperty("transitions");

        if (transitionsProp == null)
        {
            Debug.LogError("Transition asset fields not found. Make sure Step 3 compiled successfully.");
            return;
        }

        transitionsProp.ClearArray();

        AddTransition(transitionsProp, NpcActionType.Neutral, 0f, 1f, 0f);
        AddTransition(transitionsProp, NpcActionType.Warm, 0f, 0.30f, 0.70f);
        AddTransition(transitionsProp, NpcActionType.Guarded, 0.70f, 0.30f, 0f);
        AddTransition(transitionsProp, NpcActionType.Hint, 0f, 1f, 0f);
        AddTransition(transitionsProp, NpcActionType.WarmHint, 0f, 0.20f, 0.80f);
        AddTransition(transitionsProp, NpcActionType.GuardedHint, 0.60f, 0.40f, 0f);

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(assetObject);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        BaristaPolicy.RebuildPolicy();

        Debug.Log("Step 4 done.");
    }

    private static void AddTransition(
        SerializedProperty listProp,
        NpcActionType action,
        float worsenProbability,
        float keepProbability,
        float improveProbability)
    {
        int index = listProp.arraySize;
        listProp.InsertArrayElementAtIndex(index);

        SerializedProperty entry = listProp.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("action").enumValueIndex = (int)action;
        entry.FindPropertyRelative("worsenProbability").floatValue = worsenProbability;
        entry.FindPropertyRelative("keepProbability").floatValue = keepProbability;
        entry.FindPropertyRelative("improveProbability").floatValue = improveProbability;
    }

    private static string ResolveScriptPath(string fileName, string fallbackFolder)
    {
        string search = Path.GetFileNameWithoutExtension(fileName) + " t:Script";
        string[] guids = AssetDatabase.FindAssets(search);

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (Path.GetFileName(path) == fileName)
                return path;
        }

        EnsureFolder(fallbackFolder);
        return fallbackFolder + "/" + fileName;
    }

    private static void WriteScript(string assetPath, string content)
    {
        string fullPath = Path.GetFullPath(assetPath);
        string directory = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(fullPath, content);
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string[] parts = folderPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }
}