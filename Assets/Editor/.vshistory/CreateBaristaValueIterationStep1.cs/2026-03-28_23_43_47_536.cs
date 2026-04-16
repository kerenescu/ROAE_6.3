using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateBaristaValueIterationStep1
{
    private const string CoreFolder = "Assets/ROAE_2/Scripts/NPC_AI/CoreStates";
    private const string BaristaFolder = "Assets/ROAE_2/Scripts/NPC_AI/Barista";

    [MenuItem("Tools/ROAE/Barista/Create Value Iteration Step 1")]
    public static void CreateFiles()
    {
        EnsureFolder(CoreFolder);
        EnsureFolder(BaristaFolder);

        WriteScript(
            ResolveScriptPath("NpcStateSpaceGenerator.cs", CoreFolder),
            @"using System;
using System.Collections.Generic;

public static class NpcStateSpaceGenerator
{
    public static List<NpcDecisionState> GenerateAllStates()
    {
        List<NpcDecisionState> states = new List<NpcDecisionState>();

        foreach (EmpathyBucket empathy in Enum.GetValues(typeof(EmpathyBucket)))
        {
            foreach (CreativityBucket creativity in Enum.GetValues(typeof(CreativityBucket)))
            {
                foreach (CorruptionBucket corruption in Enum.GetValues(typeof(CorruptionBucket)))
                {
                    foreach (RelationshipBucket relationship in Enum.GetValues(typeof(RelationshipBucket)))
                    {
                        NpcDecisionState state = new NpcDecisionState
                        {
                            empathy = empathy,
                            creativity = creativity,
                            corruption = corruption,
                            relationship = relationship
                        };

                        states.Add(state);
                    }
                }
            }
        }

        return states;
    }
}
");

        WriteScript(
            ResolveScriptPath("BaristaRewardModel.cs", BaristaFolder),
            @"public static class BaristaRewardModel
{
    public static NpcActionType[] GetAvailableActions()
    {
        return new[]
        {
            NpcActionType.Neutral,
            NpcActionType.Warm,
            NpcActionType.Guarded,
            NpcActionType.Hint,
            NpcActionType.WarmHint,
            NpcActionType.GuardedHint
        };
    }

    public static float GetReward(NpcDecisionState state, NpcActionType action)
    {
        float reward = 0f;

        switch (action)
        {
            case NpcActionType.Neutral:
                reward += 1f;
                break;

            case NpcActionType.Warm:
                reward += 2f;
                break;

            case NpcActionType.Guarded:
                reward += 2f;
                break;

            case NpcActionType.Hint:
                reward += 2f;
                break;

            case NpcActionType.WarmHint:
                reward += 3f;
                break;

            case NpcActionType.GuardedHint:
                reward += 3f;
                break;
        }

        if (state.relationship == RelationshipBucket.Good)
        {
            if (IsWarmAction(action))
                reward += 3f;

            if (action == NpcActionType.Guarded || action == NpcActionType.GuardedHint)
                reward -= 3f;
        }

        if (state.relationship == RelationshipBucket.Bad)
        {
            if (action == NpcActionType.Guarded || action == NpcActionType.GuardedHint)
                reward += 3f;

            if (IsWarmAction(action))
                reward -= 3f;
        }

        if (state.empathy == EmpathyBucket.High)
        {
            if (IsWarmAction(action))
                reward += 2f;
        }

        if (state.empathy == EmpathyBucket.Low)
        {
            if (action == NpcActionType.Guarded || action == NpcActionType.GuardedHint)
                reward += 2f;

            if (IsWarmAction(action))
                reward -= 2f;
        }

        if (state.creativity == CreativityBucket.High)
        {
            if (HasHint(action))
                reward += 2f;
        }

        if (state.creativity == CreativityBucket.Low)
        {
            if (HasHint(action))
                reward -= 1f;
        }

        if (state.corruption == CorruptionBucket.High)
        {
            if (action == NpcActionType.Guarded || action == NpcActionType.GuardedHint)
                reward += 2f;

            if (IsWarmAction(action))
                reward -= 2f;
        }

        if (state.corruption == CorruptionBucket.Low)
        {
            if (IsWarmAction(action))
                reward += 1f;
        }

        return reward;
    }

    private static bool IsWarmAction(NpcActionType action)
    {
        return action == NpcActionType.Warm || action == NpcActionType.WarmHint;
    }

    private static bool HasHint(NpcActionType action)
    {
        return action == NpcActionType.Hint ||
               action == NpcActionType.WarmHint ||
               action == NpcActionType.GuardedHint;
    }
}
");

        AssetDatabase.Refresh();
        Debug.Log("Value iteration step 1 files created.");
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