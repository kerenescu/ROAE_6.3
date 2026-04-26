using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RepairBaristaTestSetup
{
    private const string ResourcesFolder = "Assets/Resources/NPC_AI/Barista";
    private const string TransitionAssetPath = "Assets/Resources/NPC_AI/Barista/BaristaTransitionProfile.asset";

    [MenuItem("Tools/ROAE/Repair/Barista Transition Asset")]
    public static void RepairTransitionAsset()
    {
        EnsureFolder(ResourcesFolder);

        Object assetObject = AssetDatabase.LoadAssetAtPath<Object>(TransitionAssetPath);
        if (assetObject == null)
        {
            ScriptableObject newAsset = ScriptableObject.CreateInstance("NpcTransitionProfile");
            if (newAsset == null)
            {
                Debug.LogError("NpcTransitionProfile type not available.");
                return;
            }

            AssetDatabase.CreateAsset(newAsset, TransitionAssetPath);
            assetObject = newAsset;
        }

        SerializedObject so = new SerializedObject(assetObject);
        SerializedProperty transitionsProp = so.FindProperty("transitions");

        if (transitionsProp == null)
        {
            Debug.LogError("Transition asset does not contain transitions.");
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

        Debug.Log("BaristaTransitionProfile.asset repaired.");
    }

    [MenuItem("Tools/ROAE/Repair/Barista Neutral Choice Dialogue")]
    public static void RepairNeutralChoiceDialogue()
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
                new LineSpec("Barista", "Then let us keep things simple.")
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
                new ChoiceSpec("I wanted to see how you are.", positiveFollowup, "barista", 2),
                new ChoiceSpec("I just need something from you.", negativeFollowup, "barista", -2)
            }
        );

        Object reactionProfileObject = LoadBaristaReactionProfile();
        if (reactionProfileObject != null)
        {
            SerializedObject profileSo = new SerializedObject(reactionProfileObject);
            SerializedProperty neutralDialogueProp = profileSo.FindProperty("neutralDialogue");
            if (neutralDialogueProp != null)
            {
                neutralDialogueProp.objectReferenceValue = neutral;
                profileSo.ApplyModifiedProperties();
                EditorUtility.SetDirty(reactionProfileObject);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Barista neutral choice dialogue repaired.");
    }

    [MenuItem("Tools/ROAE/Repair/Force Neutral In Open Scene")]
    public static void ForceNeutralInOpenScene()
    {
        BaristaDialogueTrigger[] triggers = Object.FindObjectsByType<BaristaDialogueTrigger>(FindObjectsSortMode.None);

        if (triggers == null || triggers.Length == 0)
        {
            Debug.LogWarning("No BaristaDialogueTrigger found in the open scene.");
            return;
        }

        for (int i = 0; i < triggers.Length; i++)
        {
            SerializedObject so = new SerializedObject(triggers[i]);

            SerializedProperty selectorProp = so.FindProperty("actionSelector");
            if (selectorProp != null)
                selectorProp.objectReferenceValue = null;

            SerializedProperty actionProp = so.FindProperty("currentAction");
            if (actionProp != null)
                actionProp.enumValueIndex = (int)NpcActionType.Neutral;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(triggers[i]);
        }

        if (EditorSceneManager.GetActiveScene().IsValid())
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("Forced Neutral in open scene. Save the scene if needed.");
    }

    private static Object LoadBaristaReactionProfile()
    {
        string[] guids = AssetDatabase.FindAssets("t:NpcReactionProfile");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            string name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (name.Contains("barista"))
                return AssetDatabase.LoadAssetAtPath<Object>(path);
        }

        return null;
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

        return "Assets/ROAE_2/Scripts/NPC_AI/Barista";
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

        if (choicesProp != null)
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

            SerializedProperty choiceTextProp = choiceProp.FindPropertyRelative("choiceText");
            if (choiceTextProp != null)
                choiceTextProp.stringValue = choices[i].choiceText;

            SerializedProperty nextDialogueProp = choiceProp.FindPropertyRelative("nextDialogue");
            if (nextDialogueProp != null)
                nextDialogueProp.objectReferenceValue = choices[i].nextDialogue;

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