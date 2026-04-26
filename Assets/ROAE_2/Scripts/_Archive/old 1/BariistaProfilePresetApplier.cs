using System.IO;
using UnityEditor;
using UnityEngine;

public static class BaristaProfilePresetApplier
{
    private const string RewardAssetName = "BaristaRewardProfile";
    private const string TransitionAssetName = "BaristaTransitionProfile";

    [MenuItem("Tools/Barista/Apply Stylish Reward + Transition Preset")]
    public static void ApplyPreset()
    {
        Object rewardAsset = FindAssetByExactName(RewardAssetName);
        Object transitionAsset = FindAssetByExactName(TransitionAssetName);

        if (rewardAsset == null)
        {
            Debug.LogError("Could not find asset: " + RewardAssetName);
            return;
        }

        if (transitionAsset == null)
        {
            Debug.LogError("Could not find asset: " + TransitionAssetName);
            return;
        }

        ApplyRewardProfile(rewardAsset);
        ApplyTransitionProfile(transitionAsset);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Applied preset to BaristaRewardProfile and BaristaTransitionProfile.");
    }

    private static Object FindAssetByExactName(string exactName)
    {
        string[] guids = AssetDatabase.FindAssets(exactName);
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName == exactName)
                return AssetDatabase.LoadAssetAtPath<Object>(path);
        }

        return null;
    }

    private static void ApplyRewardProfile(Object asset)
    {
        Undo.RecordObject(asset, "Apply Barista Reward Preset");

        SerializedObject so = new SerializedObject(asset);

        SerializedProperty baseRewards = so.FindProperty("baseRewards");
        SerializedProperty rules = so.FindProperty("rules");

        if (baseRewards == null || rules == null)
        {
            Debug.LogError("Reward profile does not expose baseRewards/rules as expected.");
            return;
        }

        baseRewards.arraySize = 0;
        rules.arraySize = 0;

        AddBaseReward(baseRewards, NpcActionType.Neutral, 1f);
        AddBaseReward(baseRewards, NpcActionType.Warm, 2f);
        AddBaseReward(baseRewards, NpcActionType.Guarded, 2f);
        AddBaseReward(baseRewards, NpcActionType.Hint, 2f);
        AddBaseReward(baseRewards, NpcActionType.WarmHint, 3f);
        AddBaseReward(baseRewards, NpcActionType.GuardedHint, 3f);

        // Empathy
        AddRule(rules, NpcActionType.Warm, 3f, useEmpathy: true, empathy: EmpathyBucket.High);
        AddRule(rules, NpcActionType.WarmHint, 3f, useEmpathy: true, empathy: EmpathyBucket.High);

        AddRule(rules, NpcActionType.Guarded, 3f, useEmpathy: true, empathy: EmpathyBucket.Low);
        AddRule(rules, NpcActionType.GuardedHint, 2f, useEmpathy: true, empathy: EmpathyBucket.Low);
        AddRule(rules, NpcActionType.Warm, -3f, useEmpathy: true, empathy: EmpathyBucket.Low);
        AddRule(rules, NpcActionType.WarmHint, -2f, useEmpathy: true, empathy: EmpathyBucket.Low);

        // Creativity
        AddRule(rules, NpcActionType.Hint, 2f, useCreativity: true, creativity: CreativityBucket.High);
        AddRule(rules, NpcActionType.WarmHint, 3f, useCreativity: true, creativity: CreativityBucket.High);
        AddRule(rules, NpcActionType.GuardedHint, 2f, useCreativity: true, creativity: CreativityBucket.High);

        AddRule(rules, NpcActionType.Hint, 1f, useCreativity: true, creativity: CreativityBucket.Medium);

        AddRule(rules, NpcActionType.Hint, -2f, useCreativity: true, creativity: CreativityBucket.Low);
        AddRule(rules, NpcActionType.WarmHint, -2f, useCreativity: true, creativity: CreativityBucket.Low);
        AddRule(rules, NpcActionType.GuardedHint, -1f, useCreativity: true, creativity: CreativityBucket.Low);
        AddRule(rules, NpcActionType.Neutral, 1f, useCreativity: true, creativity: CreativityBucket.Low);

        // Corruption
        AddRule(rules, NpcActionType.Guarded, 2f, useCorruption: true, corruption: CorruptionBucket.High);
        AddRule(rules, NpcActionType.GuardedHint, 4f, useCorruption: true, corruption: CorruptionBucket.High);
        AddRule(rules, NpcActionType.Warm, -3f, useCorruption: true, corruption: CorruptionBucket.High);
        AddRule(rules, NpcActionType.WarmHint, -2f, useCorruption: true, corruption: CorruptionBucket.High);

        AddRule(rules, NpcActionType.GuardedHint, 1f, useCorruption: true, corruption: CorruptionBucket.Medium);

        AddRule(rules, NpcActionType.Warm, 1f, useCorruption: true, corruption: CorruptionBucket.Low);
        AddRule(rules, NpcActionType.WarmHint, 2f, useCorruption: true, corruption: CorruptionBucket.Low);

        // Relationship
        AddRule(rules, NpcActionType.Warm, 3f, useRelationship: true, relationship: RelationshipBucket.Good);
        AddRule(rules, NpcActionType.WarmHint, 4f, useRelationship: true, relationship: RelationshipBucket.Good);
        AddRule(rules, NpcActionType.Guarded, -3f, useRelationship: true, relationship: RelationshipBucket.Good);
        AddRule(rules, NpcActionType.GuardedHint, -2f, useRelationship: true, relationship: RelationshipBucket.Good);

        AddRule(rules, NpcActionType.Guarded, 3f, useRelationship: true, relationship: RelationshipBucket.Bad);
        AddRule(rules, NpcActionType.GuardedHint, 3f, useRelationship: true, relationship: RelationshipBucket.Bad);
        AddRule(rules, NpcActionType.Warm, -3f, useRelationship: true, relationship: RelationshipBucket.Bad);
        AddRule(rules, NpcActionType.WarmHint, -2f, useRelationship: true, relationship: RelationshipBucket.Bad);

        // Combo rules
        AddRule(
            rules, NpcActionType.WarmHint, 2f,
            useEmpathy: true, empathy: EmpathyBucket.High,
            useCreativity: true, creativity: CreativityBucket.High);

        AddRule(
            rules, NpcActionType.GuardedHint, 3f,
            useEmpathy: true, empathy: EmpathyBucket.Low,
            useCorruption: true, corruption: CorruptionBucket.High);

        AddRule(
            rules, NpcActionType.Hint, 2f,
            useCreativity: true, creativity: CreativityBucket.High,
            useCorruption: true, corruption: CorruptionBucket.Low,
            useRelationship: true, relationship: RelationshipBucket.Bad);

        AddRule(
            rules, NpcActionType.WarmHint, -1f,
            useCorruption: true, corruption: CorruptionBucket.High,
            useRelationship: true, relationship: RelationshipBucket.Good);

        AddRule(
            rules, NpcActionType.GuardedHint, 2f,
            useCorruption: true, corruption: CorruptionBucket.High,
            useRelationship: true, relationship: RelationshipBucket.Good);

        AddRule(
            rules, NpcActionType.GuardedHint, 1f,
            useEmpathy: true, empathy: EmpathyBucket.High,
            useRelationship: true, relationship: RelationshipBucket.Bad);

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);
    }

    private static void ApplyTransitionProfile(Object asset)
    {
        Undo.RecordObject(asset, "Apply Barista Transition Preset");

        SerializedObject so = new SerializedObject(asset);
        SerializedProperty transitions = so.FindProperty("transitions");

        if (transitions == null)
        {
            Debug.LogError("Transition profile does not expose transitions as expected.");
            return;
        }

        transitions.arraySize = 0;

        AddTransition(transitions, NpcActionType.Neutral, 0.20f, 0.60f, 0.20f);
        AddTransition(transitions, NpcActionType.Warm, 0.10f, 0.40f, 0.50f);
        AddTransition(transitions, NpcActionType.Guarded, 0.50f, 0.40f, 0.10f);
        AddTransition(transitions, NpcActionType.Hint, 0.15f, 0.50f, 0.35f);
        AddTransition(transitions, NpcActionType.WarmHint, 0.05f, 0.35f, 0.60f);
        AddTransition(transitions, NpcActionType.GuardedHint, 0.35f, 0.45f, 0.20f);

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);
    }

    private static void AddBaseReward(SerializedProperty array, NpcActionType action, float reward)
    {
        int index = array.arraySize;
        array.InsertArrayElementAtIndex(index);
        SerializedProperty entry = array.GetArrayElementAtIndex(index);

        entry.FindPropertyRelative("action").enumValueIndex = (int)action;
        entry.FindPropertyRelative("reward").floatValue = reward;
    }

    private static void AddTransition(
        SerializedProperty array,
        NpcActionType action,
        float worsen,
        float keep,
        float improve)
    {
        int index = array.arraySize;
        array.InsertArrayElementAtIndex(index);
        SerializedProperty entry = array.GetArrayElementAtIndex(index);

        entry.FindPropertyRelative("action").enumValueIndex = (int)action;
        entry.FindPropertyRelative("worsenProbability").floatValue = worsen;
        entry.FindPropertyRelative("keepProbability").floatValue = keep;
        entry.FindPropertyRelative("improveProbability").floatValue = improve;
    }

    private static void AddRule(
        SerializedProperty array,
        NpcActionType action,
        float rewardDelta,
        bool useEmpathy = false,
        EmpathyBucket empathy = EmpathyBucket.Neutral,
        bool useCreativity = false,
        CreativityBucket creativity = CreativityBucket.Medium,
        bool useCorruption = false,
        CorruptionBucket corruption = CorruptionBucket.Medium,
        bool useRelationship = false,
        RelationshipBucket relationship = RelationshipBucket.Neutral)
    {
        int index = array.arraySize;
        array.InsertArrayElementAtIndex(index);
        SerializedProperty entry = array.GetArrayElementAtIndex(index);

        entry.FindPropertyRelative("action").enumValueIndex = (int)action;
        entry.FindPropertyRelative("rewardDelta").floatValue = rewardDelta;

        entry.FindPropertyRelative("useEmpathy").boolValue = useEmpathy;
        entry.FindPropertyRelative("empathy").enumValueIndex = (int)empathy;

        entry.FindPropertyRelative("useCreativity").boolValue = useCreativity;
        entry.FindPropertyRelative("creativity").enumValueIndex = (int)creativity;

        entry.FindPropertyRelative("useCorruption").boolValue = useCorruption;
        entry.FindPropertyRelative("corruption").enumValueIndex = (int)corruption;

        entry.FindPropertyRelative("useRelationship").boolValue = useRelationship;
        entry.FindPropertyRelative("relationship").enumValueIndex = (int)relationship;
    }
}