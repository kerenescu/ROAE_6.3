using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CompanionAuthoringMenu
{
    private const string DataRoot = "Assets/ROAE_2/Data/Companion";
    private const string ProfilePath = DataRoot + "/CompanionProfile.asset";
    private const string DialoguePath = DataRoot + "/CompanionDialogueLibrary.asset";

    [MenuItem("ROAE/Companion/Create Starter Assets")]
    public static void CreateStarterAssets()
    {
        EnsureFolder("Assets/ROAE_2/Data");
        EnsureFolder(DataRoot);

        CompanionProfile profile = AssetDatabase.LoadAssetAtPath<CompanionProfile>(ProfilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<CompanionProfile>();
            profile.name = "CompanionProfile";
            profile.EnsureStarterRules();
            AssetDatabase.CreateAsset(profile, ProfilePath);
        }

        CompanionDialogueLibrary library = AssetDatabase.LoadAssetAtPath<CompanionDialogueLibrary>(DialoguePath);
        if (library == null)
        {
            library = ScriptableObject.CreateInstance<CompanionDialogueLibrary>();
            library.name = "CompanionDialogueLibrary";
            library.EnsureStarterEntries();
            AssetDatabase.CreateAsset(library, DialoguePath);
        }

        EditorUtility.SetDirty(profile);
        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.objects = new Object[] { profile, library };
        Debug.Log("[ROAE][CompanionAuthoringMenu] Starter assets are ready.");
    }

    [MenuItem("ROAE/Companion/Create Runtime Host In Scene")]
    public static void CreateRuntimeHostInScene()
    {
        CreateStarterAssets();

        CompanionSystem existing = Object.FindFirstObjectByType<CompanionSystem>();
        if (existing != null)
        {
            Selection.activeObject = existing.gameObject;
            Debug.Log("[ROAE][CompanionAuthoringMenu] Scene already contains a CompanionSystem.");
            return;
        }

        GameObject host = new GameObject("ROAE Companion System");
        CompanionSystem system = host.AddComponent<CompanionSystem>();
        system.AssignAuthoringAssets(
            AssetDatabase.LoadAssetAtPath<CompanionProfile>(ProfilePath),
            AssetDatabase.LoadAssetAtPath<CompanionDialogueLibrary>(DialoguePath));

        Undo.RegisterCreatedObjectUndo(host, "Create ROAE Companion System");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeObject = host;
        Debug.Log("[ROAE][CompanionAuthoringMenu] Runtime host created in the active scene.");
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
        string name = Path.GetFileName(folderPath);
        if (!string.IsNullOrWhiteSpace(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, name);
    }
}
