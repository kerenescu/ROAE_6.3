using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CreateBaristaRelationshipTest
{
    private const string TargetFolder = "Assets/ROAE_2/Scripts/NPC_AI/Barista";

    [MenuItem("Tools/ROAE/Create Barista Relationship Test")]
    public static void CreateTest()
    {
        EnsureFolder(TargetFolder);

        DialogueData positiveFollowup = GetOrCreateDialogue(
            "[choice positive - barista - test]",
            new List<LineSpec>
            {
                new LineSpec("Barista", "That was kinder than I expected."),
                new LineSpec("Rina", "I did not want this to feel cold."),
                new LineSpec("Barista", "Then maybe we can speak more openly next time.")
            }
        );

        DialogueData negativeFollowup = GetOrCreateDialogue(
            "[choice negative - barista - test]",
            new List<LineSpec>
            {
                new LineSpec("Barista", "I see."),
                new LineSpec("Rina", "I only needed an answer."),
                new LineSpec("Barista", "Then keep it that way.")
            }
        );

        DialogueData neutral = GetOrCreateDialogueWithChoices(
            "[neutral - barista - test]",
            new List<LineSpec>
            {
                new LineSpec("Barista", "You came back."),
                new LineSpec("Rina", "I wanted to talk for a moment."),
                new LineSpec("Barista", "Then say what kind of conversation this is.")
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
            "[warm - barista - test]",
            new List<LineSpec>
            {
                new LineSpec("Barista", "It is good to see you again."),
                new LineSpec("Rina", "You sound warmer today."),
                new LineSpec("Barista", "That depends on who is standing in front of me.")
            }
        );

        DialogueData guarded = GetOrCreateDialogue(
            "[guarded - barista - test]",
            new List<LineSpec>
            {
                new LineSpec("Barista", "What is it this time."),
                new LineSpec("Rina", "You look tense."),
                new LineSpec("Barista", "Some conversations leave a mark.")
            }
        );

        DialogueData hint = GetOrCreateDialogue(
            "[hint - barista - test]",
            new List<LineSpec>
            {
                new LineSpec("Barista", "If you are looking for a clue, watch who avoids your eyes."),
                new LineSpec("Rina", "That is not very precise."),
                new LineSpec("Barista", "Precise answers rarely survive in this place.")
            }
        );

        UpdateReactionProfile(
            "[barista - reaction profile]",
            neutral,
            warm,
            guarded,
            hint
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Barista relationship test created.");
    }

    private static DialogueData GetOrCreateDialogue(string assetName, List<LineSpec> lines)
    {
        string path = TargetFolder + "/" + assetName + ".asset";
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
        string assetName,
        List<LineSpec> lines,
        List<ChoiceSpec> choices)
    {
        string path = TargetFolder + "/" + assetName + ".asset";
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
        string assetName,
        DialogueData neutral,
        DialogueData warm,
        DialogueData guarded,
        DialogueData hint)
    {
        string path = TargetFolder + "/" + assetName + ".asset";
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