using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class UpgradeBaristaCompositeActions
{
    private const string DefaultCoreFolder = "Assets/ROAE_2/Scripts/NPC_AI";
    private const string DefaultBaristaFolder = "Assets/ROAE_2/Scripts/NPC_AI/Barista";

    [MenuItem("Tools/ROAE/Barista/Step 1 - Upgrade Code For Composite Actions")]
    public static void UpgradeCode()
    {
        EnsureFolder(DefaultCoreFolder);
        EnsureFolder(DefaultBaristaFolder);

        WriteScript(
            ResolveScriptPath("NpcActionType.cs", DefaultCoreFolder),
            @"public enum NpcActionType
{
    Neutral,
    Warm,
    Guarded,
    Hint,
    WarmHint,
    GuardedHint
}
");

        WriteScript(
            ResolveScriptPath("NpcReactionProfile.cs", DefaultCoreFolder),
            @"using UnityEngine;

[CreateAssetMenu(fileName = ""NewNpcReactionProfile"", menuName = ""Dialogue System/NPC Reaction Profile"")]
public class NpcReactionProfile : ScriptableObject
{
    [Header(""Authored dialogues for each action"")]
    [SerializeField] private DialogueData neutralDialogue;
    [SerializeField] private DialogueData warmDialogue;
    [SerializeField] private DialogueData guardedDialogue;
    [SerializeField] private DialogueData hintDialogue;
    [SerializeField] private DialogueData warmHintDialogue;
    [SerializeField] private DialogueData guardedHintDialogue;

    public DialogueData GetDialogueForAction(NpcActionType action)
    {
        switch (action)
        {
            case NpcActionType.Warm:
                return warmDialogue != null ? warmDialogue : neutralDialogue;

            case NpcActionType.Guarded:
                return guardedDialogue != null ? guardedDialogue : neutralDialogue;

            case NpcActionType.Hint:
                return hintDialogue != null ? hintDialogue : neutralDialogue;

            case NpcActionType.WarmHint:
                if (warmHintDialogue != null) return warmHintDialogue;
                if (warmDialogue != null) return warmDialogue;
                if (hintDialogue != null) return hintDialogue;
                return neutralDialogue;

            case NpcActionType.GuardedHint:
                if (guardedHintDialogue != null) return guardedHintDialogue;
                if (guardedDialogue != null) return guardedDialogue;
                if (hintDialogue != null) return hintDialogue;
                return neutralDialogue;

            case NpcActionType.Neutral:
            default:
                return neutralDialogue;
        }
    }
}
");

        WriteScript(
            ResolveScriptPath("BaristaActionSelector.cs", DefaultBaristaFolder),
            @"using UnityEngine;

public class BaristaActionSelector : MonoBehaviour
{
    [SerializeField] private string npcId = ""barista"";
    [SerializeField] private NpcActionType fallbackAction = NpcActionType.Neutral;

    public NpcActionType GetAction()
    {
        NpcDecisionState state = NpcStateDiscretizer.Build(npcId);

        if (state.relationship == RelationshipBucket.Bad)
        {
            if (state.creativity == CreativityBucket.High)
                return NpcActionType.GuardedHint;

            return NpcActionType.Guarded;
        }

        if (state.corruption == CorruptionBucket.High)
        {
            if (state.creativity == CreativityBucket.High)
                return NpcActionType.GuardedHint;

            return NpcActionType.Guarded;
        }

        if (state.empathy == EmpathyBucket.Low)
        {
            if (state.creativity == CreativityBucket.High)
                return NpcActionType.GuardedHint;

            return NpcActionType.Guarded;
        }

        if (state.relationship == RelationshipBucket.Good)
        {
            if (state.creativity == CreativityBucket.High)
                return NpcActionType.WarmHint;

            return NpcActionType.Warm;
        }

        if (state.empathy == EmpathyBucket.High && state.creativity == CreativityBucket.High)
            return NpcActionType.WarmHint;

        if (state.creativity == CreativityBucket.High)
            return NpcActionType.Hint;

        if (state.empathy == EmpathyBucket.High)
            return NpcActionType.Warm;

        return fallbackAction;
    }
}
");

        AssetDatabase.Refresh();
        Debug.Log("Step 1 done. Wait for compile, then run Step 2.");
    }

    [MenuItem("Tools/ROAE/Barista/Step 2 - Create Composite Action Assets")]
    public static void CreateAssets()
    {
        string folder = ResolveBaristaAssetFolder();
        EnsureFolder(folder);

        DialogueData positiveFollowup = GetOrCreateDialogue(
            folder,
            "[choice positive - barista - test]",
            new List<LineSpec>
            {
                new LineSpec("Barista", "That was kinder than I expected."),
                new LineSpec("Rina", "I wanted this to feel honest."),
                new LineSpec("Barista", "Then maybe next time I will answer more openly.")
            }
        );

        DialogueData negativeFollowup = GetOrCreateDialogue(
            folder,
            "[choice negative - barista - test]",
            new List<LineSpec>
            {
                new LineSpec("Barista", "I see."),
                new LineSpec("Rina", "I only needed an answer."),
                new LineSpec("Barista", "Then keep it simple between us.")
            }
        );

        DialogueData neutral = GetOrCreateDialogueWithChoices(
            folder,
            "[neutral - barista - test]",
            new List<LineSpec>
            {
                new LineSpec("Barista", "You came back."),
                new LineSpec("Rina", "I wanted to talk for a moment."),
                new LineSpec("Barista", "Then tell me what kind of conversation this is.")
            },
            new List<ChoiceSpec>
            {
                new ChoiceSpec(
                    "I wanted to see how you are.",
                    positiveFollowup,
                    "barista",
                    2
                ),
                new ChoiceSpec(
                    "I just need something from you.",
                    negativeFollowup,
                    "barista",
                    -2
                )
            }
        );

        DialogueData warm = GetOrCreateDialogue(
            folder,
            "[warm - barista - test]",
            new List<LineSpec>
            {
                new LineSpec("Barista", "It is good to see you again."),
                new LineSpec("Rina", "You sound warmer today."),
                new LineSpec("Barista", "That depends on who stands in front of me.")
            }
        );

        DialogueData guarded = GetOrCreateDialogue(
            folder,
            "[guarded - barista - test]",
            new List<LineSpec>
            {
                new LineSpec("Barista", "What is it this time."),
                new LineSpec("Rina", "You look tense."),
                new LineSpec("Barista", "Some conversations leave a mark.")
            }
        );

        DialogueData hint = GetOrCreateDialogue(
            folder,
            "[hint - barista - test]",
            new List<LineSpec>
            {
                new LineSpec("Barista", "If you are looking for a clue, watch who avoids your eyes."),
                new LineSpec("Rina", "That is not very precise."),
                new LineSpec("Barista", "Precise answers rarely survive in this place.")
            }
        );

        DialogueData warmHint = GetOrCreateDialogue(
            folder,
            "[warm hint - barista - test]",
            new List<LineSpec>
            {
                new LineSpec("Barista", "Since you asked gently, I will tell you this much."),
                new LineSpec("Rina", "I am listening."),
                new LineSpec("Barista", "Look for the one who speaks softly when the room gets too still.")
            }
        );

        DialogueData guardedHint = GetOrCreateDialogue(
            folder,
            "[guarded hint - barista - test]",
            new List<LineSpec>
            {
                new LineSpec("Barista", "Do not mistake this for trust."),
                new LineSpec("Rina", "Then why tell me anything at all."),
                new LineSpec("Barista", "Because even now, you should stop looking in the obvious place.")
            }
        );

        UpdateReactionProfile(
            folder,
            "[barista - reaction profile]",
            neutral,
            warm,
            guarded,
            hint,
            warmHint,
            guardedHint
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Step 2 done.");
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

    private static string ResolveBaristaAssetFolder()
    {
        string[] guids = AssetDatabase.FindAssets("t:NpcReactionProfile");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            string name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (name.Contains("barista"))
                return Path.GetDirectoryName(path).Replace("\\", "/");
        }

        return DefaultBaristaFolder;
    }

    private static void WriteScript(string assetPath, string content)
    {
        string fullPath = Path.GetFullPath(assetPath);
        string directory = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(fullPath, content);
    }

    private static DialogueData GetOrCreateDialogue(string folder, string assetName, List<LineSpec> lines)
    {
        string path = folder + "/" + assetName + ".asset";
        DialogueData asset = AssetDatabase.LoadAssetAtPath<DialogueData>(path);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<DialogueData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        SerializedObject so = new SerializedObject(asset);
        SerializedProperty linesProp = so.FindProperty("dialogueLines");
        SerializedProperty choicesProp = so.FindProperty("choices");

        linesProp.ClearArray();
        for (int i = 0; i < lines.Count; i++)
        {
            linesProp.InsertArrayElementAtIndex(i);
            SerializedProperty lineProp = linesProp.GetArrayElementAtIndex(i);
            lineProp.FindPropertyRelative("Speaker").stringValue = lines[i].speaker;
            lineProp.FindPropertyRelative("Text").stringValue = lines[i].text;
        }

        choicesProp.ClearArray();

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);

        return asset;
    }

    private static DialogueData GetOrCreateDialogueWithChoices(
        string folder,
        string assetName,
        List<LineSpec> lines,
        List<ChoiceSpec> choices)
    {
        string path = folder + "/" + assetName + ".asset";
        DialogueData asset = AssetDatabase.LoadAssetAtPath<DialogueData>(path);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<DialogueData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        SerializedObject so = new SerializedObject(asset);
        SerializedProperty linesProp = so.FindProperty("dialogueLines");
        SerializedProperty choicesProp = so.FindProperty("choices");

        linesProp.ClearArray();
        for (int i = 0; i < lines.Count; i++)
        {
            linesProp.InsertArrayElementAtIndex(i);
            SerializedProperty lineProp = linesProp.GetArrayElementAtIndex(i);
            lineProp.FindPropertyRelative("Speaker").stringValue = lines[i].speaker;
            lineProp.FindPropertyRelative("Text").stringValue = lines[i].text;
        }

        choicesProp.ClearArray();
        for (int i = 0; i < choices.Count; i++)
        {
            choicesProp.InsertArrayElementAtIndex(i);
            SerializedProperty choiceProp = choicesProp.GetArrayElementAtIndex(i);

            choiceProp.FindPropertyRelative("choiceText").stringValue = choices[i].choiceText;
            choiceProp.FindPropertyRelative("nextDialogue").objectReferenceValue = choices[i].nextDialogue;

            SerializedProperty statEffectProp = choiceProp.FindPropertyRelative("statEffect");
            if (statEffectProp != null)
            {
                SerializedProperty creativityProp = statEffectProp.FindPropertyRelative("creativity");
                SerializedProperty empathyProp = statEffectProp.FindPropertyRelative("empathy");
                SerializedProperty corruptionProp = statEffectProp.FindPropertyRelative("plantCorruption");

                if (creativityProp != null) creativityProp.intValue = 0;
                if (empathyProp != null) empathyProp.intValue = 0;
                if (corruptionProp != null) corruptionProp.intValue = 0;
            }

            SerializedProperty relationshipEffectProp = choiceProp.FindPropertyRelative("relationshipEffect");
            if (relationshipEffectProp != null)
            {
                SerializedProperty npcIdProp = relationshipEffectProp.FindPropertyRelative("npcId");
                SerializedProperty amountProp = relationshipEffectProp.FindPropertyRelative("amount");

                if (npcIdProp != null) npcIdProp.stringValue = choices[i].npcId;
                if (amountProp != null) amountProp.intValue = choices[i].amount;
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);

        return asset;
    }

    private static void UpdateReactionProfile(
        string folder,
        string assetName,
        DialogueData neutral,
        DialogueData warm,
        DialogueData guarded,
        DialogueData hint,
        DialogueData warmHint,
        DialogueData guardedHint)
    {
        string path = folder + "/" + assetName + ".asset";
        NpcReactionProfile asset = AssetDatabase.LoadAssetAtPath<NpcReactionProfile>(path);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<NpcReactionProfile>();
            AssetDatabase.CreateAsset(asset, path);
        }

        SerializedObject so = new SerializedObject(asset);
        so.FindProperty("neutralDialogue").objectReferenceValue = neutral;
        so.FindProperty("warmDialogue").objectReferenceValue = warm;
        so.FindProperty("guardedDialogue").objectReferenceValue = guarded;
        so.FindProperty("hintDialogue").objectReferenceValue = hint;
        so.FindProperty("warmHintDialogue").objectReferenceValue = warmHint;
        so.FindProperty("guardedHintDialogue").objectReferenceValue = guardedHint;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(asset);
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

    private struct LineSpec
    {
        public string speaker;
        public string text;

        public LineSpec(string speaker, string text)
        {
            this.speaker = speaker;
            this.text = text;
        }
    }

    private struct ChoiceSpec
    {
        public string choiceText;
        public DialogueData nextDialogue;
        public string npcId;
        public int amount;

        public ChoiceSpec(string choiceText, DialogueData nextDialogue, string npcId, int amount)
        {
            this.choiceText = choiceText;
            this.nextDialogue = nextDialogue;
            this.npcId = npcId;
            this.amount = amount;
        }
    }
}