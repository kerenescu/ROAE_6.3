using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateGenericRewardSystemStep
{
    private const string CoreFolder = "Assets/ROAE_2/Scripts/NPC_AI/CoreStates";
    private const string BaristaFolder = "Assets/ROAE_2/Scripts/NPC_AI/Barista";
    private const string ResourcesFolder = "Assets/Resources/NPC_AI/Barista";
    private const string RewardAssetPath = "Assets/Resources/NPC_AI/Barista/BaristaRewardProfile.asset";

    [MenuItem("Tools/ROAE/NPC AI/Step 1 - Create Generic Reward Code")]
    public static void CreateCode()
    {
        EnsureFolder(CoreFolder);
        EnsureFolder(BaristaFolder);
        EnsureFolder(ResourcesFolder);

        WriteScript(
            ResolveScriptPath("NpcBaseActionReward.cs", CoreFolder),
            @"using System;

[Serializable]
public class NpcBaseActionReward
{
    public NpcActionType action;
    public float reward;
}
");

        WriteScript(
            ResolveScriptPath("NpcRewardRule.cs", CoreFolder),
            @"using System;

[Serializable]
public class NpcRewardRule
{
    public NpcActionType action;
    public float rewardDelta;

    public bool useEmpathy;
    public EmpathyBucket empathy;

    public bool useCreativity;
    public CreativityBucket creativity;

    public bool useCorruption;
    public CorruptionBucket corruption;

    public bool useRelationship;
    public RelationshipBucket relationship;

    public bool Matches(NpcDecisionState state, NpcActionType candidateAction)
    {
        if (candidateAction != action)
            return false;

        if (useEmpathy && state.empathy != empathy)
            return false;

        if (useCreativity && state.creativity != creativity)
            return false;

        if (useCorruption && state.corruption != corruption)
            return false;

        if (useRelationship && state.relationship != relationship)
            return false;

        return true;
    }
}
");

        WriteScript(
            ResolveScriptPath("NpcRewardProfile.cs", CoreFolder),
            @"using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = ""NewNpcRewardProfile"", menuName = ""Dialogue System/NPC Reward Profile"")]
public class NpcRewardProfile : ScriptableObject
{
    [SerializeField] private List<NpcBaseActionReward> baseRewards = new List<NpcBaseActionReward>();
    [SerializeField] private List<NpcRewardRule> rules = new List<NpcRewardRule>();

    public IReadOnlyList<NpcBaseActionReward> BaseRewards => baseRewards;
    public IReadOnlyList<NpcRewardRule> Rules => rules;
}
");

        WriteScript(
            ResolveScriptPath("NpcRewardEvaluator.cs", CoreFolder),
            @"public static class NpcRewardEvaluator
{
    public static float GetReward(NpcRewardProfile profile, NpcDecisionState state, NpcActionType action)
    {
        if (profile == null)
            return 0f;

        float reward = 0f;

        var baseRewards = profile.BaseRewards;
        for (int i = 0; i < baseRewards.Count; i++)
        {
            if (baseRewards[i].action == action)
                reward += baseRewards[i].reward;
        }

        var rules = profile.Rules;
        for (int i = 0; i < rules.Count; i++)
        {
            if (rules[i].Matches(state, action))
                reward += rules[i].rewardDelta;
        }

        return reward;
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

    private static Dictionary<NpcDecisionState, NpcActionType> cachedPolicy;
    private static NpcRewardProfile cachedRewardProfile;

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
        BuildPolicy();
    }

    private static void BuildPolicy()
    {
        if (cachedRewardProfile == null)
            cachedRewardProfile = Resources.Load<NpcRewardProfile>(RewardProfilePath);

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
            (state, action) => NpcRewardEvaluator.GetReward(cachedRewardProfile, state, action),
            GetTransitions,
            0.85f,
            0.0001f,
            100
        );
    }

    private static List<StateTransition> GetTransitions(NpcDecisionState state, NpcActionType action)
    {
        List<StateTransition> transitions = new List<StateTransition>();

        switch (action)
        {
            case NpcActionType.Warm:
                transitions.Add(new StateTransition(SetRelationship(state, ImproveRelationship(state.relationship)), 0.70f));
                transitions.Add(new StateTransition(state, 0.30f));
                break;

            case NpcActionType.Guarded:
                transitions.Add(new StateTransition(SetRelationship(state, WorsenRelationship(state.relationship)), 0.70f));
                transitions.Add(new StateTransition(state, 0.30f));
                break;

            case NpcActionType.Hint:
                transitions.Add(new StateTransition(state, 1.00f));
                break;

            case NpcActionType.WarmHint:
                transitions.Add(new StateTransition(SetRelationship(state, ImproveRelationship(state.relationship)), 0.80f));
                transitions.Add(new StateTransition(state, 0.20f));
                break;

            case NpcActionType.GuardedHint:
                transitions.Add(new StateTransition(SetRelationship(state, WorsenRelationship(state.relationship)), 0.60f));
                transitions.Add(new StateTransition(state, 0.40f));
                break;

            case NpcActionType.Neutral:
            default:
                transitions.Add(new StateTransition(state, 1.00f));
                break;
        }

        return transitions;
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

            case RelationshipBucket.Good:
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

            case RelationshipBucket.Bad:
            default:
                return RelationshipBucket.Bad;
        }
    }
}
");

        AssetDatabase.Refresh();
        Debug.Log("Step 1 done. Wait for compile, then run Step 2.");
    }

    [MenuItem("Tools/ROAE/NPC AI/Step 2 - Create Barista Reward Asset")]
    public static void CreateRewardAsset()
    {
        EnsureFolder(ResourcesFolder);

        NpcRewardProfile asset = AssetDatabase.LoadAssetAtPath<NpcRewardProfile>(RewardAssetPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<NpcRewardProfile>();
            AssetDatabase.CreateAsset(asset, RewardAssetPath);
        }

        SerializedObject so = new SerializedObject(asset);
        SerializedProperty baseRewardsProp = so.FindProperty("baseRewards");
        SerializedProperty rulesProp = so.FindProperty("rules");

        baseRewardsProp.ClearArray();
        rulesProp.ClearArray();

        AddBaseReward(baseRewardsProp, NpcActionType.Neutral, 1f);
        AddBaseReward(baseRewardsProp, NpcActionType.Warm, 2f);
        AddBaseReward(baseRewardsProp, NpcActionType.Guarded, 2f);
        AddBaseReward(baseRewardsProp, NpcActionType.Hint, 2f);
        AddBaseReward(baseRewardsProp, NpcActionType.WarmHint, 3f);
        AddBaseReward(baseRewardsProp, NpcActionType.GuardedHint, 3f);

        AddRule(rulesProp, NpcActionType.Warm, 3f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Good);
        AddRule(rulesProp, NpcActionType.WarmHint, 3f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Good);
        AddRule(rulesProp, NpcActionType.Guarded, -3f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Good);
        AddRule(rulesProp, NpcActionType.GuardedHint, -3f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Good);

        AddRule(rulesProp, NpcActionType.Guarded, 3f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Bad);
        AddRule(rulesProp, NpcActionType.GuardedHint, 3f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Bad);
        AddRule(rulesProp, NpcActionType.Warm, -3f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Bad);
        AddRule(rulesProp, NpcActionType.WarmHint, -3f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Bad);

        AddRule(rulesProp, NpcActionType.Warm, 2f, true, EmpathyBucket.High, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.WarmHint, 2f, true, EmpathyBucket.High, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);

        AddRule(rulesProp, NpcActionType.Guarded, 2f, true, EmpathyBucket.Low, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.GuardedHint, 2f, true, EmpathyBucket.Low, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.Warm, -2f, true, EmpathyBucket.Low, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.WarmHint, -2f, true, EmpathyBucket.Low, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);

        AddRule(rulesProp, NpcActionType.Hint, 2f, false, EmpathyBucket.Neutral, true, CreativityBucket.High, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.WarmHint, 2f, false, EmpathyBucket.Neutral, true, CreativityBucket.High, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.GuardedHint, 2f, false, EmpathyBucket.Neutral, true, CreativityBucket.High, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);

        AddRule(rulesProp, NpcActionType.Hint, -1f, false, EmpathyBucket.Neutral, true, CreativityBucket.Low, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.WarmHint, -1f, false, EmpathyBucket.Neutral, true, CreativityBucket.Low, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.GuardedHint, -1f, false, EmpathyBucket.Neutral, true, CreativityBucket.Low, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);

        AddRule(rulesProp, NpcActionType.Guarded, 2f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, true, CorruptionBucket.High, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.GuardedHint, 2f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, true, CorruptionBucket.High, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.Warm, -2f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, true, CorruptionBucket.High, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.WarmHint, -2f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, true, CorruptionBucket.High, false, RelationshipBucket.Neutral);

        AddRule(rulesProp, NpcActionType.Warm, 1f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, true, CorruptionBucket.Low, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.WarmHint, 1f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, true, CorruptionBucket.Low, false, RelationshipBucket.Neutral);

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        BaristaPolicy.RebuildPolicy();

        Debug.Log("Step 2 done.");
    }

    private static void AddBaseReward(SerializedProperty listProp, NpcActionType action, float reward)
    {
        int index = listProp.arraySize;
        listProp.InsertArrayElementAtIndex(index);

        SerializedProperty entry = listProp.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("action").enumValueIndex = (int)action;
        entry.FindPropertyRelative("reward").floatValue = reward;
    }

    private static void AddRule(
        SerializedProperty listProp,
        NpcActionType action,
        float rewardDelta,
        bool useEmpathy,
        EmpathyBucket empathy,
        bool useCreativity,
        CreativityBucket creativity,
        bool useCorruption,
        CorruptionBucket corruption,
        bool useRelationship,
        RelationshipBucket relationship)
    {
        int index = listProp.arraySize;
        listProp.InsertArrayElementAtIndex(index);

        SerializedProperty rule = listProp.GetArrayElementAtIndex(index);
        rule.FindPropertyRelative("action").enumValueIndex = (int)action;
        rule.FindPropertyRelative("rewardDelta").floatValue = rewardDelta;

        rule.FindPropertyRelative("useEmpathy").boolValue = useEmpathy;
        rule.FindPropertyRelative("empathy").enumValueIndex = (int)empathy;

        rule.FindPropertyRelative("useCreativity").boolValue = useCreativity;
        rule.FindPropertyRelative("creativity").enumValueIndex = (int)creativity;

        rule.FindPropertyRelative("useCorruption").boolValue = useCorruption;
        rule.FindPropertyRelative("corruption").enumValueIndex = (int)corruption;

        rule.FindPropertyRelative("useRelationship").boolValue = useRelationship;
        rule.FindPropertyRelative("relationship").enumValueIndex = (int)relationship;
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