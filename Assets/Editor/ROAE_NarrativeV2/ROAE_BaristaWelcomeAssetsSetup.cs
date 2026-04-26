using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ROAE_BaristaWelcomeAssetsSetup
{
    private const string Root = "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome";
    private const string EffectsDir = Root + "/Effects";
    private const string DialoguesDir = Root + "/Dialogues";
    private const string ConfigsDir = Root + "/Configs";

    [MenuItem("Tools/ROAE/Barista Welcome/Create Assets And Auto-Wire Selected")]
    public static void CreateAssetsAndAutoWireSelected()
    {
        EnsureFolder("Assets/ROAE_2/Data");
        EnsureFolder("Assets/ROAE_2/Data/NarrativeV2");
        EnsureFolder(Root);
        EnsureFolder(EffectsDir);
        EnsureFolder(DialoguesDir);
        EnsureFolder(ConfigsDir);

        var effectMarkIntroDone = CreateChoiceEffect("BW_MarkIntroDone", "MarkIntroDone");
        var effectApplyNaive = CreateChoiceEffect("BW_ApplyNaiveResponse", "ApplyNaiveResponse");
        var effectApplyGuarded = CreateChoiceEffect("BW_ApplyGuardedResponse", "ApplyGuardedResponse");
        var effectOrderCola = CreateChoiceEffect("BW_OrderCola", "OrderCola");
        var effectOrderSap = CreateChoiceEffect("BW_OrderPhotosyntheticSap", "OrderPhotosyntheticSap");
        var effectDrinkHeld = CreateChoiceEffect("BW_DrinkHeldDrink", "DrinkHeldDrink");

        var bwDrinkPreparing = CreateDialogue(
            "BW_Drink_Preparing",
            new[]
            {
                Line("Barista", "It'll only take a moment. Have a seat.")
            },
            null
        );

        var bwAlreadyHasDrink = CreateDialogue(
            "BW_Already_Has_Drink",
            new[]
            {
                Line("Rina", "I already have a drink.")
            },
            null
        );

        var bwOrderMenu = CreateDialogue(
            "BW_Order_Menu",
            new[]
            {
                Line("Barista", "Have you decided what you'd like to order?")
            },
            new[]
            {
                Choice("Not yet.", null, null, null, null),
                Choice("Alright... give me a photosynthetic drink, please.", null, null, null, new UnityEngine.Object[] { effectOrderSap }),
                Choice("Do you have cola?", null, null, null, new UnityEngine.Object[] { effectOrderCola })
            }
        );

        var bwDrinkHeld = CreateDialogue(
            "BW_Drink_Held",
            new[]
            {
                Line("Rina", "Alright... let's drink this.")
            },
            new[]
            {
                Choice("Drink it.", null, null, null, new UnityEngine.Object[] { effectDrinkHeld })
            }
        );

        var bwIntroNeutral = CreateDialogue(
            "BW_Intro_Neutral",
            new[]
            {
                Line("Barista", "Hello there. What can I get for you today?"),
                Line("Rina", "Hi... um... it's my first time here. Do you have a menu?"),
                Line("Barista", "Besides the usual brews, I received a fresh batch of photosynthetic sap first thing this morning. Shall I prepare you a cup?"),
                Line("Rina", "But... I don't have anything to pay you with."),
                Line("Barista", "Hah, what a funny thing to say this morning, miss."),
                Line("Rina", "Excuse me?"),
                Line("Barista", "One laaaarge cup of photosynthetic sap, coming right up. Have a seat.")
            },
            new[]
            {
                Choice("Okay... thank you.", bwDrinkPreparing, null, null, new UnityEngine.Object[] { effectApplyNaive, effectMarkIntroDone }),
                Choice("No, it's alright. I think I'll decide later if I want anything.", null, null, null, new UnityEngine.Object[] { effectApplyGuarded, effectMarkIntroDone })
            }
        );

        var bwIntroMischievous = CreateDialogue(
            "BW_Intro_Mischievous",
            new[]
            {
                Line("Barista", "Hello there. What can I get for you today?"),
                Line("Rina", "Hi... um... it's my first time here. Do you have a menu?"),
                Line("Barista", "Really? I could have sworn this wasn't the first time you've seen me."),
                Line("Barista", "I mean... the first time I've seen you."),
                Line("Rina", "..."),
                Line("Rina", "I don't have anything to pay you with."),
                Line("Barista", "You're funny. I like you already."),
                Line("Rina", "..."),
                Line("Barista", "Then let me prepare you a photosynthetic sap. Something to make sure you come back a second time, alright?")
            },
            new[]
            {
                Choice("Sure.", bwDrinkPreparing, null, null, new UnityEngine.Object[] { effectApplyNaive, effectMarkIntroDone }),
                Choice("It's okay. I need a little more time to think.", null, null, null, new UnityEngine.Object[] { effectApplyGuarded, effectMarkIntroDone })
            }
        );

        CreateConfigAsset("BaristaWelcomeConfig_VI", "ValueIteration");
        CreateConfigAsset("BaristaWelcomeConfig_PI", "PolicyIteration");

        AutoWireSelected(
            bwIntroNeutral,
            bwIntroMischievous,
            bwDrinkPreparing,
            bwOrderMenu,
            bwAlreadyHasDrink
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ROAE][BaristaWelcome] Assets created. Dialogues: " +
                  bwIntroNeutral.name + ", " +
                  bwIntroMischievous.name + ", " +
                  bwDrinkPreparing.name + ", " +
                  bwAlreadyHasDrink.name + ", " +
                  bwOrderMenu.name + ", " +
                  bwDrinkHeld.name);
    }

    [MenuItem("Tools/ROAE/Barista Welcome/Auto-Wire Selected Only")]
    public static void AutoWireSelectedOnly()
    {
        var neutral = LoadDialogue("BW_Intro_Neutral");
        var mischievous = LoadDialogue("BW_Intro_Mischievous");
        var preparing = LoadDialogue("BW_Drink_Preparing");
        var orderMenu = LoadDialogue("BW_Order_Menu");
        var alreadyHasDrink = LoadDialogue("BW_Already_Has_Drink");

        AutoWireSelected(neutral, mischievous, preparing, orderMenu, alreadyHasDrink);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void AutoWireSelected(
        DialogueData neutral,
        DialogueData mischievous,
        DialogueData preparing,
        DialogueData orderMenu,
        DialogueData alreadyHasDrink)
    {
        if (Selection.activeGameObject == null)
        {
            Debug.Log("[ROAE][BaristaWelcome] No selected GameObject. Assets were created, but nothing was auto-wired.");
            return;
        }

        var go = Selection.activeGameObject;
        var trigger = go.GetComponent("BaristaDialogueTrigger") as MonoBehaviour;

        if (trigger == null)
        {
            Debug.Log("[ROAE][BaristaWelcome] Selected object has no BaristaDialogueTrigger. Assets were created, but nothing was auto-wired.");
            return;
        }

        var so = new SerializedObject(trigger);

        TryAssignObject(so, "neutralIntroDialogue", neutral);
        TryAssignObject(so, "mischievousIntroDialogue", mischievous);
        TryAssignObject(so, "drinkPreparingDialogue", preparing);
        TryAssignObject(so, "orderMenuDialogue", orderMenu);
        TryAssignObject(so, "alreadyHasDrinkDialogue", alreadyHasDrink);

        if (TryAssignComponentByTypeName(go, so, "dialogueManager", "DialogueManager"))
        {
            Debug.Log("[ROAE][BaristaWelcome] DialogueManager auto-assigned from selected object hierarchy.");
        }

        if (TryAssignComponentByTypeName(go, so, "brain", "BaristaWelcomeBrain"))
        {
            Debug.Log("[ROAE][BaristaWelcome] Brain auto-assigned from selected object hierarchy.");
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(trigger);

        Debug.Log("[ROAE][BaristaWelcome] Auto-wired selected object: " + go.name);
    }

    private static bool TryAssignComponentByTypeName(GameObject go, SerializedObject so, string fieldName, string typeName)
    {
        var prop = so.FindProperty(fieldName);
        if (prop == null || prop.propertyType != SerializedPropertyType.ObjectReference)
            return false;

        if (prop.objectReferenceValue != null)
            return true;

        Component found = FindComponentInSelfParentOrChildren(go, typeName);
        if (found == null)
            return false;

        prop.objectReferenceValue = found;
        return true;
    }

    private static Component FindComponentInSelfParentOrChildren(GameObject go, string typeName)
    {
        Component[] self = go.GetComponents<Component>();
        foreach (var c in self)
        {
            if (c != null && c.GetType().Name == typeName)
                return c;
        }

        Transform parent = go.transform.parent;
        while (parent != null)
        {
            Component[] comps = parent.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c != null && c.GetType().Name == typeName)
                    return c;
            }
            parent = parent.parent;
        }

        Component[] children = go.GetComponentsInChildren<Component>(true);
        foreach (var c in children)
        {
            if (c != null && c.GetType().Name == typeName)
                return c;
        }

        return null;
    }

    private static UnityEngine.Object CreateChoiceEffect(string assetName, string commandName)
    {
        string path = EffectsDir + "/" + assetName + ".asset";
        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance("BaristaWelcomeChoiceEffect") as ScriptableObject;
            if (asset == null)
                throw new Exception("Could not create BaristaWelcomeChoiceEffect. Make sure the script compiles.");
            AssetDatabase.CreateAsset(asset, path);
        }

        var so = new SerializedObject(asset);
        SetEnumByName(so, "command", commandName);
        SetBoolIfExists(so, "debugLog", true);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static void CreateConfigAsset(string assetName, string plannerModeName)
    {
        string path = ConfigsDir + "/" + assetName + ".asset";
        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance("BaristaWelcomeConfig") as ScriptableObject;
            if (asset == null)
                throw new Exception("Could not create BaristaWelcomeConfig. Make sure the script compiles.");
            AssetDatabase.CreateAsset(asset, path);
        }

        var so = new SerializedObject(asset);
        SetEnumByName(so, "plannerMode", plannerModeName);
        SetFloatIfExists(so, "gamma", 0.85f);
        SetFloatIfExists(so, "epsilon", 0.0001f);
        SetIntIfExists(so, "maxIterations", 50);
        SetIntIfExists(so, "corruptionThresholdForMischief", 2);
        SetFloatIfExists(so, "rewardNeutralBase", 2f);
        SetFloatIfExists(so, "rewardNeutralIfLowCorruption", 2f);
        SetFloatIfExists(so, "rewardMischievousBase", 1f);
        SetFloatIfExists(so, "rewardMischievousIfReadUnknownText", 4f);
        SetFloatIfExists(so, "rewardMischievousIfCorruptionHigh", 3f);
        SetFloatIfExists(so, "selfLoopProbability", 0.70f);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static DialogueData CreateDialogue(string assetName, BaristaLine[] lines, BaristaChoiceSpec[] choices)
    {
        string path = GetDialoguePath(assetName);
        EnsureFolder(Path.GetDirectoryName(path).Replace("\\", "/"));
        var dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
        if (dialogue == null)
        {
            dialogue = ScriptableObject.CreateInstance<DialogueData>();
            AssetDatabase.CreateAsset(dialogue, path);
        }

        var so = new SerializedObject(dialogue);

        var linesProp = so.FindProperty("dialogueLines");
        if (linesProp != null)
        {
            linesProp.arraySize = lines != null ? lines.Length : 0;
            for (int i = 0; i < linesProp.arraySize; i++)
            {
                var elem = linesProp.GetArrayElementAtIndex(i);
                SetStringOnChild(elem, "Speaker", lines[i].speaker);
                SetStringOnChild(elem, "Text", lines[i].text);
            }
        }

        var choicesProp = so.FindProperty("choices");
        if (choicesProp != null)
        {
            choicesProp.arraySize = choices != null ? choices.Length : 0;
            for (int i = 0; i < choicesProp.arraySize; i++)
            {
                var elem = choicesProp.GetArrayElementAtIndex(i);
                SetStringOnChild(elem, "choiceText", choices[i].choiceText);
                SetObjectOnChild(elem, "nextDialogue", choices[i].nextDialogue);
                SetObjectOnChild(elem, "statEffect", choices[i].statEffect);
                SetObjectOnChild(elem, "relationshipEffect", choices[i].relationshipEffect);

                var extraEffects = elem.FindPropertyRelative("extraEffects");
                if (extraEffects != null)
                {
                    extraEffects.arraySize = choices[i].extraEffects != null ? choices[i].extraEffects.Length : 0;
                    for (int j = 0; j < extraEffects.arraySize; j++)
                    {
                        var effectElem = extraEffects.GetArrayElementAtIndex(j);
                        effectElem.objectReferenceValue = choices[i].extraEffects[j];
                    }
                }
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(dialogue);
        return dialogue;
    }

    private static DialogueData LoadDialogue(string assetName)
    {
        string path = GetDialoguePath(assetName);
        var dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
        if (dialogue != null)
            return dialogue;

        string[] guids = AssetDatabase.FindAssets(assetName + " t:DialogueData", new[] { DialoguesDir });
        for (int i = 0; i < guids.Length; i++)
        {
            string candidatePath = AssetDatabase.GUIDToAssetPath(guids[i]);
            var candidate = AssetDatabase.LoadAssetAtPath<DialogueData>(candidatePath);
            if (candidate != null && candidate.name == assetName)
                return candidate;
        }

        return null;
    }

    private static string GetDialoguePath(string assetName)
    {
        string subfolder = GetDialogueSubfolder(assetName);
        return string.IsNullOrEmpty(subfolder)
            ? DialoguesDir + "/" + assetName + ".asset"
            : DialoguesDir + "/" + subfolder + "/" + assetName + ".asset";
    }

    private static string GetDialogueSubfolder(string assetName)
    {
        if (assetName.StartsWith("BW_Intro_", StringComparison.Ordinal))
            return "Welcome/Intro";
        if (assetName.StartsWith("BW_Order_Menu_", StringComparison.Ordinal))
            return "Welcome/Order";
        if (assetName.StartsWith("BW_Drink_Preparing_", StringComparison.Ordinal))
            return "Welcome/Preparing";
        if (assetName.StartsWith("BW_Drink_Reminder_", StringComparison.Ordinal))
            return "Welcome/Reminder";
        if (assetName.StartsWith("BW_Already_Has_Drink_", StringComparison.Ordinal) || assetName == "BW_Drink_Held")
            return "Welcome/HeldDrink";
        if (assetName == "BW_Cola")
            return "Welcome/Replies";
        if (assetName == "BW_Drink_Preparing")
            return "LegacyGenerated/Preparing";
        if (assetName == "BW_Already_Has_Drink")
            return "LegacyGenerated/HeldDrink";
        if (assetName == "BW_Order_Menu")
            return "LegacyGenerated/Order";
        return string.Empty;
    }

    private static BaristaLine Line(string speaker, string text)
    {
        return new BaristaLine { speaker = speaker, text = text };
    }

    private static BaristaChoiceSpec Choice(string choiceText, DialogueData nextDialogue, UnityEngine.Object statEffect, UnityEngine.Object relationshipEffect, UnityEngine.Object[] extraEffects)
    {
        return new BaristaChoiceSpec
        {
            choiceText = choiceText,
            nextDialogue = nextDialogue,
            statEffect = statEffect,
            relationshipEffect = relationshipEffect,
            extraEffects = extraEffects
        };
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string folder = Path.GetFileName(path);

        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, folder);
    }

    private static void TryAssignObject(SerializedObject so, string propertyName, UnityEngine.Object value)
    {
        var prop = so.FindProperty(propertyName);
        if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
            prop.objectReferenceValue = value;
    }

    private static void SetStringOnChild(SerializedProperty parent, string childName, string value)
    {
        var prop = parent.FindPropertyRelative(childName);
        if (prop != null && prop.propertyType == SerializedPropertyType.String)
            prop.stringValue = value;
    }

    private static void SetObjectOnChild(SerializedProperty parent, string childName, UnityEngine.Object value)
    {
        var prop = parent.FindPropertyRelative(childName);
        if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
            prop.objectReferenceValue = value;
    }

    private static void SetEnumByName(SerializedObject so, string propertyName, string enumName)
    {
        var prop = so.FindProperty(propertyName);
        if (prop == null || prop.propertyType != SerializedPropertyType.Enum)
            return;

        int index = Array.IndexOf(prop.enumNames, enumName);
        if (index >= 0)
            prop.enumValueIndex = index;
    }

    private static void SetBoolIfExists(SerializedObject so, string propertyName, bool value)
    {
        var prop = so.FindProperty(propertyName);
        if (prop != null && prop.propertyType == SerializedPropertyType.Boolean)
            prop.boolValue = value;
    }

    private static void SetFloatIfExists(SerializedObject so, string propertyName, float value)
    {
        var prop = so.FindProperty(propertyName);
        if (prop != null && prop.propertyType == SerializedPropertyType.Float)
            prop.floatValue = value;
    }

    private static void SetIntIfExists(SerializedObject so, string propertyName, int value)
    {
        var prop = so.FindProperty(propertyName);
        if (prop != null && prop.propertyType == SerializedPropertyType.Integer)
            prop.intValue = value;
    }

    private struct BaristaLine
    {
        public string speaker;
        public string text;
    }

    private struct BaristaChoiceSpec
    {
        public string choiceText;
        public DialogueData nextDialogue;
        public UnityEngine.Object statEffect;
        public UnityEngine.Object relationshipEffect;
        public UnityEngine.Object[] extraEffects;
    }
}
