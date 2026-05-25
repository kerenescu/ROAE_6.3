using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class MelcCompanionBuilder
{
    private const string SessionKey = "ROAE.MelcCompanionBuilder.AutoBuilt";
    private const string FramesFolder = "Assets/ROAE_2/characters/Melc/Frames";
    private const string AnimationsFolder = "Assets/ROAE_2/characters/Melc/Animations";
    private const string PrefabFolder = "Assets/ROAE_2/Prefabs/Companion";
    private const string SummonClipPath = AnimationsFolder + "/MelcSummon.anim";
    private const string IdleClipPath = AnimationsFolder + "/MelcIdle.anim";
    private const string ControllerPath = AnimationsFolder + "/MelcCompanion.controller";
    private const string PrefabPath = PrefabFolder + "/CompanionManifestation.prefab";
    private const string ProfilePath = "Assets/ROAE_2/Data/Companion/CompanionProfile.asset";
    private const string DialoguePath = "Assets/ROAE_2/Data/Companion/CompanionDialogueLibrary.asset";

    [InitializeOnLoadMethod]
    private static void ScheduleAutoBuild()
    {
        EditorApplication.delayCall += TryAutoBuildOnce;
    }

    [MenuItem("ROAE/Companion/Build Melc Companion From Frames")]
    public static void BuildFromFramesMenu()
    {
        BuildOrUpdateAssets(forceRebuild: true);
    }

    private static void TryAutoBuildOnce()
    {
        if (SessionState.GetBool(SessionKey, false))
            return;

        SessionState.SetBool(SessionKey, true);

        if (!Directory.Exists(ToAbsolutePath(FramesFolder)))
            return;

        if (File.Exists(ToAbsolutePath(PrefabPath)) &&
            File.Exists(ToAbsolutePath(ControllerPath)) &&
            File.Exists(ToAbsolutePath(SummonClipPath)))
        {
            return;
        }

        BuildOrUpdateAssets(forceRebuild: false);
    }

    private static void BuildOrUpdateAssets(bool forceRebuild)
    {
        AssetDatabase.Refresh();
        EnsureFolder("Assets/ROAE_2/Data");
        EnsureFolder("Assets/ROAE_2/Data/Companion");
        EnsureFolder("Assets/ROAE_2/characters");
        EnsureFolder("Assets/ROAE_2/characters/Melc");
        EnsureFolder(AnimationsFolder);
        EnsureFolder(PrefabFolder);

        string[] framePaths = Directory.GetFiles(ToAbsolutePath(FramesFolder), "*.png", SearchOption.TopDirectoryOnly)
            .Select(ToAssetPath)
            .OrderBy(GetFrameIndex)
            .ToArray();

        if (framePaths.Length == 0)
        {
            Debug.LogWarning("[ROAE][MelcCompanionBuilder] No frame PNGs found in " + FramesFolder);
            return;
        }

        List<Sprite> sprites = new List<Sprite>(framePaths.Length);
        for (int i = 0; i < framePaths.Length; i++)
        {
            ConfigureSpriteImporter(framePaths[i]);
        }

        AssetDatabase.Refresh();

        for (int i = 0; i < framePaths.Length; i++)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(framePaths[i]);
            if (sprite != null)
                sprites.Add(sprite);
        }

        if (sprites.Count == 0)
        {
            Debug.LogWarning("[ROAE][MelcCompanionBuilder] Frames imported, but no sprites could be loaded.");
            return;
        }

        if (forceRebuild)
        {
            DeleteGeneratedAsset(SummonClipPath);
            DeleteGeneratedAsset(IdleClipPath);
            DeleteGeneratedAsset(ControllerPath);
            DeleteGeneratedAsset(PrefabPath);
        }

        AnimationClip summonClip = CreateSummonClip(sprites);
        AnimationClip idleClip = CreateIdleClip(sprites[sprites.Count - 1]);
        AnimatorController controller = CreateController(summonClip, idleClip);
        GameObject prefabRoot = CreateOrUpdatePrefab(controller, sprites[0]);
        CompanionTrailPrefabBaker.BakeTrailIntoCompanionPrefab();

        CompanionAuthoringMenu.CreateStarterAssets();
        AssignPrefabToProfileAndSystems(prefabRoot.GetComponent<CompanionManifestationController>());

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ROAE][MelcCompanionBuilder] Generated summon clip, idle clip, controller, and companion prefab.");
    }

    private static void ConfigureSpriteImporter(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            return;

        bool changed = false;

        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            changed = true;
        }

        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            changed = true;
        }

        if (importer.spritePixelsPerUnit != 100f)
        {
            importer.spritePixelsPerUnit = 100f;
            changed = true;
        }

        if (importer.filterMode != FilterMode.Bilinear)
        {
            importer.filterMode = FilterMode.Bilinear;
            changed = true;
        }

        if (importer.wrapMode != TextureWrapMode.Clamp)
        {
            importer.wrapMode = TextureWrapMode.Clamp;
            changed = true;
        }

        TextureImporterSettings textureSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureSettings);

        if (textureSettings.spriteAlignment != (int)SpriteAlignment.BottomCenter)
        {
            textureSettings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            changed = true;
        }

        Vector2 targetPivot = new Vector2(0.5f, 0f);
        if (textureSettings.spritePivot != targetPivot)
        {
            textureSettings.spritePivot = targetPivot;
            changed = true;
        }

        if (changed)
            importer.SetTextureSettings(textureSettings);

        if (changed)
            importer.SaveAndReimport();
    }

    private static AnimationClip CreateSummonClip(IReadOnlyList<Sprite> sprites)
    {
        AnimationClip clip = LoadOrCreateClip(SummonClipPath, "MelcSummon");
        clip.frameRate = 24f;
        SetSpriteCurve(clip, sprites);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        settings.stopTime = Mathf.Max(0.01f, (sprites.Count - 1) / clip.frameRate);
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static AnimationClip CreateIdleClip(Sprite sprite)
    {
        AnimationClip clip = LoadOrCreateClip(IdleClipPath, "MelcIdle");
        clip.frameRate = 12f;
        SetSpriteCurve(clip, new[] { sprite, sprite });

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        settings.stopTime = 1f / clip.frameRate;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static void SetSpriteCurve(AnimationClip clip, IReadOnlyList<Sprite> sprites)
    {
        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[sprites.Count];

        for (int i = 0; i < sprites.Count; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i / Mathf.Max(1f, clip.frameRate),
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
    }

    private static AnimationClip LoadOrCreateClip(string assetPath, string name)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
        if (clip != null)
            return clip;

        clip = new AnimationClip
        {
            name = name
        };
        AssetDatabase.CreateAsset(clip, assetPath);
        return clip;
    }

    private static AnimatorController CreateController(AnimationClip summonClip, AnimationClip idleClip)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        }

        EnsureParameter(controller, "Mood", AnimatorControllerParameterType.Int);
        EnsureParameter(controller, "Shell", AnimatorControllerParameterType.Bool);
        EnsureParameter(controller, "Summon", AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, "Despawn", AnimatorControllerParameterType.Trigger);

        AnimatorControllerLayer layer = controller.layers[0];
        AnimatorStateMachine machine = layer.stateMachine;

        AnimatorState idleState = FindOrCreateState(machine, "MelcIdle", new Vector3(260f, 120f, 0f));
        AnimatorState summonState = FindOrCreateState(machine, "MelcSummon", new Vector3(520f, 120f, 0f));

        idleState.motion = idleClip;
        summonState.motion = summonClip;
        machine.defaultState = idleState;

        if (!HasTransition(idleState, summonState))
        {
            AnimatorStateTransition transition = idleState.AddTransition(summonState);
            transition.hasExitTime = false;
            transition.duration = 0f;
            transition.AddCondition(AnimatorConditionMode.If, 0f, "Summon");
        }

        if (!HasTransition(summonState, idleState))
        {
            AnimatorStateTransition transition = summonState.AddTransition(idleState);
            transition.hasExitTime = true;
            transition.exitTime = 0.99f;
            transition.duration = 0f;
        }

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static GameObject CreateOrUpdatePrefab(AnimatorController controller, Sprite firstSprite)
    {
        GameObject prefabRoot;
        CompanionManifestationController manifestation;
        Transform artRoot;
        SpriteRenderer spriteRenderer;
        Animator animator;

        if (File.Exists(ToAbsolutePath(PrefabPath)))
        {
            prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
            manifestation = prefabRoot.GetComponent<CompanionManifestationController>() ??
                            prefabRoot.AddComponent<CompanionManifestationController>();

            artRoot = prefabRoot.transform.Find("ArtRoot");
            if (artRoot == null)
            {
                GameObject art = new GameObject("ArtRoot");
                art.transform.SetParent(prefabRoot.transform, false);
                artRoot = art.transform;
            }

            spriteRenderer = artRoot.GetComponent<SpriteRenderer>() ?? artRoot.gameObject.AddComponent<SpriteRenderer>();
            animator = artRoot.GetComponent<Animator>() ?? artRoot.gameObject.AddComponent<Animator>();
        }
        else
        {
            prefabRoot = new GameObject("CompanionManifestation");
            manifestation = prefabRoot.AddComponent<CompanionManifestationController>();
            GameObject art = new GameObject("ArtRoot");
            art.transform.SetParent(prefabRoot.transform, false);
            artRoot = art.transform;
            spriteRenderer = art.AddComponent<SpriteRenderer>();
            animator = art.AddComponent<Animator>();
        }

        spriteRenderer.sprite = firstSprite;
        spriteRenderer.sortingOrder = 500;
        animator.runtimeAnimatorController = controller;
        artRoot.localScale = new Vector3(0.08f, 0.08f, 1f);

        SerializedObject serializedManifestation = new SerializedObject(manifestation);
        serializedManifestation.FindProperty("animator").objectReferenceValue = animator;
        serializedManifestation.FindProperty("artRoot").objectReferenceValue = artRoot;
        serializedManifestation.FindProperty("shellRoot").objectReferenceValue = artRoot;
        serializedManifestation.FindProperty("summonVerticalOffset").floatValue = 1.1f;
        serializedManifestation.FindProperty("summonScale").floatValue = 0.105f;
        serializedManifestation.FindProperty("renderPlaneOffsetZ").floatValue = -8f;
        SerializedProperty glowProperty = serializedManifestation.FindProperty("glowRenderers");
        glowProperty.arraySize = 1;
        glowProperty.GetArrayElementAtIndex(0).objectReferenceValue = spriteRenderer;
        serializedManifestation.ApplyModifiedPropertiesWithoutUndo();
        artRoot.localScale = new Vector3(0.105f, 0.105f, 1f);

        if (File.Exists(ToAbsolutePath(PrefabPath)))
        {
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        Object.DestroyImmediate(prefabRoot);
        return prefab;
    }

    private static void AssignPrefabToProfileAndSystems(CompanionManifestationController manifestationPrefab)
    {
        if (manifestationPrefab == null)
            return;

        CompanionProfile profile = AssetDatabase.LoadAssetAtPath<CompanionProfile>(ProfilePath);
        CompanionDialogueLibrary library = AssetDatabase.LoadAssetAtPath<CompanionDialogueLibrary>(DialoguePath);

        CompanionSystem[] systems = Object.FindObjectsByType<CompanionSystem>(FindObjectsSortMode.None);
        for (int i = 0; i < systems.Length; i++)
        {
            if (systems[i] == null)
                continue;

            Undo.RecordObject(systems[i], "Assign Melc Companion Prefab");
            systems[i].AssignAuthoringAssets(profile, library, manifestationPrefab);
            EditorUtility.SetDirty(systems[i]);
        }
    }

    private static AnimatorState FindOrCreateState(AnimatorStateMachine machine, string stateName, Vector3 position)
    {
        ChildAnimatorState[] states = machine.states;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].state != null && states[i].state.name == stateName)
                return states[i].state;
        }

        return machine.AddState(stateName, position);
    }

    private static bool HasTransition(AnimatorState from, AnimatorState to)
    {
        for (int i = 0; i < from.transitions.Length; i++)
        {
            if (from.transitions[i] != null && from.transitions[i].destinationState == to)
                return true;
        }

        return false;
    }

    private static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        for (int i = 0; i < controller.parameters.Length; i++)
        {
            if (controller.parameters[i].name == name)
                return;
        }

        controller.AddParameter(name, type);
    }

    private static void DeleteGeneratedAsset(string assetPath)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(assetPath) != null)
            AssetDatabase.DeleteAsset(assetPath);
    }

    private static void EnsureFolder(string assetFolderPath)
    {
        if (AssetDatabase.IsValidFolder(assetFolderPath))
            return;

        string parent = Path.GetDirectoryName(assetFolderPath)?.Replace("\\", "/");
        string folderName = Path.GetFileName(assetFolderPath);

        if (!string.IsNullOrWhiteSpace(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, folderName);
    }

    private static int GetFrameIndex(string assetPath)
    {
        string fileName = Path.GetFileNameWithoutExtension(assetPath);
        string digits = new string(fileName.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out int index) ? index : int.MaxValue;
    }

    private static string ToAbsolutePath(string assetPath)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), assetPath.Replace("/", "\\"));
    }

    private static string ToAssetPath(string absolutePath)
    {
        string normalized = absolutePath.Replace("\\", "/");
        string projectRoot = Directory.GetCurrentDirectory().Replace("\\", "/") + "/";
        return normalized.StartsWith(projectRoot) ? normalized.Substring(projectRoot.Length) : normalized;
    }
}
