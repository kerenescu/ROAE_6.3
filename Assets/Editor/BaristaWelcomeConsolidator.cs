using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class BaristaWelcomeConsolidator
{
    private const string DialoguesFolder = "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Dialogues";

    [MenuItem("ROAE/Barista/Consolidate Selected Barista")]
    private static void ConsolidateSelectedBarista()
    {
        GameObject barista = Selection.activeGameObject;
        if (barista == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeConsolidator] Select Barista first.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        BaristaDialogueTrigger trigger = GetOrAddComponent<BaristaDialogueTrigger>(barista);
        BaristaWelcomeBrain brain = GetOrAddComponent<BaristaWelcomeBrain>(barista);

        RemoveIfExists<BaristaWelcomeController>(barista);
        RemoveIfExists<BaristaWelcomeDebugMenu>(barista);
        RemoveIfExists<NpcRelationshipState>(barista);

        DialogueManager dialogueManager = FindSceneObject<DialogueManager>(true);

        WireBrain(brain);
        WireTrigger(trigger, brain, dialogueManager);

        EditorUtility.SetDirty(barista);
        EditorSceneManager.MarkSceneDirty(barista.scene);
        Undo.CollapseUndoOperations(group);

        Debug.Log("[ROAE][BaristaWelcomeConsolidator] Runtime cleaned on: " + barista.name);
    }

    [MenuItem("ROAE/Barista/Create Or Update Debug Tools For Selected Barista")]
    private static void CreateOrUpdateDebugToolsForSelectedBarista()
    {
        GameObject barista = Selection.activeGameObject;
        if (barista == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeConsolidator] Select Barista first.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        BaristaWelcomeBrain brain = GetOrAddComponent<BaristaWelcomeBrain>(barista);
        WireBrain(brain);

        GameObject tools = FindOrCreateDebugToolsObject(barista.name + "_DebugTools", barista.scene);

        BaristaWelcomeController controller = GetOrAddComponent<BaristaWelcomeController>(tools);
        BaristaWelcomeDebugMenu debugMenu = GetOrAddComponent<BaristaWelcomeDebugMenu>(tools);

        CreativeCore creativeCore = FindSceneObject<CreativeCore>(true);

        WireController(controller, brain);
        WireDebugMenu(debugMenu, creativeCore, controller, brain);

        EditorUtility.SetDirty(tools);
        EditorSceneManager.MarkSceneDirty(barista.scene);
        Undo.CollapseUndoOperations(group);

        Debug.Log("[ROAE][BaristaWelcomeConsolidator] Debug tools ready: " + tools.name);
    }

    [MenuItem("ROAE/Barista/Full Setup Selected Barista")]
    private static void FullSetupSelectedBarista()
    {
        ConsolidateSelectedBarista();
        CreateOrUpdateDebugToolsForSelectedBarista();
    }

    [MenuItem("ROAE/Barista/Consolidate Selected Barista", true)]
    [MenuItem("ROAE/Barista/Create Or Update Debug Tools For Selected Barista", true)]
    [MenuItem("ROAE/Barista/Full Setup Selected Barista", true)]
    private static bool ValidateMenu()
    {
        return Selection.activeGameObject != null;
    }

    private static void WireBrain(BaristaWelcomeBrain brain)
    {
        SerializedObject so = new SerializedObject(brain);

        SetEnumByName(so, "plannerMode", "ValueIteration");
        SetBool(so, "verbosePlannerLogs", true);
        SetBool(so, "verboseStateExtraction", false);
        SetObject(so, "explicitStateSource", null);

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(brain);
    }

    private static void WireTrigger(BaristaDialogueTrigger trigger, BaristaWelcomeBrain brain, DialogueManager dialogueManager)
    {
        SerializedObject so = new SerializedObject(trigger);

        SetObject(so, "dialogueManager", dialogueManager);
        SetObject(so, "brain", brain);

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

        SetBool(so, "debugLog", true);

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(trigger);

        if (dialogueManager == null)
            Debug.LogWarning("[ROAE][BaristaWelcomeConsolidator] DialogueManager not found in scene.");
    }

    private static void WireController(BaristaWelcomeController controller, BaristaWelcomeBrain brain)
    {
        SerializedObject so = new SerializedObject(controller);
        SetObject(so, "brain", brain);
        SetBool(so, "debugLog", true);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
    }

    private static void WireDebugMenu(
        BaristaWelcomeDebugMenu debugMenu,
        CreativeCore creativeCore,
        BaristaWelcomeController controller,
        BaristaWelcomeBrain brain)
    {
        SerializedObject so = new SerializedObject(debugMenu);

        SetObject(so, "creativeCore", creativeCore);
        SetObject(so, "controller", controller);
        SetObject(so, "brain", brain);

        SetInt(so, "previewCreativity", 40);
        SetInt(so, "previewEmpathy", 0);
        SetInt(so, "previewCorruption", 0);
        SetInt(so, "previewRelationship", 0);
        SetBool(so, "previewReadUnknownText", false);
        SetBool(so, "previewIntroDone", false);
        SetBool(so, "previewAccepted", false);
        SetEnumByName(so, "previewHeldDrink", "None");

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(debugMenu);
    }

    private static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T existing = go.GetComponent<T>();
        if (existing != null) return existing;
        return Undo.AddComponent<T>(go);
    }

    private static void RemoveIfExists<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        if (c != null)
            Undo.DestroyObjectImmediate(c);
    }

    private static DialogueData LoadDialogue(string assetName)
    {
        string exactPath = DialoguesFolder + "/" + assetName + ".asset";
        DialogueData asset = AssetDatabase.LoadAssetAtPath<DialogueData>(exactPath);
        if (asset != null) return asset;

        string[] guids = AssetDatabase.FindAssets(assetName + " t:DialogueData", new[] { DialoguesFolder });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            DialogueData candidate = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
            if (candidate != null && candidate.name == assetName)
                return candidate;
        }

        Debug.LogWarning("[ROAE][BaristaWelcomeConsolidator] Missing dialogue: " + assetName);
        return null;
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

    private static GameObject FindOrCreateDebugToolsObject(string objectName, UnityEngine.SceneManagement.Scene scene)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing != null)
            return existing;

        GameObject go = new GameObject(objectName);
        Undo.RegisterCreatedObjectUndo(go, "Create " + objectName);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    private static void SetObject(SerializedObject so, string propertyName, Object value)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeConsolidator] Missing property: " + propertyName);
            return;
        }

        p.objectReferenceValue = value;
    }

    private static void SetBool(SerializedObject so, string propertyName, bool value)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeConsolidator] Missing property: " + propertyName);
            return;
        }

        p.boolValue = value;
    }

    private static void SetInt(SerializedObject so, string propertyName, int value)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeConsolidator] Missing property: " + propertyName);
            return;
        }

        p.intValue = value;
    }

    private static void SetEnumByName(SerializedObject so, string propertyName, string enumName)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeConsolidator] Missing property: " + propertyName);
            return;
        }

        int idx = System.Array.IndexOf(p.enumDisplayNames, enumName);
        if (idx < 0)
            idx = System.Array.IndexOf(p.enumNames, enumName);

        if (idx < 0)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeConsolidator] Enum not found: " + propertyName + " -> " + enumName);
            return;
        }

        p.enumValueIndex = idx;
    }
}