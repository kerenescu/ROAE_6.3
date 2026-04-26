using UnityEditor;
using UnityEngine;

public static class BaristaWelcomeFlowAssetUpdater
{
    private const string DialoguesFolder = "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Dialogues";
    private const string EffectsFolder = "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Effects";

    [MenuItem("ROAE/Barista/Rewrite Flow Assets (Drink + Cola)")]
    private static void RewriteFlowAssets()
    {
        EnsureFolder(DialoguesFolder);
        EnsureFolder(EffectsFolder);

        DialogueData orderClean = LoadDialogue("BW_Order_Menu_Clean");
        DialogueData orderWarm = LoadDialogue("BW_Order_Menu_Warm");
        DialogueData orderStrange = LoadDialogue("BW_Order_Menu_Strange");

        DialogueData preparingNeutral = LoadDialogue("BW_Drink_Preparing_Neutral");
        DialogueData preparingWarm = LoadDialogue("BW_Drink_Preparing_Warm");
        DialogueData preparingMischievous = LoadDialogue("BW_Drink_Preparing_Mischievous");

        DialogueData alreadyHasCola = LoadDialogue("BW_Already_Has_Drink_Cola");
        DialogueData alreadyHasSap = LoadDialogue("BW_Already_Has_Drink_Sap");

        BaristaWelcomeChoiceEffect orderCola = GetOrCreateChoiceEffect(
            "BW_OrderCola",
            BaristaWelcomeChoiceCommand.OrderCola);

        BaristaWelcomeChoiceEffect orderSap = GetOrCreateChoiceEffect(
            "BW_OrderPhotosyntheticSap",
            BaristaWelcomeChoiceCommand.OrderPhotosyntheticSap);

        BaristaWelcomeChoiceEffect giveAcceptedDrink = GetOrCreateChoiceEffect(
            "BW_GiveAcceptedDrinkIfPossible",
            BaristaWelcomeChoiceCommand.GiveAcceptedDrinkIfPossible);

        WritePreparingDialogue(
            preparingNeutral,
            "Barista",
            "Take a seat. I will prepare your photosynthetic sap right away."
        );

        WritePreparingDialogue(
            preparingWarm,
            "Barista",
            "Take a seat. I will prepare your photosynthetic sap right away."
        );

        WritePreparingDialogue(
            preparingMischievous,
            "Barista",
            "Take a seat. I will prepare your photosynthetic sap right away."
        );

        WriteSingleLineDialogue(
            alreadyHasCola,
            "Rina",
            "I already have a drink."
        );

        WriteSingleLineDialogue(
            alreadyHasSap,
            "Rina",
            "I already have a drink."
        );

        WriteOrderMenuDialogue(
            orderClean,
            "Barista",
            "Have you decided what you would like to order?",
            preparingNeutral,
            orderSap,
            giveAcceptedDrink,
            orderCola
        );

        WriteOrderMenuDialogue(
            orderWarm,
            "Barista",
            "Have you decided what you would like to order?",
            preparingWarm,
            orderSap,
            giveAcceptedDrink,
            orderCola
        );

        WriteOrderMenuDialogue(
            orderStrange,
            "Barista",
            "Have you decided what kind of mistake you would like to drink today?",
            preparingMischievous,
            orderSap,
            giveAcceptedDrink,
            orderCola
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ROAE][BaristaWelcomeFlowAssetUpdater] Flow assets rewritten successfully.");
    }

    private static void WritePreparingDialogue(DialogueData asset, string speaker, string text)
    {
        if (asset == null) return;

        SerializedObject so = new SerializedObject(asset);

        SerializedProperty linesProp = so.FindProperty("dialogueLines");
        linesProp.arraySize = 1;

        SerializedProperty line0 = linesProp.GetArrayElementAtIndex(0);
        line0.FindPropertyRelative("Speaker").stringValue = speaker;
        line0.FindPropertyRelative("Text").stringValue = text;

        SerializedProperty choicesProp = so.FindProperty("choices");
        choicesProp.arraySize = 0;

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void WriteSingleLineDialogue(DialogueData asset, string speaker, string text)
    {
        if (asset == null) return;

        SerializedObject so = new SerializedObject(asset);

        SerializedProperty linesProp = so.FindProperty("dialogueLines");
        linesProp.arraySize = 1;

        SerializedProperty line0 = linesProp.GetArrayElementAtIndex(0);
        line0.FindPropertyRelative("Speaker").stringValue = speaker;
        line0.FindPropertyRelative("Text").stringValue = text;

        SerializedProperty choicesProp = so.FindProperty("choices");
        choicesProp.arraySize = 0;

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void WriteOrderMenuDialogue(
        DialogueData asset,
        string speaker,
        string firstLine,
        DialogueData preparingDialogue,
        BaristaWelcomeChoiceEffect orderSapEffect,
        BaristaWelcomeChoiceEffect giveAcceptedDrinkEffect,
        BaristaWelcomeChoiceEffect orderColaEffect)
    {
        if (asset == null) return;

        SerializedObject so = new SerializedObject(asset);

        SerializedProperty linesProp = so.FindProperty("dialogueLines");
        linesProp.arraySize = 1;

        SerializedProperty line0 = linesProp.GetArrayElementAtIndex(0);
        line0.FindPropertyRelative("Speaker").stringValue = speaker;
        line0.FindPropertyRelative("Text").stringValue = firstLine;

        SerializedProperty choicesProp = so.FindProperty("choices");
        choicesProp.arraySize = 3;

        // Choice 0
        SerializedProperty c0 = choicesProp.GetArrayElementAtIndex(0);
        c0.FindPropertyRelative("choiceText").stringValue = "Not yet.";
        c0.FindPropertyRelative("nextDialogue").objectReferenceValue = null;
        SafeClearObjectRef(c0, "statEffect");
        SafeClearObjectRef(c0, "relationshipEffect");
        SetExtraEffects(c0, null);

        // Choice 1
        SerializedProperty c1 = choicesProp.GetArrayElementAtIndex(1);
        c1.FindPropertyRelative("choiceText").stringValue = "Alright... give me a photosynthetic drink, please.";
        c1.FindPropertyRelative("nextDialogue").objectReferenceValue = preparingDialogue;
        SafeClearObjectRef(c1, "statEffect");
        SafeClearObjectRef(c1, "relationshipEffect");
        SetExtraEffects(c1, orderSapEffect, giveAcceptedDrinkEffect);

        // Choice 2
        SerializedProperty c2 = choicesProp.GetArrayElementAtIndex(2);
        c2.FindPropertyRelative("choiceText").stringValue = "Do you have cola?";
        c2.FindPropertyRelative("nextDialogue").objectReferenceValue = null;
        SafeClearObjectRef(c2, "statEffect");
        SafeClearObjectRef(c2, "relationshipEffect");
        SetExtraEffects(c2, orderColaEffect);

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void SetExtraEffects(SerializedProperty choiceProp, params DialogueChoiceEffect[] effects)
    {
        SerializedProperty extraEffectsProp = choiceProp.FindPropertyRelative("extraEffects");
        if (extraEffectsProp == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeFlowAssetUpdater] Missing extraEffects property.");
            return;
        }

        int count = effects != null ? effects.Length : 0;
        extraEffectsProp.arraySize = count;

        for (int i = 0; i < count; i++)
        {
            SerializedProperty element = extraEffectsProp.GetArrayElementAtIndex(i);

            if (element.propertyType == SerializedPropertyType.ObjectReference)
            {
                element.objectReferenceValue = effects[i];
            }
            else
            {
                Debug.LogWarning(
                    "[ROAE][BaristaWelcomeFlowAssetUpdater] extraEffects element is not an ObjectReference. " +
                    "Property type = " + element.propertyType);
            }
        }
    }

    private static void SafeClearObjectRef(SerializedProperty parent, string childName)
    {
        SerializedProperty child = parent.FindPropertyRelative(childName);
        if (child == null)
            return;

        if (child.propertyType != SerializedPropertyType.ObjectReference)
            return;

        child.objectReferenceValue = null;
    }

    private static DialogueData LoadDialogue(string assetName)
    {
        string path = DialoguesFolder + "/" + assetName + ".asset";
        DialogueData asset = AssetDatabase.LoadAssetAtPath<DialogueData>(path);

        if (asset == null)
            Debug.LogError("[ROAE][BaristaWelcomeFlowAssetUpdater] Missing dialogue asset: " + path);

        return asset;
    }

    private static BaristaWelcomeChoiceEffect GetOrCreateChoiceEffect(
        string assetName,
        BaristaWelcomeChoiceCommand command)
    {
        string path = EffectsFolder + "/" + assetName + ".asset";
        BaristaWelcomeChoiceEffect asset = AssetDatabase.LoadAssetAtPath<BaristaWelcomeChoiceEffect>(path);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<BaristaWelcomeChoiceEffect>();
            AssetDatabase.CreateAsset(asset, path);
        }

        SerializedObject so = new SerializedObject(asset);

        SerializedProperty commandProp = so.FindProperty("command");
        SerializedProperty debugLogProp = so.FindProperty("debugLog");

        if (commandProp != null)
            commandProp.enumValueIndex = (int)command;

        if (debugLogProp != null)
            debugLogProp.boolValue = true;

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);

        return asset;
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