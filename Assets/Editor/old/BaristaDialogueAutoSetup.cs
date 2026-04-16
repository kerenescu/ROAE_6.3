using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BaristaDialogueAutoSetup
{
    private const string FolderPath = "Assets/ROAE_2/Scripts/NPC_AI/Barista";
    private const bool OverwriteExistingChoices = true;

    private const string PositiveChoiceAssetName = "[choice positive - barista - test]";
    private const string NegativeChoiceAssetName = "[choice negative - barista - test]";

    private static readonly string[] TargetDialogueNames =
    {
        "[neutral - barista]",
        "[neutral - barista - test]",
        "[warm - barista]",
        "[warm - barista - test]",
        "[guarded - barista]",
        "[guarded - barista - test]",
        "[hint - barista]",
        "[hint - barista - test]",
        "[warm hint - barista]",
        "[warm hint - barista - test]",
        "[guarded hint - barista]",
        "[guarded hint - barista - test]"
    };

    [MenuItem("Tools/Barista/Auto Setup Dialogue Choices")]
    public static void AutoSetupDialogueChoices()
    {
        Object positiveDialogue = FindAssetByExactName(PositiveChoiceAssetName);
        Object negativeDialogue = FindAssetByExactName(NegativeChoiceAssetName);

        if (positiveDialogue == null || negativeDialogue == null)
        {
            Debug.LogError("Nu am gasit asset-urile de choice positive/negative.");
            return;
        }

        int changedCount = 0;

        foreach (string assetName in TargetDialogueNames)
        {
            Object dialogueAsset = FindAssetByExactName(assetName);
            if (dialogueAsset == null)
            {
                Debug.LogWarning("Lipseste asset-ul: " + assetName);
                continue;
            }

            SerializedObject so = new SerializedObject(dialogueAsset);
            SerializedProperty choicesProp = so.FindProperty("choices");

            if (choicesProp == null)
            {
                Debug.LogWarning("Asset-ul nu are campul 'choices': " + assetName);
                continue;
            }

            if (!OverwriteExistingChoices && choicesProp.arraySize > 0)
            {
                Debug.Log("Skip (deja are choices): " + assetName);
                continue;
            }

            Undo.RecordObject(dialogueAsset, "Auto setup barista dialogue choices");

            choicesProp.arraySize = 2;

            ChoicePreset preset = BuildPreset(assetName);

            SetChoice(
                choicesProp.GetArrayElementAtIndex(0),
                preset.positiveText,
                positiveDialogue,
                0,
                0,
                0,
                "barista",
                2
            );

            SetChoice(
                choicesProp.GetArrayElementAtIndex(1),
                preset.negativeText,
                negativeDialogue,
                0,
                0,
                0,
                "barista",
                -2
            );

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(dialogueAsset);
            changedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Barista dialogue setup terminat. Asset-uri modificate: " + changedCount);
    }

    private static void SetChoice(
        SerializedProperty choiceProp,
        string choiceText,
        Object nextDialogue,
        int creativity,
        int empathy,
        int plantCorruption,
        string npcId,
        int relationshipAmount)
    {
        SerializedProperty choiceTextProp = choiceProp.FindPropertyRelative("choiceText");
        SerializedProperty nextDialogueProp = choiceProp.FindPropertyRelative("nextDialogue");

        SerializedProperty statEffectProp = choiceProp.FindPropertyRelative("statEffect");
        SerializedProperty creativityProp = statEffectProp.FindPropertyRelative("creativity");
        SerializedProperty empathyProp = statEffectProp.FindPropertyRelative("empathy");
        SerializedProperty corruptionProp = statEffectProp.FindPropertyRelative("plantCorruption");

        SerializedProperty relationshipEffectProp = choiceProp.FindPropertyRelative("relationshipEffect");
        SerializedProperty npcIdProp = relationshipEffectProp.FindPropertyRelative("npcId");
        SerializedProperty amountProp = relationshipEffectProp.FindPropertyRelative("amount");

        choiceTextProp.stringValue = choiceText;
        nextDialogueProp.objectReferenceValue = nextDialogue;

        creativityProp.intValue = creativity;
        empathyProp.intValue = empathy;
        corruptionProp.intValue = plantCorruption;

        npcIdProp.stringValue = npcId;
        amountProp.intValue = relationshipAmount;
    }

    private static Object FindAssetByExactName(string exactName)
    {
        string[] guids = AssetDatabase.FindAssets("t:Object", new[] { FolderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);

            if (fileName == exactName)
                return AssetDatabase.LoadAssetAtPath<Object>(path);
        }

        return null;
    }

    private static ChoicePreset BuildPreset(string assetName)
    {
        string lower = assetName.ToLowerInvariant();

        if (lower.Contains("guarded hint"))
        {
            return new ChoicePreset
            {
                positiveText = "All right. I will listen carefully.",
                negativeText = "Then stop speaking in riddles."
            };
        }

        if (lower.Contains("guarded"))
        {
            return new ChoicePreset
            {
                positiveText = "All right. I will not push.",
                negativeText = "You are avoiding the point."
            };
        }

        if (lower.Contains("warm hint"))
        {
            return new ChoicePreset
            {
                positiveText = "Thank you. I will remember that.",
                negativeText = "That still tells me too little."
            };
        }

        if (lower.Contains("hint"))
        {
            return new ChoicePreset
            {
                positiveText = "Thank you. I will keep that in mind.",
                negativeText = "That is not enough for me."
            };
        }

        if (lower.Contains("warm"))
        {
            return new ChoicePreset
            {
                positiveText = "I am glad you said that.",
                negativeText = "I did not come here for comfort."
            };
        }

        return new ChoicePreset
        {
            positiveText = "I wanted to see how you are.",
            negativeText = "I just need something from you."
        };
    }

    private struct ChoicePreset
    {
        public string positiveText;
        public string negativeText;
    }
}