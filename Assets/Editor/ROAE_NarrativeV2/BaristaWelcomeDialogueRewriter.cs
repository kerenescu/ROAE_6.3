using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System;
using System.IO;

public static class BaristaWelcomeDialogueRewriter
{
    private const string Root = "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome";
    private const string DialoguesFolder = Root + "/Dialogues";
    private const string EffectsFolder = Root + "/Effects";

    [MenuItem("ROAE/Barista Welcome/Rebuild Dialogue Assets")]
    public static void RebuildDialogueAssets()
    {
        EnsureFolder(DialoguesFolder);

        DialogueData introNeutral = GetOrCreateDialogue("BW_Intro_Neutral");
        DialogueData introWarm = GetOrCreateDialogue("BW_Intro_Warm");
        DialogueData introMischievous = GetOrCreateDialogue("BW_Intro_Mischievous");

        DialogueData preparingNeutral = GetOrCreateDialogue("BW_Drink_Preparing_Neutral");
        DialogueData preparingWarm = GetOrCreateDialogue("BW_Drink_Preparing_Warm");
        DialogueData preparingMischievous = GetOrCreateDialogue("BW_Drink_Preparing_Mischievous");

        DialogueData alreadyHasCola = GetOrCreateDialogue("BW_Already_Has_Drink_Cola");
        DialogueData alreadyHasSap = GetOrCreateDialogue("BW_Already_Has_Drink_Sap");

        DialogueData orderClean = GetOrCreateDialogue("BW_Order_Menu_Clean");
        DialogueData orderWarm = GetOrCreateDialogue("BW_Order_Menu_Warm");
        DialogueData orderStrange = GetOrCreateDialogue("BW_Order_Menu_Strange");

        DialogueData drinkHeld = GetOrCreateDialogue("BW_Drink_Held");

        Object applyNaive = LoadAsset("BW_ApplyNaiveResponse");
        Object applyWarm = LoadAsset("BW_ApplyWarmResponse");
        Object applyNeutral = LoadAsset("BW_ApplyNeutralResponse");
        Object applyMischievous = LoadAsset("BW_ApplyMischievousResponse");
        Object applyGuarded = LoadAsset("BW_ApplyGuardedResponse");
        Object markIntroDone = LoadAsset("BW_MarkIntroDone");
        Object orderCola = LoadAsset("BW_OrderCola");
        Object orderSap = LoadAsset("BW_OrderPhotosyntheticSap");
        Object drinkHeldEffect = LoadAsset("BW_DrinkHeldDrink");

        if (applyGuarded == null)
        {
            applyGuarded = applyNeutral;
            Debug.LogWarning("[ROAE][BaristaWelcomeDialogueRewriter] BW_ApplyGuardedResponse was not found. Falling back to BW_ApplyNeutralResponse.");
        }

        WriteDialogue(
            introMischievous,
            new[]
            {
                L("Barista", "Hello there. What can I get for you today?"),
                L("Rina", "Hi... um... it is my first time here. Do you have a menu?"),
                L("Barista", "Really? I could have sworn this was not the first time you had seen me."),
                L("Barista", "I mean... the first time I have seen you."),
                L("Rina", "..."),
                L("Rina", "I do not have anything to pay you with."),
                L("Barista", "You are funny. I like you already."),
                L("Rina", "..."),
                L("Barista", "Then let me prepare you a photosynthetic sap. Something to make sure you come back a second time, alright?")
            },
            new[]
            {
                C(
                    "Sure.",
                    preparingMischievous,
                    null,
                    null,
                    applyNaive,
                    markIntroDone
                ),
                C(
                    "It is okay. I need a little more time to think.",
                    null,
                    null,
                    null,
                    applyGuarded,
                    markIntroDone
                )
            }
        );

        WriteDialogue(
            introNeutral,
            new[]
            {
                L("Barista", "Hello. What can I get for you today?"),
                L("Rina", "Hi... I am still figuring that out."),
                L("Barista", "That is fair. First visits can feel a little disorienting in here."),
                L("Rina", "You noticed?"),
                L("Barista", "I notice most things."),
                L("Rina", "I do not have anything to pay you with."),
                L("Barista", "Then let me start you with something simple. A photosynthetic sap, on the house.")
            },
            new[]
            {
                C(
                    "Alright. Thank you.",
                    preparingNeutral,
                    null,
                    null,
                    applyNeutral,
                    markIntroDone
                ),
                C(
                    "I should think a little longer.",
                    null,
                    null,
                    null,
                    applyGuarded,
                    markIntroDone
                )
            }
        );

        WriteDialogue(
            introWarm,
            new[]
            {
                L("Barista", "Hello. You look like you could use a quiet place for a minute."),
                L("Rina", "Is it that obvious?"),
                L("Barista", "Only if you have been carrying too much alone."),
                L("Rina", "..."),
                L("Rina", "I do not have anything to pay you with."),
                L("Barista", "Then do not. Let me make you a photosynthetic sap. Consider it a welcome.")
            },
            new[]
            {
                C(
                    "Thank you. I would like that.",
                    preparingWarm,
                    null,
                    null,
                    applyWarm,
                    markIntroDone
                ),
                C(
                    "That is kind, but I would rather wait.",
                    null,
                    null,
                    null,
                    applyGuarded,
                    markIntroDone
                )
            }
        );

        WriteDialogue(
            preparingMischievous,
            new[]
            {
                L("Barista", "It will only take a moment. Stay where I can see you.")
            },
            null
        );

        WriteDialogue(
            preparingNeutral,
            new[]
            {
                L("Barista", "It will only take a moment. Have a seat.")
            },
            null
        );

        WriteDialogue(
            preparingWarm,
            new[]
            {
                L("Barista", "Give me just a moment. You can sit down if you want.")
            },
            null
        );

        WriteDialogue(
            alreadyHasCola,
            new[]
            {
                L("Rina", "I already have a drink.")
            },
            null
        );

        WriteDialogue(
            alreadyHasSap,
            new[]
            {
                L("Rina", "I already have a drink.")
            },
            null
        );

        WriteDialogue(
            orderClean,
            new[]
            {
                L("Barista", "Have you decided what you would like to order?")
            },
            new[]
            {
                C("Not yet.", null, null, null),
                C("Alright... give me a photosynthetic drink, please.", null, null, null, orderSap),
                C("Do you have cola?", null, null, null, orderCola)
            }
        );

        WriteDialogue(
            orderWarm,
            new[]
            {
                L("Barista", "Have you decided what you would like, or should I recommend something gentle?")
            },
            new[]
            {
                C("Not yet.", null, null, null),
                C("Alright... give me a photosynthetic drink, please.", null, null, null, orderSap),
                C("Do you have cola?", null, null, null, orderCola)
            }
        );

        WriteDialogue(
            orderStrange,
            new[]
            {
                L("Barista", "Have you decided what kind of mistake you would like to drink today?")
            },
            new[]
            {
                C("Not yet.", null, null, null),
                C("Alright... give me a photosynthetic drink, please.", null, null, null, orderSap),
                C("Do you have cola?", null, null, null, orderCola)
            }
        );

        WriteDialogue(
            drinkHeld,
            new[]
            {
                L("Rina", "Alright... let us drink this.")
            },
            new[]
            {
                C("Drink it.", null, null, null, drinkHeldEffect)
            }
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ROAE][BaristaWelcomeDialogueRewriter] Done.");
    }

    private static void WriteDialogue(DialogueData asset, LineDef[] lines, ChoiceDef[] choices)
    {
        SerializedObject so = new SerializedObject(asset);

        SerializedProperty linesProp = so.FindProperty("dialogueLines");
        linesProp.arraySize = lines != null ? lines.Length : 0;

        for (int i = 0; i < linesProp.arraySize; i++)
        {
            SerializedProperty lineProp = linesProp.GetArrayElementAtIndex(i);
            lineProp.FindPropertyRelative("Speaker").stringValue = lines[i].speaker;
            lineProp.FindPropertyRelative("Text").stringValue = lines[i].text;
        }

        SerializedProperty choicesProp = so.FindProperty("choices");
        choicesProp.arraySize = choices != null ? choices.Length : 0;

        for (int i = 0; i < choicesProp.arraySize; i++)
        {
            SerializedProperty choiceProp = choicesProp.GetArrayElementAtIndex(i);
            ChoiceDef choice = choices[i];

            choiceProp.FindPropertyRelative("choiceText").stringValue = choice.text;
            choiceProp.FindPropertyRelative("nextDialogue").objectReferenceValue = choice.nextDialogue;
            choiceProp.FindPropertyRelative("statEffect").objectReferenceValue = choice.statEffect;
            choiceProp.FindPropertyRelative("relationshipEffect").objectReferenceValue = choice.relationshipEffect;

            SerializedProperty extraEffectsProp = choiceProp.FindPropertyRelative("extraEffects");
            extraEffectsProp.arraySize = choice.extraEffects != null ? choice.extraEffects.Length : 0;

            for (int j = 0; j < extraEffectsProp.arraySize; j++)
                extraEffectsProp.GetArrayElementAtIndex(j).objectReferenceValue = choice.extraEffects[j];
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static DialogueData GetOrCreateDialogue(string assetName)
    {
        string path = GetDialoguePath(assetName);
        EnsureFolder(Path.GetDirectoryName(path).Replace("\\", "/"));
        DialogueData asset = AssetDatabase.LoadAssetAtPath<DialogueData>(path);

        if (asset != null)
            return asset;

        asset = ScriptableObject.CreateInstance<DialogueData>();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        return asset;
    }

    private static string GetDialoguePath(string assetName)
    {
        string subfolder = GetDialogueSubfolder(assetName);
        return string.IsNullOrEmpty(subfolder)
            ? DialoguesFolder + "/" + assetName + ".asset"
            : DialoguesFolder + "/" + subfolder + "/" + assetName + ".asset";
    }

    private static string GetDialogueSubfolder(string assetName)
    {
        if (assetName.StartsWith("BW3_Intro_", StringComparison.Ordinal))
            return "TarotFollowup/Intro";
        if (assetName.StartsWith("BW3_Order_Menu_", StringComparison.Ordinal))
            return "TarotFollowup/Order";
        if (assetName.StartsWith("BW3_Drink_Preparing_", StringComparison.Ordinal))
            return "TarotFollowup/Preparing";
        if (assetName.StartsWith("BW3_Drink_Reminder_", StringComparison.Ordinal))
            return "TarotFollowup/Reminder";
        if (assetName.StartsWith("BW3_Already_Has_Drink_", StringComparison.Ordinal))
            return "TarotFollowup/HeldDrink";
        if (assetName == "BW3_Cola")
            return "TarotFollowup/Replies";
        if (assetName.StartsWith("BW2_Intro_", StringComparison.Ordinal))
            return "SecondVisit/Intro";
        if (assetName.StartsWith("BW2_Order_Menu_", StringComparison.Ordinal))
            return "SecondVisit/Order";
        if (assetName.StartsWith("BW2_Drink_Preparing_", StringComparison.Ordinal))
            return "SecondVisit/Preparing";
        if (assetName.StartsWith("BW2_Drink_Reminder_", StringComparison.Ordinal))
            return "SecondVisit/Reminder";
        if (assetName.StartsWith("BW2_Already_Has_Drink_", StringComparison.Ordinal))
            return "SecondVisit/HeldDrink";
        if (assetName == "BW2_Cola")
            return "SecondVisit/Replies";
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
        return string.Empty;
    }

    private static Object LoadAsset(string assetName)
    {
        string exactPath = EffectsFolder + "/" + assetName + ".asset";
        Object asset = AssetDatabase.LoadMainAssetAtPath(exactPath);

        if (asset != null)
            return asset;

        string[] guids = AssetDatabase.FindAssets(assetName);
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            Object candidate = AssetDatabase.LoadMainAssetAtPath(path);
            if (candidate != null && candidate.name == assetName)
                return candidate;
        }

        Debug.LogWarning("[ROAE][BaristaWelcomeDialogueRewriter] Missing asset: " + assetName);
        return null;
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

    private static LineDef L(string speaker, string text)
    {
        return new LineDef
        {
            speaker = speaker,
            text = text
        };
    }

    private static ChoiceDef C(
        string text,
        DialogueData nextDialogue,
        Object statEffect,
        Object relationshipEffect,
        params Object[] extraEffects)
    {
        return new ChoiceDef
        {
            text = text,
            nextDialogue = nextDialogue,
            statEffect = statEffect,
            relationshipEffect = relationshipEffect,
            extraEffects = extraEffects
        };
    }

    private struct LineDef
    {
        public string speaker;
        public string text;
    }

    private struct ChoiceDef
    {
        public string text;
        public DialogueData nextDialogue;
        public Object statEffect;
        public Object relationshipEffect;
        public Object[] extraEffects;
    }
}
