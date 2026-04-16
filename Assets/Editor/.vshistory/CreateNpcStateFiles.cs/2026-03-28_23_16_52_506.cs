using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateNpcStateFiles
{
    private const string TargetFolder = "Assets/ROAE_2/Scripts/NPC_AI/CoreStates";

    [MenuItem("Tools/ROAE/Create NPC State Files")]
    public static void CreateFiles()
    {
        EnsureFolder(TargetFolder);

        WriteFile("EmpathyBucket.cs", @"public enum EmpathyBucket
{
    Low,
    Neutral,
    High
}
");

        WriteFile("CreativityBucket.cs", @"public enum CreativityBucket
{
    Low,
    Medium,
    High
}
");

        WriteFile("CorruptionBucket.cs", @"public enum CorruptionBucket
{
    Low,
    Medium,
    High
}
");

        WriteFile("RelationshipBucket.cs", @"public enum RelationshipBucket
{
    Bad,
    Neutral,
    Good
}
");

        WriteFile("NpcDecisionState.cs", @"using System;

[Serializable]
public struct NpcDecisionState
{
    public EmpathyBucket empathy;
    public CreativityBucket creativity;
    public CorruptionBucket corruption;
    public RelationshipBucket relationship;

    public override string ToString()
    {
        return empathy + ""_"" + creativity + ""_"" + corruption + ""_"" + relationship;
    }
}
");

        WriteFile("NpcStateDiscretizer.cs", @"using UnityEngine;

public static class NpcStateDiscretizer
{
    public static NpcDecisionState Build(string npcId)
    {
        NpcDecisionState state = new NpcDecisionState();

        state.empathy = GetEmpathyBucket();
        state.creativity = GetCreativityBucket();
        state.corruption = GetCorruptionBucket();
        state.relationship = GetRelationshipBucket(npcId);

        return state;
    }

    private static EmpathyBucket GetEmpathyBucket()
    {
        if (CreativeCore.Instance == null)
            return EmpathyBucket.Neutral;

        int value = CreativeCore.Instance.empathy;

        if (value <= -2)
            return EmpathyBucket.Low;

        if (value >= 2)
            return EmpathyBucket.High;

        return EmpathyBucket.Neutral;
    }

    private static CreativityBucket GetCreativityBucket()
    {
        if (CreativeCore.Instance == null)
            return CreativityBucket.Medium;

        int value = CreativeCore.Instance.creativity;

        if (value < 35)
            return CreativityBucket.Low;

        if (value > 70)
            return CreativityBucket.High;

        return CreativityBucket.Medium;
    }

    private static CorruptionBucket GetCorruptionBucket()
    {
        if (CreativeCore.Instance == null)
            return CorruptionBucket.Low;

        int value = CreativeCore.Instance.plantCorruption;

        if (value < 3)
            return CorruptionBucket.Low;

        if (value > 6)
            return CorruptionBucket.High;

        return CorruptionBucket.Medium;
    }

    private static RelationshipBucket GetRelationshipBucket(string npcId)
    {
        int value = NpcRelationshipState.GetRelationshipScore(npcId);

        if (value <= -2)
            return RelationshipBucket.Bad;

        if (value >= 2)
            return RelationshipBucket.Good;

        return RelationshipBucket.Neutral;
    }
}
");

        AssetDatabase.Refresh();
        Debug.Log("NPC state files created.");
    }

    private static void WriteFile(string fileName, string content)
    {
        string path = Path.Combine(TargetFolder, fileName);
        File.WriteAllText(path, content);
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