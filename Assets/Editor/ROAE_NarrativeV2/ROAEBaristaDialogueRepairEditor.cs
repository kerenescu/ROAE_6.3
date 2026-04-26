#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class ROAEBaristaDialogueRepairEditor : EditorWindow
{
    private const string ScriptsRoot = "Assets/ROAE_2/Scripts";
    private const string BrainPath = ScriptsRoot + "/NarrativeV2/BaristaWelcome/AI/BaristaWelcomeBrain.cs";
    private const string BackupRoot = ScriptsRoot + "/_GeneratedBackups";
    private const string DefaultDialogueFolder = "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Dialogue";

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

    [MenuItem("Tools/ROAE/Barista/Open Dialogue Repair Window")]
    public static void OpenWindow()
    {
        var window = GetWindow<ROAEBaristaDialogueRepairEditor>("ROAE Dialogue Repair");
        window.minSize = new Vector2(620f, 300f);
        window.Show();
    }

    [MenuItem("Tools/ROAE/Barista/Repair Live Dialogues + Brain")]
    public static void RepairAllMenu()
    {
        RepairAll();
    }

    private void OnGUI()
    {
        GUILayout.Space(8f);
        EditorGUILayout.LabelField("ROAE Barista Dialogue Repair", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Creates the missing live BW_* dialogue assets expected by BaristaWelcomeOutcomeResolver and rewrites " +
            "BaristaWelcomeBrain so planner logs stay active while final tone comes from the stats-based outcome resolver.",
            MessageType.Info);

        GUILayout.Space(8f);

        if (GUILayout.Button("Repair Live Dialogues + Brain", GUILayout.Height(34f)))
        {
            RepairAll();
        }

        GUILayout.Space(10f);
        EditorGUILayout.LabelField("Touched file", EditorStyles.boldLabel);
        EditorGUILayout.TextField(BrainPath);

        GUILayout.Space(8f);
        EditorGUILayout.LabelField("Assets created or updated", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(
            "BW_Intro_Warm\n" +
            "BW_Intro_Guarded\n" +
            "BW_Intro_Ominous\n" +
            "BW_Intro_Reverent\n" +
            "BW_Order_Menu_Clean\n" +
            "BW_Order_Menu_Strange\n" +
            "BW_Already_Has_Drink_Cola\n" +
            "BW_Already_Has_Drink_Sap",
            GUILayout.MinHeight(120f));
    }

    private static void RepairAll()
    {
        try
        {
            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupDir = BackupRoot + "/BaristaDialogueRepair_" + stamp;
            EnsureDirectory(BackupRoot);
            EnsureDirectory(backupDir);

            Log("Repair", "Starting repair.");
            Log("Repair", "Backup folder: " + backupDir);

            var dialogueFolder = ResolveDialogueFolder();
            EnsureDirectory(dialogueFolder);
            Log("Repair", "Resolved dialogue folder: " + dialogueFolder);

            EnsureDialogueAsset(
                dialogueFolder,
                "BW_Intro_Warm",
                new[] { "BW_Intro_Neutral", "BW_Intro_Mischievous" },
                new[]
                {
                    new LineSpec("Barista", "You look less lost today. I could make this easy on you, if you let me."),
                    new LineSpec("Rina", "Easy sounds suspicious here. But all right. What are you offering?")
                });

            EnsureDialogueAsset(
                dialogueFolder,
                "BW_Intro_Guarded",
                new[] { "BW_Intro_Neutral", "BW_Intro_Mischievous" },
                new[]
                {
                    new LineSpec("Barista", "You are staring like the menu insulted you. Ask cleanly, or do not ask at all."),
                    new LineSpec("Rina", "Fine. Then let me ask cleanly. What can I actually order?")
                });

            EnsureDialogueAsset(
                dialogueFolder,
                "BW_Intro_Ominous",
                new[] { "BW_Intro_Mischievous", "BW_Intro_Neutral" },
                new[]
                {
                    new LineSpec("Barista", "The room noticed you before I did. That is rarely good."),
                    new LineSpec("Rina", "Then tell me what it wants from me, before it decides on its own.")
                });

            EnsureDialogueAsset(
                dialogueFolder,
                "BW_Intro_Reverent",
                new[] { "BW_Intro_Mischievous", "BW_Intro_Neutral" },
                new[]
                {
                    new LineSpec("Barista", "Most people read the words and miss the wound beneath them. You did not."),
                    new LineSpec("Rina", "Then do not waste the recognition. Tell me what comes next.")
                });

            EnsureDialogueAsset(
                dialogueFolder,
                "BW_Order_Menu_Clean",
                new[] { "BW_Order_Menu" },
                new[]
                {
                    new LineSpec("Barista", "For now, keep it simple. Choose what you can carry."),
                    new LineSpec("Rina", "All right. Show me the options.")
                });

            EnsureDialogueAsset(
                dialogueFolder,
                "BW_Order_Menu_Strange",
                new[] { "BW_Order_Menu" },
                new[]
                {
                    new LineSpec("Barista", "Since you have already stepped off the safe path, I can offer the stranger menu."),
                    new LineSpec("Rina", "Then do not water it down. Show me the real options.")
                });

            EnsureDialogueAsset(
                dialogueFolder,
                "BW_Already_Has_Drink_Cola",
                new[] { "BW_Already_Has_Drink", "BW_Drink_Held" },
                new[]
                {
                    new LineSpec("Barista", "You are already holding a cola. Finish that before you ask me for another miracle.")
                });

            EnsureDialogueAsset(
                dialogueFolder,
                "BW_Already_Has_Drink_Sap",
                new[] { "BW_Already_Has_Drink", "BW_Drink_Held" },
                new[]
                {
                    new LineSpec("Barista", "You are already carrying the photosynthetic sap. Drink it, study it, or fear it. But do not pretend you came empty-handed.")
                });

            BackupAndWrite(BrainPath, BuildBrainFile(), backupDir);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Log("Repair", "Repair completed successfully.");
            Log("Repair", "Next step: run Tools/ROAE/Barista/Repair Live Dialogues + Brain, let Unity recompile, then test Warm/Guarded/Ominous/Reverent presets again.");
        }
        catch (Exception ex)
        {
            Debug.LogError("[ROAE][Editor][DialogueRepair][ERROR] " + ex);
            throw;
        }
    }

    private static string ResolveDialogueFolder()
    {
        var priorityNames = new[]
        {
            "BW_Intro_Neutral",
            "BW_Intro_Mischievous",
            "BW_Order_Menu",
            "BW_Already_Has_Drink"
        };

        for (var i = 0; i < priorityNames.Length; i++)
        {
            var path = FindDialogueAssetPath(priorityNames[i]);
            if (!string.IsNullOrEmpty(path))
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                {
                    return dir.Replace("\\", "/");
                }
            }
        }

        return DefaultDialogueFolder;
    }

    private static string FindDialogueAssetPath(string assetName)
    {
        var guids = AssetDatabase.FindAssets(assetName + " t:DialogueData");
        for (var i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (string.Equals(Path.GetFileNameWithoutExtension(path), assetName, StringComparison.OrdinalIgnoreCase))
            {
                return path.Replace("\\", "/");
            }
        }

        return string.Empty;
    }

    private static void EnsureDialogueAsset(
        string folder,
        string targetName,
        string[] templateCandidates,
        LineSpec[] lines)
    {
        var targetPath = folder + "/" + targetName + ".asset";
        var targetExists = File.Exists(ToAbsolutePath(targetPath));

        if (!targetExists)
        {
            var templatePath = ResolveTemplatePath(templateCandidates);
            if (!string.IsNullOrEmpty(templatePath))
            {
                if (!AssetDatabase.CopyAsset(templatePath, targetPath))
                {
                    throw new InvalidOperationException("Failed to clone template from " + templatePath + " to " + targetPath);
                }

                Log("Dialogue", "Cloned " + targetName + " from template " + Path.GetFileNameWithoutExtension(templatePath));
            }
            else
            {
                var asset = ScriptableObject.CreateInstance<DialogueData>();
                AssetDatabase.CreateAsset(asset, targetPath);
                Log("Dialogue", "Created blank asset " + targetName);
            }

            AssetDatabase.ImportAsset(targetPath);
        }
        else
        {
            Log("Dialogue", "Updating existing asset " + targetName);
        }

        var dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(targetPath);
        if (dialogue == null)
        {
            throw new InvalidOperationException("Failed to load DialogueData at " + targetPath);
        }

        WriteLines(dialogue, lines);
        EditorUtility.SetDirty(dialogue);
    }

    private static string ResolveTemplatePath(string[] templateCandidates)
    {
        for (var i = 0; i < templateCandidates.Length; i++)
        {
            var path = FindDialogueAssetPath(templateCandidates[i]);
            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }
        }

        return string.Empty;
    }

    private static void WriteLines(DialogueData dialogue, LineSpec[] lines)
    {
        var so = new SerializedObject(dialogue);
        var linesProp = so.FindProperty("dialogueLines");
        if (linesProp == null || !linesProp.isArray)
        {
            throw new InvalidOperationException("DialogueData.dialogueLines serialized property not found.");
        }

        linesProp.arraySize = lines.Length;

        for (var i = 0; i < lines.Length; i++)
        {
            var lineProp = linesProp.GetArrayElementAtIndex(i);
            var speakerProp = lineProp.FindPropertyRelative("Speaker");
            var textProp = lineProp.FindPropertyRelative("Text");

            if (speakerProp != null) speakerProp.stringValue = lines[i].speaker;
            if (textProp != null) textProp.stringValue = lines[i].text;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BackupAndWrite(string projectRelativePath, string newContent, string backupDirProjectRelative)
    {
        var absolutePath = ToAbsolutePath(projectRelativePath);
        EnsureDirectory(Path.GetDirectoryName(absolutePath));

        if (File.Exists(absolutePath))
        {
            var backupAbsolute = Path.Combine(ToAbsolutePath(backupDirProjectRelative), SanitizeBackupFileName(projectRelativePath));
            File.Copy(absolutePath, backupAbsolute, true);
            Log("Backup", "Backed up " + projectRelativePath);
        }
        else
        {
            Log("Backup", "File did not exist, writing fresh " + projectRelativePath);
        }

        File.WriteAllText(absolutePath, newContent);
        Log("Write", "Wrote " + projectRelativePath);
    }

    private static void EnsureDirectory(string projectRelativeOrAbsolutePath)
    {
        if (string.IsNullOrEmpty(projectRelativeOrAbsolutePath))
        {
            return;
        }

        var absolutePath = projectRelativeOrAbsolutePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)
            ? ToAbsolutePath(projectRelativeOrAbsolutePath)
            : projectRelativeOrAbsolutePath;

        if (!Directory.Exists(absolutePath))
        {
            Directory.CreateDirectory(absolutePath);
        }
    }

    private static string ToAbsolutePath(string projectRelativePath)
    {
        var dataPath = Application.dataPath.Replace("\\", "/");
        var projectRoot = dataPath.Substring(0, dataPath.Length - "Assets".Length);
        return Path.Combine(projectRoot, projectRelativePath).Replace("\\", "/");
    }

    private static string SanitizeBackupFileName(string projectRelativePath)
    {
        return projectRelativePath
            .Replace("Assets/", string.Empty)
            .Replace("/", "__")
            .Replace("\\", "__")
            + ".bak";
    }

    private static void Log(string channel, string message)
    {
        Debug.Log("[ROAE][Editor][" + channel + "] " + message);
    }

    private static string BuildBrainFile()
    {
        return NormalizeNewLines(@"
using System;
using System.Reflection;
using UnityEngine;

public sealed class BaristaWelcomeBrain : MonoBehaviour
{
    [SerializeField] private BaristaPlannerMode plannerMode = BaristaPlannerMode.ValueIteration;
    [SerializeField] private bool verbosePlannerLogs = true;
    [SerializeField] private bool verboseStateExtraction = false;
    [SerializeField] private MonoBehaviour explicitStateSource;
    [SerializeField] private bool preferOutcomeResolverForFinalTone = true;

    public BaristaIntroTone DecideOpeningTone()
    {
        var runtimeState = ExtractRuntimeState();
        var plannerAction = BaristaIntroPlanningSolvers.DecideAction(runtimeState, plannerMode, verbosePlannerLogs);
        var plannerTone = BaristaIntroPlanningSolvers.MapActionToTone(plannerAction);
        var resolved = ResolveOutcome(runtimeState);
        var finalTone = preferOutcomeResolverForFinalTone ? resolved.introTone : plannerTone;

        Debug.Log(
            ""[ROAE][BaristaWelcomeBrain] planner="" + plannerMode +
            "" extractedState={"" + runtimeState.ToDebugString() + ""}"" +
            "" selectedAction="" + plannerAction +
            "" plannerTone="" + plannerTone +
            "" resolvedOutcome={"" + resolved.BuildDebugString() + ""}"" +
            "" finalTone="" + finalTone);

        return finalTone;
    }

    public string DebugDecideActionLabel()
    {
        var runtimeState = ExtractRuntimeState();
        var plannerAction = BaristaIntroPlanningSolvers.DecideAction(runtimeState, plannerMode, verbosePlannerLogs);
        var resolved = ResolveOutcome(runtimeState);

        return
            ""plannerAction="" + plannerAction +
            "" plannerTone="" + BaristaIntroPlanningSolvers.MapActionToTone(plannerAction) +
            "" outcome="" + resolved.outcomeType +
            "" tone="" + resolved.introTone +
            "" dialogue="" + resolved.dialogueId;
    }

    private BaristaWelcomePlannerResult ResolveOutcome(BaristaIntroPlanningRuntimeState runtimeState)
    {
        var input = new BaristaWelcomePlannerInput
        {
            creativity = ExtractCreativity(),
            empathy = ExtractEmpathy(),
            corruption = runtimeState.corruption,
            readUnknownText = runtimeState.readUnknownText,
            introDone = runtimeState.introDone,
            accepted = runtimeState.acceptedFirstDrink,
            heldDrink = ParseHeldDrink(runtimeState.heldDrink)
        };

        return BaristaWelcomeOutcomeResolver.Resolve(input);
    }

    private static BaristaDrinkType ParseHeldDrink(string heldDrink)
    {
        if (string.IsNullOrEmpty(heldDrink) || string.Equals(heldDrink, ""None"", StringComparison.OrdinalIgnoreCase))
        {
            return BaristaDrinkType.None;
        }

        if (heldDrink.IndexOf(""Photo"", StringComparison.OrdinalIgnoreCase) >= 0 ||
            heldDrink.IndexOf(""Sap"", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return BaristaDrinkType.PhotosyntheticSap;
        }

        if (heldDrink.IndexOf(""Cola"", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return BaristaDrinkType.Cola;
        }

        try
        {
            return (BaristaDrinkType)Enum.Parse(typeof(BaristaDrinkType), heldDrink, true);
        }
        catch
        {
            return BaristaDrinkType.None;
        }
    }

    private BaristaIntroPlanningRuntimeState ExtractRuntimeState()
    {
        var stateSource = ResolvePrimaryStateSource();

        var readUnknownText = ReadBool(
            stateSource,
            new[] { ""readUnknownText"", ""_readUnknownText"", ""ReadUnknownText"" },
            PlayerPrefs.GetInt(""read_unknown_text_01"", 0) == 1);

        var introDone = ReadBool(
            stateSource,
            new[] { ""introDone"", ""_introDone"", ""IntroDone"" },
            false);

        var acceptedFirstDrink = ReadBool(
            stateSource,
            new[] { ""accepted"", ""acceptedFirstDrink"", ""_accepted"", ""Accepted"" },
            false);

        var heldDrink = ReadString(
            stateSource,
            new[] { ""heldDrink"", ""_heldDrink"", ""HeldDrink"" },
            ""None"");

        var corruption = ExtractCorruption();

        var runtimeState = new BaristaIntroPlanningRuntimeState
        {
            readUnknownText = readUnknownText,
            corruption = corruption,
            introDone = introDone,
            acceptedFirstDrink = acceptedFirstDrink,
            heldDrink = string.IsNullOrEmpty(heldDrink) ? ""None"" : heldDrink
        };

        if (verboseStateExtraction)
        {
            Debug.Log(
                ""[ROAE][BaristaWelcomeBrain][StateExtraction] source="" + DescribeObject(stateSource) +
                "" runtimeState={"" + runtimeState.ToDebugString() + ""}"" +
                "" creativity="" + ExtractCreativity() +
                "" empathy="" + ExtractEmpathy());
        }

        return runtimeState;
    }

    private object ResolvePrimaryStateSource()
    {
        if (explicitStateSource != null)
        {
            return explicitStateSource;
        }

        var controllerType = Type.GetType(""BaristaWelcomeController"");
        if (controllerType != null)
        {
            var component = GetComponent(controllerType);
            if (component != null)
            {
                return component;
            }
        }

        return this;
    }

    private int ExtractCreativity()
    {
        var creativeCoreType = Type.GetType(""CreativeCore"");
        if (creativeCoreType == null)
        {
            return PlayerPrefs.GetInt(""creativity"", 40);
        }

        var instance = UnityEngine.Object.FindObjectOfType(creativeCoreType);
        if (instance == null)
        {
            return PlayerPrefs.GetInt(""creativity"", 40);
        }

        var boxedValue = ReadMember(instance, new[] { ""creativity"", ""_creativity"", ""Creativity"" });
        if (boxedValue == null)
        {
            return PlayerPrefs.GetInt(""creativity"", 40);
        }

        try
        {
            return Convert.ToInt32(boxedValue);
        }
        catch
        {
            return PlayerPrefs.GetInt(""creativity"", 40);
        }
    }

    private int ExtractEmpathy()
    {
        var creativeCoreType = Type.GetType(""CreativeCore"");
        if (creativeCoreType == null)
        {
            return PlayerPrefs.GetInt(""empathy"", 0);
        }

        var instance = UnityEngine.Object.FindObjectOfType(creativeCoreType);
        if (instance == null)
        {
            return PlayerPrefs.GetInt(""empathy"", 0);
        }

        var boxedValue = ReadMember(instance, new[] { ""empathy"", ""_empathy"", ""Empathy"" });
        if (boxedValue == null)
        {
            return PlayerPrefs.GetInt(""empathy"", 0);
        }

        try
        {
            return Convert.ToInt32(boxedValue);
        }
        catch
        {
            return PlayerPrefs.GetInt(""empathy"", 0);
        }
    }

    private int ExtractCorruption()
    {
        var creativeCoreType = Type.GetType(""CreativeCore"");
        if (creativeCoreType == null)
        {
            return PlayerPrefs.GetInt(""plantCorruption"", 0);
        }

        var instance = UnityEngine.Object.FindObjectOfType(creativeCoreType);
        if (instance == null)
        {
            return PlayerPrefs.GetInt(""plantCorruption"", 0);
        }

        var boxedValue = ReadMember(instance, new[] { ""plantCorruption"", ""_plantCorruption"", ""corruption"", ""_corruption"" });
        if (boxedValue == null)
        {
            return PlayerPrefs.GetInt(""plantCorruption"", 0);
        }

        try
        {
            return Convert.ToInt32(boxedValue);
        }
        catch
        {
            return PlayerPrefs.GetInt(""plantCorruption"", 0);
        }
    }

    private static bool ReadBool(object source, string[] memberNames, bool fallback)
    {
        var boxedValue = ReadMember(source, memberNames);
        if (boxedValue == null)
        {
            return fallback;
        }

        if (boxedValue is bool boolValue)
        {
            return boolValue;
        }

        try
        {
            return Convert.ToInt32(boxedValue) != 0;
        }
        catch
        {
            return fallback;
        }
    }

    private static string ReadString(object source, string[] memberNames, string fallback)
    {
        var boxedValue = ReadMember(source, memberNames);
        if (boxedValue == null)
        {
            return fallback;
        }

        if (boxedValue is Enum enumValue)
        {
            return enumValue.ToString();
        }

        return boxedValue.ToString();
    }

    private static object ReadMember(object source, string[] memberNames)
    {
        if (source == null)
        {
            return null;
        }

        var type = source.GetType();

        for (var i = 0; i < memberNames.Length; i++)
        {
            var memberName = memberNames[i];

            var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                return field.GetValue(source);
            }

            var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.CanRead)
            {
                return property.GetValue(source, null);
            }

            var method = type.GetMethod(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (method != null)
            {
                return method.Invoke(source, null);
            }
        }

        return null;
    }

    private static string DescribeObject(object source)
    {
        if (source == null)
        {
            return ""<null>"";
        }

        var unityObject = source as UnityEngine.Object;
        if (unityObject != null)
        {
            return unityObject.name + "" ("" + source.GetType().Name + "")"";
        }

        return source.GetType().Name;
    }
}
");
    }

    private static string NormalizeNewLines(string input)
    {
        return input.Trim() + Environment.NewLine;
    }
}
#endif