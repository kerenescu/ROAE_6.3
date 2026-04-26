using UnityEditor;
using UnityEngine;

public static class RebalanceBaristaRewardsForNeutralFlow
{
    private const string RewardAssetPath = "Assets/Resources/NPC_AI/Barista/BaristaRewardProfile.asset";

    [MenuItem("Tools/ROAE/Repair/Rebalance Barista Reward Profile")]
    public static void Rebalance()
    {
        Object assetObject = AssetDatabase.LoadAssetAtPath<Object>(RewardAssetPath);
        if (assetObject == null)
        {
            Debug.LogError("BaristaRewardProfile.asset not found.");
            return;
        }

        SerializedObject so = new SerializedObject(assetObject);
        SerializedProperty baseRewardsProp = so.FindProperty("baseRewards");
        SerializedProperty rulesProp = so.FindProperty("rules");

        if (baseRewardsProp == null || rulesProp == null)
        {
            Debug.LogError("Reward profile fields not found.");
            return;
        }

        baseRewardsProp.ClearArray();
        rulesProp.ClearArray();

        AddBaseReward(baseRewardsProp, NpcActionType.Neutral, 2.0f);
        AddBaseReward(baseRewardsProp, NpcActionType.Warm, 1.0f);
        AddBaseReward(baseRewardsProp, NpcActionType.Guarded, 1.0f);
        AddBaseReward(baseRewardsProp, NpcActionType.Hint, 1.0f);
        AddBaseReward(baseRewardsProp, NpcActionType.WarmHint, 1.0f);
        AddBaseReward(baseRewardsProp, NpcActionType.GuardedHint, 1.0f);

        AddRule(rulesProp, NpcActionType.Warm, 2.0f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Good);
        AddRule(rulesProp, NpcActionType.WarmHint, 4.0f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Good);
        AddRule(rulesProp, NpcActionType.Guarded, -2.0f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Good);
        AddRule(rulesProp, NpcActionType.GuardedHint, -3.0f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Good);

        AddRule(rulesProp, NpcActionType.Guarded, 2.0f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Bad);
        AddRule(rulesProp, NpcActionType.GuardedHint, 4.0f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Bad);
        AddRule(rulesProp, NpcActionType.Warm, -2.0f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Bad);
        AddRule(rulesProp, NpcActionType.WarmHint, -3.0f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, true, RelationshipBucket.Bad);

        AddRule(rulesProp, NpcActionType.Warm, 1.5f, true, EmpathyBucket.High, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.WarmHint, 1.5f, true, EmpathyBucket.High, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);

        AddRule(rulesProp, NpcActionType.Guarded, 1.5f, true, EmpathyBucket.Low, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.GuardedHint, 1.5f, true, EmpathyBucket.Low, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.Warm, -1.5f, true, EmpathyBucket.Low, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.WarmHint, -1.5f, true, EmpathyBucket.Low, false, CreativityBucket.Medium, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);

        AddRule(rulesProp, NpcActionType.Hint, 2.0f, false, EmpathyBucket.Neutral, true, CreativityBucket.High, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.WarmHint, 2.0f, false, EmpathyBucket.Neutral, true, CreativityBucket.High, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.GuardedHint, 2.0f, false, EmpathyBucket.Neutral, true, CreativityBucket.High, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);

        AddRule(rulesProp, NpcActionType.Hint, -1.0f, false, EmpathyBucket.Neutral, true, CreativityBucket.Low, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.WarmHint, -1.0f, false, EmpathyBucket.Neutral, true, CreativityBucket.Low, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.GuardedHint, -1.0f, false, EmpathyBucket.Neutral, true, CreativityBucket.Low, false, CorruptionBucket.Medium, false, RelationshipBucket.Neutral);

        AddRule(rulesProp, NpcActionType.Guarded, 2.0f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, true, CorruptionBucket.High, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.GuardedHint, 2.5f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, true, CorruptionBucket.High, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.Warm, -1.5f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, true, CorruptionBucket.High, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.WarmHint, -2.0f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, true, CorruptionBucket.High, false, RelationshipBucket.Neutral);

        AddRule(rulesProp, NpcActionType.Warm, 0.5f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, true, CorruptionBucket.Low, false, RelationshipBucket.Neutral);
        AddRule(rulesProp, NpcActionType.WarmHint, 0.5f, false, EmpathyBucket.Neutral, false, CreativityBucket.Medium, true, CorruptionBucket.Low, false, RelationshipBucket.Neutral);

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(assetObject);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        BaristaPolicy.RebuildPolicy();

        Debug.Log("Barista reward profile rebalanced.");
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
}