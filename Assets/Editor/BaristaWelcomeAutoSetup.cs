using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using Object = UnityEngine.Object;

public static class BaristaWelcomeAutoSetup
{
    private const string DialoguesFolder = "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Dialogues";
    private const string EffectsFolder = "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Effects";

    [MenuItem("ROAE/Barista/Setup Selected Barista/Runtime Only")]
    private static void SetupSelectedBaristaRuntimeOnly()
    {
        GameObject go = GetSelectedBaristaOrWarn();
        if (go == null) return;

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        BaristaWelcomeBrain brain = GetOrAddComponent<BaristaWelcomeBrain>(go);
        BaristaDialogueTrigger trigger = GetOrAddComponent<BaristaDialogueTrigger>(go);

        RemoveComponentIfExists<BaristaWelcomeController>(go);
        RemoveComponentIfExists<BaristaWelcomeDebugMenu>(go);
        RemoveComponentIfExists<NpcRelationshipState>(go);

        DialogueManager dialogueManager = FindSceneObject<DialogueManager>(true);

        WireBrain(brain, false);
        WireTrigger(trigger, brain, dialogueManager);

        EditorUtility.SetDirty(go);
        if (dialogueManager != null) EditorUtility.SetDirty(dialogueManager);
        EditorSceneManager.MarkSceneDirty(go.scene);

        Undo.CollapseUndoOperations(group);

        Debug.Log("[ROAE][BaristaWelcomeAutoSetup] Runtime-only setup complete for: " + go.name);
    }

    [MenuItem("ROAE/Barista/Setup Selected Barista/Runtime + Debug")]
    private static void SetupSelectedBaristaRuntimeAndDebug()
    {
        GameObject go = GetSelectedBaristaOrWarn();
        if (go == null) return;

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        BaristaWelcomeBrain brain = GetOrAddComponent<BaristaWelcomeBrain>(go);
        BaristaDialogueTrigger trigger = GetOrAddComponent<BaristaDialogueTrigger>(go);
        BaristaWelcomeController controller = GetOrAddComponent<BaristaWelcomeController>(go);
        BaristaWelcomeDebugMenu debugMenu = GetOrAddComponent<BaristaWelcomeDebugMenu>(go);

        RemoveComponentIfExists<NpcRelationshipState>(go);

        DialogueManager dialogueManager = FindSceneObject<DialogueManager>(true);
        CreativeCore creativeCore = FindSceneObject<CreativeCore>(true);

        WireBrain(brain, true);
        WireTrigger(trigger, brain, dialogueManager);
        WireController(controller, brain);
        WireDebugMenu(debugMenu, creativeCore, controller, brain);

        EditorUtility.SetDirty(go);
        if (dialogueManager != null) EditorUtility.SetDirty(dialogueManager);
        EditorSceneManager.MarkSceneDirty(go.scene);

        Undo.CollapseUndoOperations(group);

        Debug.Log("[ROAE][BaristaWelcomeAutoSetup] Runtime+Debug setup complete for: " + go.name);
    }

    [MenuItem("ROAE/Barista/Setup Selected Barista/Strip Legacy Components")]
    private static void StripLegacyComponents()
    {
        GameObject go = GetSelectedBaristaOrWarn();
        if (go == null) return;

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        RemoveComponentIfExists<NpcRelationshipState>(go);
        RemoveComponentIfExists<BaristaWelcomeDebugMenu>(go);
        RemoveComponentIfExists<BaristaWelcomeController>(go);

        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(go.scene);

        Undo.CollapseUndoOperations(group);

        Debug.Log("[ROAE][BaristaWelcomeAutoSetup] Legacy components stripped from: " + go.name);
    }

    [MenuItem("ROAE/Barista/Setup Selected Barista/Validate Scene Refs")]
    private static void ValidateSceneRefs()
    {
        GameObject go = GetSelectedBaristaOrWarn();
        if (go == null) return;

        DialogueManager dialogueManager = FindSceneObject<DialogueManager>(true);
        CreativeCore creativeCore = FindSceneObject<CreativeCore>(true);

        Debug.Log(
            "[ROAE][BaristaWelcomeAutoSetup] Validate | selected=" + go.name +
            " | DialogueManager=" + (dialogueManager != null ? dialogueManager.name : "NULL") +
            " | CreativeCore=" + (creativeCore != null ? creativeCore.name : "NULL"));
    }

    [MenuItem("ROAE/Barista/Setup Selected Barista/Runtime Only", true)]
    [MenuItem("ROAE/Barista/Setup Selected Barista/Runtime + Debug", true)]
    [MenuItem("ROAE/Barista/Setup Selected Barista/Strip Legacy Components", true)]
    [MenuItem("ROAE/Barista/Setup Selected Barista/Validate Scene Refs", true)]
    private static bool ValidateSelectedBaristaMenu()
    {
        return Selection.activeGameObject != null;
    }

    private static GameObject GetSelectedBaristaOrWarn()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeAutoSetup] Select the Barista GameObject first.");
            return null;
        }

        return go;
    }

    private static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T existing = go.GetComponent<T>();
        if (existing != null) return existing;
        return Undo.AddComponent<T>(go);
    }

    private static void RemoveComponentIfExists<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        if (c != null)
            Undo.DestroyObjectImmediate(c);
    }

    private static void WireBrain(BaristaWelcomeBrain brain, bool debugMode)
    {
        SerializedObject so = new SerializedObject(brain);
        SetEnumByName(so, "plannerMode", "ValueIteration");
        SetBool(so, "verbosePlannerLogs", debugMode);
        SetBool(so, "verboseStateExtraction", debugMode);
        SetObject(so, "explicitStateSource", null);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(brain);
    }

    private static void WireTrigger(
        BaristaDialogueTrigger trigger,
        BaristaWelcomeBrain brain,
        DialogueManager dialogueManager)
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
            Debug.LogWarning("[ROAE][BaristaWelcomeAutoSetup] No DialogueManager found in scene. Trigger was wired except for dialogueManager.");
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

        Debug.LogWarning("[ROAE][BaristaWelcomeAutoSetup] Missing dialogue asset: " + assetName);
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

    private static void SetObject(SerializedObject so, string propertyName, Object value)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeAutoSetup] Missing serialized property: " + propertyName);
            return;
        }

        p.objectReferenceValue = value;
    }

    private static void SetBool(SerializedObject so, string propertyName, bool value)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeAutoSetup] Missing serialized property: " + propertyName);
            return;
        }

        p.boolValue = value;
    }

    private static void SetInt(SerializedObject so, string propertyName, int value)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeAutoSetup] Missing serialized property: " + propertyName);
            return;
        }

        p.intValue = value;
    }

    private static void SetEnumByName(SerializedObject so, string propertyName, string enumName)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeAutoSetup] Missing serialized property: " + propertyName);
            return;
        }

        int idx = System.Array.IndexOf(p.enumDisplayNames, enumName);
        if (idx < 0)
            idx = System.Array.IndexOf(p.enumNames, enumName);

        if (idx < 0)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeAutoSetup] Enum value not found: " + propertyName + " -> " + enumName);
            return;
        }

        p.enumValueIndex = idx;
    }
}