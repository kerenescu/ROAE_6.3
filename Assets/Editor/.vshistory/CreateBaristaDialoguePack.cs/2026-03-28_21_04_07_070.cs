using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class CreateBaristaDialoguePack
{
    private const string TargetFolder = "Assets/ROAE_2/Scripts/NPC_AI/Barista";

    [MenuItem("Tools/ROAE/Create Barista Dialogue Pack")]
    public static void CreatePack()
    {
        EnsureFolder(TargetFolder);

        DialogueData neutral = CreateDialogueAsset(
            "[neutral - barista]",
            new List<(string speaker, string text)>
            {
                ("Barista", "Hei. Ai venit din nou."),
                ("Rina", "Da. Am vrut sa vad ce mai e pe aici."),
                ("Barista", "Nu multe s-au schimbat. Dar uneori e suficient sa stai putin.")
            }
        );

        DialogueData warm = CreateDialogueAsset(
            "[warm - barista]",
            new List<(string speaker, string text)>
            {
                ("Barista", "Ma bucur ca ai revenit."),
                ("Rina", "Nu ma asteptam sa spui asta."),
                ("Barista", "Uneori oamenii au nevoie sa li se spuna ca sunt bineveniti aici.")
            }
        );

        DialogueData guarded = CreateDialogueAsset(
            "[guarded - barista]",
            new List<(string speaker, string text)>
            {
                ("Barista", "Esti iar aici."),
                ("Rina", "Daca deranjez, pot sa plec."),
                ("Barista", "Nu am spus asta. Doar ca unele lucruri se observa dupa un timp.")
            }
        );

        DialogueData hint = CreateDialogueAsset(
            "[hint - barista]",
            new List<(string speaker, string text)>
            {
                ("Barista", "Daca tot cauti raspunsuri, nu te uita doar la ce e in fata ta."),
                ("Rina", "Adica?"),
                ("Barista", "Unii oameni spun mai mult dupa ce ai facut deja un pas spre ei.")
            }
        );

        CreateReactionProfile("[barista - reaction profile]", neutral, warm, guarded, hint);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Barista dialogue pack created.");
    }

    private static DialogueData CreateDialogueAsset(string fileName, List<(string speaker, string text)> lines)
    {
        string path = $"{TargetFolder}/{fileName}.asset";

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
            SerializedProperty element = linesProp.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("Speaker").stringValue = lines[i].speaker;
            element.FindPropertyRelative("Text").stringValue = lines[i].text;
        }

        choicesProp.ClearArray();

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);

        return asset;
    }

    private static void CreateReactionProfile(
        string fileName,
        DialogueData neutral,
        DialogueData warm,
        DialogueData guarded,
        DialogueData hint)
    {
        string path = $"{TargetFolder}/{fileName}.asset";

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
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }
}