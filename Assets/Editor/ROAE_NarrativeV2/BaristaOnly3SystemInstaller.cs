#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ROAE.EditorTools
{
    public static class BaristaOnly3SystemInstaller
    {
        private const string LogPrefix = "[ROAE][BaristaOnly3Installer] ";

        private const string DataRoot = "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome";
        private const string ConfigsRoot = DataRoot + "/Configs";
        private const string DialoguesRoot = DataRoot + "/Dialogues";
        private const string EffectsRoot = DataRoot + "/Effects";
        private const string BackupRoot = "Assets/ROAE_2/_GeneratedBackups";

        private static readonly string[] GeneratedDialogueNames =
        {
            "BW_Intro_Neutral",
            "BW_Intro_Warm",
            "BW_Intro_Mischievous",
            "BW_Order_Menu_Clean",
            "BW_Order_Menu_Warm",
            "BW_Order_Menu_Strange",
            "BW_Drink_Preparing_Neutral",
            "BW_Drink_Preparing_Warm",
            "BW_Drink_Preparing_Mischievous",
            "BW_Already_Has_Drink_Cola",
            "BW_Already_Has_Drink_Sap"
        };

        private static readonly string[] GeneratedEffectNames =
        {
            "BW_MarkIntroDone",
            "BW_OrderCola",
            "BW_OrderPhotosyntheticSap",
            "BW_ApplyNeutralResponse",
            "BW_ApplyWarmResponse",
            "BW_ApplyMischievousResponse",
            "BW_ClearHeldDrink"
        };

        [MenuItem("Tools/ROAE/Barista/Install Robust Only3 System")]
        public static void InstallRobustOnly3System()
        {
            try
            {
                Debug.Log(LogPrefix + "Install started.");
                EnsureFolders();
                BackupCriticalAssets();
                DeletePreviousGeneratedAssets();
                CreateDialogues();
                CreateEffects();
                CreateConfig();
                PatchRuntimeObjectsInOpenScenes();
                WriteReadme();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log(LogPrefix + "Install completed.");
            }
            catch (Exception ex)
            {
                Debug.LogError(LogPrefix + "Install failed: " + ex);
                throw;
            }
        }

        [MenuItem("Tools/ROAE/Barista/Clean Generated Only3 Assets")]
        public static void CleanGeneratedOnly3Assets()
        {
            EnsureFolders();
            DeletePreviousGeneratedAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(LogPrefix + "Generated assets removed.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/ROAE_2");
            EnsureFolder("Assets/ROAE_2/Data");
            EnsureFolder("Assets/ROAE_2/Data/NarrativeV2");
            EnsureFolder(DataRoot);
            EnsureFolder(ConfigsRoot);
            EnsureFolder(DialoguesRoot);
            EnsureFolder(EffectsRoot);
            EnsureFolder("Assets/ROAE_2/_GeneratedBackups");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
            Debug.Log(LogPrefix + "Created folder: " + path);
        }

        private static void BackupCriticalAssets()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var targetFolder = BackupRoot + "/BaristaOnly3Install_" + timestamp;
            EnsureFolder(targetFolder);

            var candidates = new[]
            {
                "Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome/AI/BaristaWelcomeBrain.cs",
                "Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome/AI/BaristaOutcomeResolver.cs",
                "Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome/AI/BaristaIntroPlanningSolvers.cs",
                "Assets/ROAE_2/Scripts/Bar_Inside/barista/BaristaDialogueTrigger.cs",
                "Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome/Runtime/BaristaWelcomeDebugMenu.cs",
                "Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome/Runtime/BaristaWelcomeChoiceEffect.cs",
                "Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome/Core/BaristaWelcomeState.cs"
            };

            foreach (var assetPath in candidates)
            {
                if (!File.Exists(assetPath))
                {
                    continue;
                }

                var fileName = assetPath.Replace("Assets/", string.Empty).Replace('/', '_');
                var backupPath = targetFolder + "/" + fileName + ".bak";
                FileUtil.CopyFileOrDirectory(assetPath, backupPath);
                var meta = assetPath + ".meta";
                if (File.Exists(meta))
                {
                    FileUtil.CopyFileOrDirectory(meta, backupPath + ".meta");
                }

                Debug.Log(LogPrefix + "Backed up: " + assetPath);
            }
        }

        private static void DeletePreviousGeneratedAssets()
        {
            foreach (var name in GeneratedDialogueNames)
            {
                DeleteIfExists(GetDialoguePath(name));
            }

            foreach (var name in GeneratedEffectNames)
            {
                DeleteIfExists(EffectsRoot + "/" + name + ".asset");
            }

            DeleteIfExists(ConfigsRoot + "/BaristaWelcomeConfig_Only3.asset");
            DeleteIfExists(DataRoot + "/README_BARISTA_ONLY3_SETUP.txt");
        }

        private static void DeleteIfExists(string path)
        {
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (obj == null && !File.Exists(path))
            {
                return;
            }

            AssetDatabase.DeleteAsset(path);
            Debug.Log(LogPrefix + "Deleted: " + path);
        }

        private static void CreateDialogues()
        {
            var dialogueType = FindTypeByName("DialogueData");
            if (dialogueType == null)
            {
                throw new InvalidOperationException("DialogueData type not found. Installer cannot generate dialogue assets.");
            }

            CreateDialogueAsset(dialogueType,
                "BW_Intro_Neutral",
                new[]
                {
                    "You look like you got lost on purpose.",
                    "If you want a drink, say so plainly. I respect plain language."
                },
                new[]
                {
                    CreateChoiceSpec("I need something ordinary.", null, "BW_ApplyNeutralResponse", 0, 0, 0),
                    CreateChoiceSpec("I am not sure ordinary will do.", null, "BW_ApplyMischievousResponse", 1, -1, 0),
                    CreateChoiceSpec("You seem kind. Surprise me.", null, "BW_ApplyWarmResponse", 0, 1, 0)
                },
                "BW_MarkIntroDone");

            CreateDialogueAsset(dialogueType,
                "BW_Intro_Warm",
                new[]
                {
                    "You seem tired in the way artists get tired.",
                    "Sit with the feeling for a second. Then tell me what kind of comfort you want."
                },
                new[]
                {
                    CreateChoiceSpec("Something gentle, please.", null, "BW_ApplyWarmResponse", 0, 1, 0),
                    CreateChoiceSpec("Maybe gentle is overrated.", null, "BW_ApplyMischievousResponse", 1, -1, 0),
                    CreateChoiceSpec("Just show me the menu.", null, "BW_ApplyNeutralResponse", 0, 0, 0)
                },
                "BW_MarkIntroDone");

            CreateDialogueAsset(dialogueType,
                "BW_Intro_Mischievous",
                new[]
                {
                    "You have the face of someone about to ask for trouble in a glass.",
                    "Good. Trouble at least has flavor."
                },
                new[]
                {
                    CreateChoiceSpec("Then give me your strangest thing.", null, "BW_ApplyMischievousResponse", 1, -1, 0),
                    CreateChoiceSpec("Actually, maybe something safe.", null, "BW_ApplyNeutralResponse", 0, 0, 0),
                    CreateChoiceSpec("Only if you promise not to mock me.", null, "BW_ApplyWarmResponse", 0, 1, 0)
                },
                "BW_MarkIntroDone");

            CreateDialogueAsset(dialogueType,
                "BW_Order_Menu_Clean",
                new[]
                {
                    "Today I can offer cola, or something less responsible.",
                    "Choose carefully, or at least choose honestly."
                },
                new[]
                {
                    CreateChoiceSpec("Cola.", null, "BW_OrderCola", 0, 0, 0),
                    CreateChoiceSpec("The weird potion.", "BW_Drink_Preparing_Neutral", "BW_OrderPhotosyntheticSap", 1, 0, 0),
                    CreateChoiceSpec("Talk to me a little more first.", null, "BW_ApplyNeutralResponse", 0, 0, 0)
                },
                null);

            CreateDialogueAsset(dialogueType,
                "BW_Order_Menu_Warm",
                new[]
                {
                    "Cola if you need something familiar.",
                    "Or the green one, if you trust me enough to be brave for five minutes."
                },
                new[]
                {
                    CreateChoiceSpec("I will take the cola.", null, "BW_OrderCola", 0, 0, 0),
                    CreateChoiceSpec("I trust you. The green one.", "BW_Drink_Preparing_Warm", "BW_OrderPhotosyntheticSap", 1, 1, 0),
                    CreateChoiceSpec("You make that sound suspiciously tender.", null, "BW_ApplyWarmResponse", 0, 1, 0)
                },
                null);

            CreateDialogueAsset(dialogueType,
                "BW_Order_Menu_Strange",
                new[]
                {
                    "Cola is the safe lie.",
                    "The other drink is the interesting mistake."
                },
                new[]
                {
                    CreateChoiceSpec("Fine. Cola.", null, "BW_OrderCola", 0, 0, 0),
                    CreateChoiceSpec("Interesting mistake, please.", "BW_Drink_Preparing_Mischievous", "BW_OrderPhotosyntheticSap", 1, -1, 0),
                    CreateChoiceSpec("You say that like a dare.", null, "BW_ApplyMischievousResponse", 1, -1, 0)
                },
                null);

            CreateDialogueAsset(dialogueType,
                "BW_Drink_Preparing_Neutral",
                new[]
                {
                    "That one takes a little time.",
                    "Take a seat. I will bring it when it is ready."
                },
                Array.Empty<ChoiceSpec>(),
                null);

            CreateDialogueAsset(dialogueType,
                "BW_Drink_Preparing_Warm",
                new[]
                {
                    "That one should not be rushed.",
                    "Take a seat. I will bring it to you when it settles into itself."
                },
                Array.Empty<ChoiceSpec>(),
                null);

            CreateDialogueAsset(dialogueType,
                "BW_Drink_Preparing_Mischievous",
                new[]
                {
                    "Excellent choice. Reckless, but elegant.",
                    "Take a seat. If it starts glowing, that means I did it right."
                },
                Array.Empty<ChoiceSpec>(),
                null);

            CreateDialogueAsset(dialogueType,
                "BW_Already_Has_Drink_Cola",
                new[]
                {
                    "You already have a cola.",
                    "Finish that one before you start collecting identities in cups."
                },
                Array.Empty<ChoiceSpec>(),
                null);

            CreateDialogueAsset(dialogueType,
                "BW_Already_Has_Drink_Sap",
                new[]
                {
                    "You are already holding the strange one.",
                    "Go sit with your decision before you come back for another."
                },
                Array.Empty<ChoiceSpec>(),
                null);
        }

        private static void CreateEffects()
        {
            var effectType = FindTypeByName("BaristaWelcomeChoiceEffect");
            if (effectType == null)
            {
                throw new InvalidOperationException("BaristaWelcomeChoiceEffect type not found. Installer cannot generate effect assets.");
            }

            CreateChoiceEffect(effectType, "BW_MarkIntroDone", "MarkIntroDone", "Neutral", 0, 0, 0);
            CreateChoiceEffect(effectType, "BW_OrderCola", "OrderCola", "Neutral", 0, 0, 0);
            CreateChoiceEffect(effectType, "BW_OrderPhotosyntheticSap", "OrderPhotosyntheticSap", "Neutral", 0, 0, 0);
            CreateChoiceEffect(effectType, "BW_ApplyNeutralResponse", "ApplyNeutralResponse", "Neutral", 0, 0, 0);
            CreateChoiceEffect(effectType, "BW_ApplyWarmResponse", "ApplyWarmResponse", "Warm", 0, 1, 0);
            CreateChoiceEffect(effectType, "BW_ApplyMischievousResponse", "ApplyMischievousResponse", "Mischievous", 1, -1, 0);
            CreateChoiceEffect(effectType, "BW_ClearHeldDrink", "ClearHeldDrink", "Neutral", 0, 0, 0);
        }

        private static void CreateConfig()
        {
            var configType = FindTypeByName("BaristaWelcomeConfig");
            if (configType == null)
            {
                throw new InvalidOperationException("BaristaWelcomeConfig type not found.");
            }

            var config = ScriptableObject.CreateInstance(configType);
            SetString(config, new[] { "modeName", "configName", "label" }, "Only3");
            SetEnumByName(config, new[] { "plannerMode", "mode", "planner" }, "ValueIteration");
            SetBool(config, new[] { "useOnlyThreeTones", "onlyThreeTones", "strictOnly3" }, true);
            SetFloat(config, new[] { "gamma", "discountFactor" }, 0.88f);
            SetInt(config, new[] { "maxIterations", "iterations" }, 90);
            SetFloat(config, new[] { "epsilon", "convergenceEpsilon" }, 0.0001f);
            SetStringList(config, new[] { "allowedTones", "toneNames", "supportedTones" }, new[] { "Neutral", "Warm", "Mischievous" });

            var configPath = ConfigsRoot + "/BaristaWelcomeConfig_Only3.asset";
            AssetDatabase.CreateAsset(config, configPath);
            Debug.Log(LogPrefix + "Created config: " + configPath);
        }

        private static void PatchRuntimeObjectsInOpenScenes()
        {
            var config = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(ConfigsRoot + "/BaristaWelcomeConfig_Only3.asset");
            if (config == null)
            {
                Debug.LogWarning(LogPrefix + "Config asset missing while patching runtime objects.");
                return;
            }

            PatchAllObjectsNamedType("BaristaWelcomeBrain", obj =>
            {
                SetObjectReference(obj, new[] { "config", "welcomeConfig", "defaultConfig" }, config);
                SetEnumByName(obj, new[] { "plannerMode", "planner", "mode" }, "ValueIteration");
                SetBool(obj, new[] { "logDecisions", "verboseLogs", "enableVerboseLogs" }, true);
                SetBool(obj, new[] { "cacheDecisions", "cacheSolverResults", "cacheResults" }, true);
                SetStringList(obj, new[] { "allowedTones", "toneNames" }, new[] { "Neutral", "Warm", "Mischievous" });
            });

            PatchAllObjectsNamedType("BaristaDialogueTrigger", obj =>
            {
                SetBool(obj, new[] { "reuseResolvedTone", "reuseTone", "persistTonePerMoment" }, true);
                SetBool(obj, new[] { "debugLogs", "logDecisions", "verboseLogs" }, true);
                SetObjectReference(obj, new[] { "brain", "welcomeBrain" }, FindSceneObjectOfTypeByName("BaristaWelcomeBrain"));
                SetObjectReference(obj, new[] { "introNeutralDialogue" }, LoadDialogue("BW_Intro_Neutral"));
                SetObjectReference(obj, new[] { "introWarmDialogue" }, LoadDialogue("BW_Intro_Warm"));
                SetObjectReference(obj, new[] { "introMischievousDialogue" }, LoadDialogue("BW_Intro_Mischievous"));
                SetObjectReference(obj, new[] { "orderNeutralDialogue", "orderCleanDialogue" }, LoadDialogue("BW_Order_Menu_Clean"));
                SetObjectReference(obj, new[] { "orderWarmDialogue" }, LoadDialogue("BW_Order_Menu_Warm"));
                SetObjectReference(obj, new[] { "orderMischievousDialogue", "orderStrangeDialogue" }, LoadDialogue("BW_Order_Menu_Strange"));
                SetObjectReference(obj, new[] { "drinkPreparingNeutralDialogue" }, LoadDialogue("BW_Drink_Preparing_Neutral"));
                SetObjectReference(obj, new[] { "drinkPreparingWarmDialogue" }, LoadDialogue("BW_Drink_Preparing_Warm"));
                SetObjectReference(obj, new[] { "drinkPreparingMischievousDialogue" }, LoadDialogue("BW_Drink_Preparing_Mischievous"));
                SetObjectReference(obj, new[] { "alreadyHasColaDialogue" }, LoadDialogue("BW_Already_Has_Drink_Cola"));
                SetObjectReference(obj, new[] { "alreadyHasSapDialogue", "alreadyHasDrinkDialogue" }, LoadDialogue("BW_Already_Has_Drink_Sap"));
            });

            PatchAllObjectsNamedType("BaristaWelcomeDebugMenu", obj =>
            {
                SetBool(obj, new[] { "debugLogs", "verboseLogs" }, true);
                SetInt(obj, new[] { "warmCreativity", "warmCreativityValue" }, 40);
                SetInt(obj, new[] { "warmEmpathy", "warmEmpathyValue" }, 0);
                SetInt(obj, new[] { "warmCorruption", "warmCorruptionValue" }, 0);
                SetInt(obj, new[] { "mischievousCreativity", "mischievousCreativityValue" }, 55);
                SetInt(obj, new[] { "mischievousEmpathy", "mischievousEmpathyValue" }, -10);
                SetInt(obj, new[] { "mischievousCorruption", "mischievousCorruptionValue" }, 0);
                SetInt(obj, new[] { "neutralCreativity", "neutralCreativityValue" }, 10);
                SetInt(obj, new[] { "neutralEmpathy", "neutralEmpathyValue" }, 0);
                SetInt(obj, new[] { "neutralCorruption", "neutralCorruptionValue" }, 0);
            });

            EditorUtility.SetDirty(config);
            Debug.Log(LogPrefix + "Open-scene objects patched.");
        }

        private static void PatchAllObjectsNamedType(string typeName, Action<UnityEngine.Object> patch)
        {
            var type = FindTypeByName(typeName);
            if (type == null)
            {
                Debug.LogWarning(LogPrefix + "Type not found while patching: " + typeName);
                return;
            }

            var objects = Resources.FindObjectsOfTypeAll(type);
            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    continue;
                }

                patch(obj);
                EditorUtility.SetDirty(obj);
                Debug.Log(LogPrefix + "Patched object: " + obj.name + " (" + typeName + ")");
            }
        }

        private static UnityEngine.Object FindSceneObjectOfTypeByName(string typeName)
        {
            var type = FindTypeByName(typeName);
            if (type == null)
            {
                return null;
            }

            var objects = Resources.FindObjectsOfTypeAll(type);
            return objects.FirstOrDefault();
        }

        private static UnityEngine.Object LoadDialogue(string assetName)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetDialoguePath(assetName));
            if (asset != null)
            {
                return asset;
            }

            string[] guids = AssetDatabase.FindAssets(assetName + " t:DialogueData", new[] { DialoguesRoot });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var candidate = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (candidate != null && candidate.name == assetName)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static void CreateDialogueAsset(Type dialogueType, string assetName, string[] lines, ChoiceSpec[] choices, string rootEffectAssetName)
        {
            var dialogue = ScriptableObject.CreateInstance(dialogueType);

            SetString(dialogue, new[] { "dialogueId", "id", "key" }, assetName);
            SetString(dialogue, new[] { "title", "displayName", "dialogueName" }, assetName);
            SetString(dialogue, new[] { "speakerName", "speaker", "characterName" }, "Barista");
            SetStringList(dialogue, new[] { "lines", "sentences", "dialogueLines" }, lines);
            SetObjectReference(dialogue, new[] { "rootEffect", "onDialogueStartEffect", "entryEffect" }, LoadEffect(rootEffectAssetName));

            if (choices != null && choices.Length > 0)
            {
                TryAssignChoices(dialogue, choices);
            }

            var path = GetDialoguePath(assetName);
            EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/'));
            AssetDatabase.CreateAsset(dialogue, path);
            Debug.Log(LogPrefix + "Created dialogue: " + path);
        }

        private static string GetDialoguePath(string assetName)
        {
            string subfolder = GetDialogueSubfolder(assetName);
            return string.IsNullOrEmpty(subfolder)
                ? DialoguesRoot + "/" + assetName + ".asset"
                : DialoguesRoot + "/" + subfolder + "/" + assetName + ".asset";
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

        private static UnityEngine.Object LoadEffect(string effectName)
        {
            if (string.IsNullOrEmpty(effectName))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(EffectsRoot + "/" + effectName + ".asset");
        }

        private static void CreateChoiceEffect(Type effectType, string assetName, string actionName, string toneName, int deltaCreativity, int deltaEmpathy, int deltaCorruption)
        {
            var effect = ScriptableObject.CreateInstance(effectType);
            SetString(effect, new[] { "effectName", "nameOverride", "debugName" }, assetName);
            SetEnumByName(effect, new[] { "effectType", "choiceEffectType", "mode", "action" }, actionName);
            SetEnumByName(effect, new[] { "tone", "targetTone", "introTone" }, toneName);
            SetInt(effect, new[] { "deltaCreativity", "creativityDelta" }, deltaCreativity);
            SetInt(effect, new[] { "deltaEmpathy", "empathyDelta" }, deltaEmpathy);
            SetInt(effect, new[] { "deltaCorruption", "corruptionDelta", "plantCorruptionDelta" }, deltaCorruption);
            SetBool(effect, new[] { "logApplication", "debugLogs", "verboseLogs" }, true);

            var path = EffectsRoot + "/" + assetName + ".asset";
            AssetDatabase.CreateAsset(effect, path);
            Debug.Log(LogPrefix + "Created effect: " + path);
        }

        private static void TryAssignChoices(UnityEngine.Object dialogue, ChoiceSpec[] choices)
        {
            var so = new SerializedObject(dialogue);
            var choicesProperty = FindProperty(so, new[] { "choices", "dialogueChoices", "options" });
            if (choicesProperty == null || !choicesProperty.isArray)
            {
                Debug.LogWarning(LogPrefix + "Could not find serialized choices array on dialogue asset: " + dialogue.name);
                return;
            }

            choicesProperty.ClearArray();
            for (var i = 0; i < choices.Length; i++)
            {
                choicesProperty.InsertArrayElementAtIndex(i);
                var element = choicesProperty.GetArrayElementAtIndex(i);
                AssignChoiceSerialized(element, choices[i]);
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignChoiceSerialized(SerializedProperty choiceProperty, ChoiceSpec spec)
        {
            SetChildString(choiceProperty, new[] { "choiceText", "text", "label" }, spec.Text);
            SetChildObjectReference(choiceProperty, new[] { "nextDialogue", "nextDialogueData", "followupDialogue" }, LoadDialogue(spec.NextDialogueAssetName));
            SetChildObjectReference(choiceProperty, new[] { "effect", "choiceEffect", "onChooseEffect" }, LoadEffect(spec.EffectAssetName));
            SetChildInt(choiceProperty, new[] { "deltaCreativity", "creativityDelta" }, spec.DeltaCreativity);
            SetChildInt(choiceProperty, new[] { "deltaEmpathy", "empathyDelta" }, spec.DeltaEmpathy);
            SetChildInt(choiceProperty, new[] { "deltaCorruption", "corruptionDelta" }, spec.DeltaCorruption);
        }

        private static void SetChildString(SerializedProperty parent, string[] names, string value)
        {
            var prop = FindRelativeProperty(parent, names);
            if (prop != null && prop.propertyType == SerializedPropertyType.String)
            {
                prop.stringValue = value;
            }
        }

        private static void SetChildObjectReference(SerializedProperty parent, string[] names, UnityEngine.Object value)
        {
            var prop = FindRelativeProperty(parent, names);
            if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
            {
                prop.objectReferenceValue = value;
            }
        }

        private static void SetChildInt(SerializedProperty parent, string[] names, int value)
        {
            var prop = FindRelativeProperty(parent, names);
            if (prop != null && prop.propertyType == SerializedPropertyType.Integer)
            {
                prop.intValue = value;
            }
        }

        private static SerializedProperty FindProperty(SerializedObject so, IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                var prop = so.FindProperty(name);
                if (prop != null)
                {
                    return prop;
                }
            }

            return null;
        }

        private static SerializedProperty FindRelativeProperty(SerializedProperty parent, IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                var prop = parent.FindPropertyRelative(name);
                if (prop != null)
                {
                    return prop;
                }
            }

            return null;
        }

        private static void WriteReadme()
        {
            var path = DataRoot + "/README_BARISTA_ONLY3_SETUP.txt";
            File.WriteAllText(path,
@"ROAE BARISTA ONLY3 SETUP

This installer generated a strict 3-tone barista setup:
- Neutral
- Warm
- Mischievous

Flow:
1. Intro moment resolves one of the 3 tones.
2. Order moment resolves one of the 3 tones.
3. Weird potion uses a preparing dialogue that asks Rina to take a seat.
4. Cola is immediate and blocks re-order with a dedicated dialogue.
5. Held strange potion also blocks re-order with a dedicated dialogue.

Generated by Tools/ROAE/Barista/Install Robust Only3 System.
");
            AssetDatabase.ImportAsset(path);
            Debug.Log(LogPrefix + "Created README: " + path);
        }

        private static Type FindTypeByName(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type found = null;
                try
                {
                    found = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    found = ex.Types?.FirstOrDefault(t => t != null && t.Name == typeName);
                }

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void SetObjectReference(UnityEngine.Object target, IEnumerable<string> memberNames, UnityEngine.Object value)
        {
            if (target == null || value == null)
            {
                return;
            }

            foreach (var memberName in memberNames)
            {
                if (TrySetMember(target, memberName, value))
                {
                    return;
                }
            }
        }

        private static void SetString(UnityEngine.Object target, IEnumerable<string> memberNames, string value)
        {
            if (target == null)
            {
                return;
            }

            foreach (var memberName in memberNames)
            {
                if (TrySetMember(target, memberName, value))
                {
                    return;
                }
            }
        }

        private static void SetBool(UnityEngine.Object target, IEnumerable<string> memberNames, bool value)
        {
            if (target == null)
            {
                return;
            }

            foreach (var memberName in memberNames)
            {
                if (TrySetMember(target, memberName, value))
                {
                    return;
                }
            }
        }

        private static void SetInt(UnityEngine.Object target, IEnumerable<string> memberNames, int value)
        {
            if (target == null)
            {
                return;
            }

            foreach (var memberName in memberNames)
            {
                if (TrySetMember(target, memberName, value))
                {
                    return;
                }
            }
        }

        private static void SetFloat(UnityEngine.Object target, IEnumerable<string> memberNames, float value)
        {
            if (target == null)
            {
                return;
            }

            foreach (var memberName in memberNames)
            {
                if (TrySetMember(target, memberName, value))
                {
                    return;
                }
            }
        }

        private static void SetEnumByName(UnityEngine.Object target, IEnumerable<string> memberNames, string enumName)
        {
            if (target == null)
            {
                return;
            }

            foreach (var memberName in memberNames)
            {
                var member = FindFieldOrProperty(target.GetType(), memberName);
                if (member == null)
                {
                    continue;
                }

                var memberType = GetMemberType(member);
                if (!memberType.IsEnum)
                {
                    continue;
                }

                try
                {
                    var value = Enum.Parse(memberType, enumName);
                    SetMemberValue(target, member, value);
                    return;
                }
                catch
                {
                }
            }
        }

        private static void SetStringList(UnityEngine.Object target, IEnumerable<string> memberNames, IEnumerable<string> values)
        {
            if (target == null)
            {
                return;
            }

            foreach (var memberName in memberNames)
            {
                var member = FindFieldOrProperty(target.GetType(), memberName);
                if (member == null)
                {
                    continue;
                }

                var memberType = GetMemberType(member);
                try
                {
                    if (memberType == typeof(string[]))
                    {
                        SetMemberValue(target, member, values.ToArray());
                        return;
                    }

                    if (typeof(IList<string>).IsAssignableFrom(memberType))
                    {
                        var list = Activator.CreateInstance(memberType) as IList<string>;
                        if (list != null)
                        {
                            foreach (var value in values)
                            {
                                list.Add(value);
                            }
                            SetMemberValue(target, member, list);
                            return;
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private static bool TrySetMember(UnityEngine.Object target, string memberName, object value)
        {
            var member = FindFieldOrProperty(target.GetType(), memberName);
            if (member == null)
            {
                return false;
            }

            var memberType = GetMemberType(member);
            if (value != null && !memberType.IsAssignableFrom(value.GetType()))
            {
                return false;
            }

            SetMemberValue(target, member, value);
            return true;
        }

        private static MemberInfo FindFieldOrProperty(Type type, string memberName)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var field = type.GetField(memberName, flags);
            if (field != null)
            {
                return field;
            }

            var property = type.GetProperty(memberName, flags);
            if (property != null && property.CanWrite)
            {
                return property;
            }

            return null;
        }

        private static Type GetMemberType(MemberInfo member)
        {
            return member switch
            {
                FieldInfo field => field.FieldType,
                PropertyInfo property => property.PropertyType,
                _ => typeof(object)
            };
        }

        private static void SetMemberValue(UnityEngine.Object target, MemberInfo member, object value)
        {
            switch (member)
            {
                case FieldInfo field:
                    field.SetValue(target, value);
                    break;
                case PropertyInfo property when property.CanWrite:
                    property.SetValue(target, value, null);
                    break;
            }
        }

        private static ChoiceSpec CreateChoiceSpec(string text, string nextDialogueAssetName, string effectAssetName, int deltaCreativity, int deltaEmpathy, int deltaCorruption)
        {
            return new ChoiceSpec
            {
                Text = text,
                NextDialogueAssetName = nextDialogueAssetName,
                EffectAssetName = effectAssetName,
                DeltaCreativity = deltaCreativity,
                DeltaEmpathy = deltaEmpathy,
                DeltaCorruption = deltaCorruption
            };
        }

        private sealed class ChoiceSpec
        {
            public string Text;
            public string NextDialogueAssetName;
            public string EffectAssetName;
            public int DeltaCreativity;
            public int DeltaEmpathy;
            public int DeltaCorruption;
        }
    }
}
#endif
