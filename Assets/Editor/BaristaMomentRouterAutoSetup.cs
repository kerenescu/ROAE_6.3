using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class BaristaMomentRouterAutoSetup
{
    private const string MomentsFolder = "Assets/ROAE_2/Data/NarrativeV2/BaristaMoments";
    private const string WelcomeMomentAssetPath = MomentsFolder + "/BM_Welcome_Default.asset";
    private const string DialoguesFolder = "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Dialogues";

    [MenuItem("ROAE/Barista/Install Moment Router On Selected Barista")]
    private static void InstallMomentRouterOnSelectedBarista()
    {
        GameObject barista = Selection.activeGameObject;
        if (barista == null)
        {
            Debug.LogWarning("[ROAE][BaristaMomentRouterAutoSetup] Select the Barista GameObject first.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        EnsureFolder(MomentsFolder);

        BaristaWelcomeBrain brain = GetOrAddComponent<BaristaWelcomeBrain>(barista);
        NpcMomentRouter router = GetOrAddComponent<NpcMomentRouter>(barista);
        DialogueManager dialogueManager = FindSceneObject<DialogueManager>(true);

        BaristaMomentDefinition welcomeDef = GetOrCreateWelcomeMomentDefinition();
        WireWelcomeDefinition(welcomeDef);
        WireRouter(router, brain, dialogueManager, welcomeDef);

        RemoveLegacyTrigger(barista);

        EditorUtility.SetDirty(barista);
        EditorUtility.SetDirty(welcomeDef);
        EditorSceneManager.MarkSceneDirty(barista.scene);

        Undo.CollapseUndoOperations(group);

        Debug.Log("[ROAE][BaristaMomentRouterAutoSetup] Router installed on: " + barista.name);
    }

    [MenuItem("ROAE/Barista/Create Or Update Welcome Moment Asset")]
    private static void CreateOrUpdateWelcomeMomentAsset()
    {
        EnsureFolder(MomentsFolder);

        BaristaMomentDefinition welcomeDef = GetOrCreateWelcomeMomentDefinition();
        WireWelcomeDefinition(welcomeDef);

        EditorUtility.SetDirty(welcomeDef);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ROAE][BaristaMomentRouterAutoSetup] Welcome moment asset ready: " + WelcomeMomentAssetPath);
    }

    [MenuItem("ROAE/Barista/Install Moment Router On Selected Barista", true)]
    private static bool ValidateInstallMomentRouter()
    {
        return Selection.activeGameObject != null;
    }

    private static void RemoveLegacyTrigger(GameObject go)
    {
        BaristaDialogueTrigger legacy = go.GetComponent<BaristaDialogueTrigger>();
        if (legacy != null)
            Undo.DestroyObjectImmediate(legacy);
    }

    private static void WireRouter(
        NpcMomentRouter router,
        BaristaWelcomeBrain brain,
        DialogueManager dialogueManager,
        BaristaMomentDefinition welcomeDef)
    {
        SerializedObject so = new SerializedObject(router);

        SetObject(so, "dialogueManager", dialogueManager);
        SetObject(so, "baristaBrain", brain);
        SetBool(so, "debugLog", true);

        SerializedProperty momentsProp = so.FindProperty("baristaMoments");
        if (momentsProp != null)
        {
            momentsProp.arraySize = 1;
            momentsProp.GetArrayElementAtIndex(0).objectReferenceValue = welcomeDef;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(router);
    }

    private static BaristaMomentDefinition GetOrCreateWelcomeMomentDefinition()
    {
        BaristaMomentDefinition asset = AssetDatabase.LoadAssetAtPath<BaristaMomentDefinition>(WelcomeMomentAssetPath);
        if (asset != null)
            return asset;

        asset = ScriptableObject.CreateInstance<BaristaMomentDefinition>();
        AssetDatabase.CreateAsset(asset, WelcomeMomentAssetPath);
        AssetDatabase.SaveAssets();
        return asset;
    }

    private static void WireWelcomeDefinition(BaristaMomentDefinition def)
    {
        SerializedObject so = new SerializedObject(def);

        SetString(so, "momentId", "barista_welcome");
        SetBool(so, "isEnabled", true);
        SetInt(so, "priority", 100);

        SetBool(so, "skipWhenCompleted", false);
        SetString(so, "completedFlagKey", "");
        SetEnumByName(so, "mode", "StandardBaristaLoop");
        SetEnumByName(so, "toneMode", "UseBrainOnIntroAndStoredLoop");

        ClearStringArray(so, "requiredTrueFlags");
        ClearStringArray(so, "requiredFalseFlags");

        SetObject(so, "singleDialogue", null);

        SetObject(so, "neutralIntroDialogue", LoadDialogue("BW_Intro_Neutral"));
        SetObject(so, "warmIntroDialogue", LoadDialogue("BW_Intro_Warm"));
        SetObject(so, "mischievousIntroDialogue", LoadDialogue("BW_Intro_Mischievous"));

        SetObject(so, "neutralOrderMenuDialogue", LoadDialogue("BW_Order_Menu_Clean"));
        SetObject(so, "warmOrderMenuDialogue", LoadDialogue("BW_Order_Menu_Warm"));
        SetObject(so, "mischievousOrderMenuDialogue", LoadDialogue("BW_Order_Menu_Strange"));

        SetObject(so, "neutralPreparingDialogue", LoadDialogue("BW_Drink_Preparing_Neutral"));
        SetObject(so, "warmPreparingDialogue", LoadDialogue("BW_Drink_Preparing_Warm"));
        SetObject(so, "mischievousPreparingDialogue", LoadDialogue("BW_Drink_Preparing_Mischievous"));

        SetObject(so, "alreadyHasColaDialogue", LoadDialogue("BW_Already_Has_Drink_Cola"));
        SetObject(so, "alreadyHasSapDialogue", LoadDialogue("BW_Already_Has_Drink_Sap"));
        SetObject(so, "genericAlreadyHasDrinkDialogue", null);

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(def);
        AssetDatabase.SaveAssets();
    }

    private static DialogueData LoadDialogue(string assetName)
    {
        string exactPath = DialoguesFolder + "/" + assetName + ".asset";
        DialogueData asset = AssetDatabase.LoadAssetAtPath<DialogueData>(exactPath);
        if (asset != null)
            return asset;

        string[] guids = AssetDatabase.FindAssets(assetName + " t:DialogueData", new[] { DialoguesFolder });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            DialogueData candidate = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
            if (candidate != null && candidate.name == assetName)
                return candidate;
        }

        Debug.LogWarning("[ROAE][BaristaMomentRouterAutoSetup] Missing dialogue asset: " + assetName);
        return null;
    }

    private static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T existing = go.GetComponent<T>();
        if (existing != null)
            return existing;

        return Undo.AddComponent<T>(go);
    }

    private static T FindSceneObject<T>(bool includeInactive) where T : Component
    {
        T[] all = Resources.FindObjectsOfTypeAll<T>();
        for (int i = 0; i < all.Length; i++)
        {
            T c = all[i];
            if (c == null) continue;
            if (EditorUtility.IsPersistent(c)) continue;
            if (!c.gameObject.scene.IsValid()) continue;
            if (!includeInactive && !c.gameObject.activeInHierarchy) continue;
            return c;
        }

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

    private static void SetObject(SerializedObject so, string propertyName, Object value)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null)
        {
            Debug.LogWarning("[ROAE][BaristaMomentRouterAutoSetup] Missing property: " + propertyName);
            return;
        }

        p.objectReferenceValue = value;
    }

    private static void SetBool(SerializedObject so, string propertyName, bool value)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null)
        {
            Debug.LogWarning("[ROAE][BaristaMomentRouterAutoSetup] Missing property: " + propertyName);
            return;
        }

        p.boolValue = value;
    }

    private static void SetInt(SerializedObject so, string propertyName, int value)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null)
        {
            Debug.LogWarning("[ROAE][BaristaMomentRouterAutoSetup] Missing property: " + propertyName);
            return;
        }

        p.intValue = value;
    }

    private static void SetString(SerializedObject so, string propertyName, string value)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null)
        {
            Debug.LogWarning("[ROAE][BaristaMomentRouterAutoSetup] Missing property: " + propertyName);
            return;
        }

        p.stringValue = value ?? string.Empty;
    }

    private static void SetEnumByName(SerializedObject so, string propertyName, string enumName)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null)
        {
            Debug.LogWarning("[ROAE][BaristaMomentRouterAutoSetup] Missing property: " + propertyName);
            return;
        }

        int idx = System.Array.IndexOf(p.enumDisplayNames, enumName);
        if (idx < 0)
            idx = System.Array.IndexOf(p.enumNames, enumName);

        if (idx < 0)
        {
            Debug.LogWarning("[ROAE][BaristaMomentRouterAutoSetup] Enum not found: " + propertyName + " -> " + enumName);
            return;
        }

        p.enumValueIndex = idx;
    }

    private static void ClearStringArray(SerializedObject so, string propertyName)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null)
        {
            Debug.LogWarning("[ROAE][BaristaMomentRouterAutoSetup] Missing property: " + propertyName);
            return;
        }

        p.arraySize = 0;
    }
}