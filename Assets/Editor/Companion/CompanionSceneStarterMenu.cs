using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CompanionSceneStarterMenu
{
    [MenuItem("ROAE/Companion/Create Quiet Safe Summon Cluster")]
    public static void CreateQuietSafeCluster()
    {
        CreateCluster(
            rootName: "Companion_QuietSafeCluster",
            pointId: "quiet_space_summon",
            zoneId: "quiet_space_zone",
            pointType: CompanionSummonPointType.QuietSpace,
            zoneScale: new Vector3(4.5f, 3f, 1f),
            summonOffset: new Vector3(0f, -0.35f, 0f),
            observationOffset: new Vector3(1.2f, 0.35f, 0f),
            zoneTags: new[] { CompanionEnvironmentTag.Shelter, CompanionEnvironmentTag.Silence, CompanionEnvironmentTag.Warmth },
            summonTags: new[] { CompanionEnvironmentTag.Shelter, CompanionEnvironmentTag.Silence, CompanionEnvironmentTag.Warmth },
            observationTags: new[] { CompanionEnvironmentTag.Loneliness, CompanionEnvironmentTag.Warmth });
    }

    [MenuItem("ROAE/Companion/Create Mirror Summon Cluster")]
    public static void CreateMirrorCluster()
    {
        CreateCluster(
            rootName: "Companion_MirrorCluster",
            pointId: "mirror_summon",
            zoneId: "mirror_zone",
            pointType: CompanionSummonPointType.Mirror,
            zoneScale: new Vector3(3f, 4f, 1f),
            summonOffset: new Vector3(0f, -0.5f, 0f),
            observationOffset: new Vector3(0.9f, 0.15f, 0f),
            zoneTags: new[] { CompanionEnvironmentTag.Reflection, CompanionEnvironmentTag.Falsehood, CompanionEnvironmentTag.Shelter },
            summonTags: new[] { CompanionEnvironmentTag.Reflection, CompanionEnvironmentTag.Falsehood },
            observationTags: new[] { CompanionEnvironmentTag.Reflection, CompanionEnvironmentTag.Falsehood, CompanionEnvironmentTag.Memory });
    }

    private static void CreateCluster(
        string rootName,
        string pointId,
        string zoneId,
        CompanionSummonPointType pointType,
        Vector3 zoneScale,
        Vector3 summonOffset,
        Vector3 observationOffset,
        CompanionEnvironmentTag[] zoneTags,
        CompanionEnvironmentTag[] summonTags,
        CompanionEnvironmentTag[] observationTags)
    {
        CompanionAuthoringMenu.CreateStarterAssets();
        CompanionAuthoringMenu.CreateRuntimeHostInScene();

        GameObject root = new GameObject(rootName);
        Undo.RegisterCreatedObjectUndo(root, "Create Companion Cluster");

        GameObject zone = new GameObject("EnvironmentZone");
        zone.transform.SetParent(root.transform, false);
        zone.transform.localScale = zoneScale;
        BoxCollider2D zoneCollider = zone.AddComponent<BoxCollider2D>();
        zoneCollider.isTrigger = true;
        CompanionEnvironmentZone zoneComponent = zone.AddComponent<CompanionEnvironmentZone>();
        ConfigureEnvironmentZone(zoneComponent, zoneId, zoneTags);

        GameObject summonPoint = new GameObject("SummonPoint");
        summonPoint.transform.SetParent(root.transform, false);
        summonPoint.transform.localPosition = summonOffset;
        CircleCollider2D summonCollider = summonPoint.AddComponent<CircleCollider2D>();
        summonCollider.isTrigger = true;
        summonCollider.radius = 0.9f;
        CompanionSummonPoint summonComponent = summonPoint.AddComponent<CompanionSummonPoint>();
        ConfigureSummonPoint(summonComponent, pointId, pointType, summonTags);

        GameObject anchor = new GameObject("Anchor");
        anchor.transform.SetParent(summonPoint.transform, false);
        anchor.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        AssignObjectReference(summonComponent, "anchor", anchor.transform);

        GameObject observation = new GameObject("ObservationTarget");
        observation.transform.SetParent(root.transform, false);
        observation.transform.localPosition = observationOffset;
        CompanionObservationTarget observationComponent = observation.AddComponent<CompanionObservationTarget>();
        ConfigureObservationTarget(observationComponent, pointId + "_observation", observationTags);

        GameObject focus = new GameObject("FocusPoint");
        focus.transform.SetParent(observation.transform, false);
        focus.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        AssignObjectReference(observationComponent, "focusPoint", focus.transform);

        GameObject bridge = new GameObject("ActionBridge");
        bridge.transform.SetParent(root.transform, false);
        CompanionActionBridge bridgeComponent = bridge.AddComponent<CompanionActionBridge>();
        ConfigureActionBridge(bridgeComponent, summonComponent, observationComponent);

        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[ROAE][CompanionSceneStarterMenu] Created cluster: " + rootName);
    }

    private static void ConfigureSummonPoint(
        CompanionSummonPoint point,
        string pointId,
        CompanionSummonPointType pointType,
        CompanionEnvironmentTag[] tags)
    {
        SerializedObject serialized = new SerializedObject(point);
        serialized.FindProperty("pointId").stringValue = pointId;
        serialized.FindProperty("pointType").enumValueIndex = (int)pointType;
        serialized.FindProperty("isEmotionallySafeSpace").boolValue = true;
        serialized.FindProperty("allowManualSummon").boolValue = true;
        serialized.FindProperty("autoSummonOnPlayerEnter").boolValue = true;
        serialized.FindProperty("autoSpeakOnSummon").boolValue = true;
        serialized.FindProperty("ignoreCorruptionRestrictionForTesting").boolValue = true;
        serialized.FindProperty("requirePlayerInsideTrigger").boolValue = true;
        serialized.FindProperty("playerTag").stringValue = "Player";
        serialized.FindProperty("ambientThreatLevel").enumValueIndex = (int)CompanionThreatLevel.None;
        SetEnumArray(serialized.FindProperty("ambientTags"), tags);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(point);
    }

    private static void ConfigureEnvironmentZone(
        CompanionEnvironmentZone zone,
        string zoneId,
        CompanionEnvironmentTag[] tags)
    {
        SerializedObject serialized = new SerializedObject(zone);
        serialized.FindProperty("zoneId").stringValue = zoneId;
        serialized.FindProperty("contributesSafeSpace").boolValue = true;
        serialized.FindProperty("playerTag").stringValue = "Player";
        serialized.FindProperty("threatLevel").enumValueIndex = (int)CompanionThreatLevel.None;
        SetEnumArray(serialized.FindProperty("tags"), tags);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(zone);
    }

    private static void ConfigureObservationTarget(
        CompanionObservationTarget observation,
        string targetId,
        CompanionEnvironmentTag[] tags)
    {
        SerializedObject serialized = new SerializedObject(observation);
        serialized.FindProperty("targetId").stringValue = targetId;
        serialized.FindProperty("hiddenThing").boolValue = true;
        serialized.FindProperty("requestHintOnObserve").boolValue = true;
        serialized.FindProperty("oneShotHint").boolValue = false;
        serialized.FindProperty("pulseStrength").floatValue = 0.8f;
        serialized.FindProperty("discoveryId").stringValue = targetId;
        SetEnumArray(serialized.FindProperty("tags"), tags);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(observation);
    }

    private static void ConfigureActionBridge(
        CompanionActionBridge bridge,
        CompanionSummonPoint summonPoint,
        CompanionObservationTarget observation)
    {
        SerializedObject serialized = new SerializedObject(bridge);
        serialized.FindProperty("summonPoint").objectReferenceValue = summonPoint;
        serialized.FindProperty("observationTarget").objectReferenceValue = observation;
        serialized.FindProperty("intent").enumValueIndex = (int)CompanionDialogueIntent.Hint;
        SetEnumArray(serialized.FindProperty("extraTags"), new[] { CompanionEnvironmentTag.Shelter, CompanionEnvironmentTag.Warmth });
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(bridge);
    }

    private static void AssignObjectReference(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.FindProperty(propertyName).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetEnumArray(SerializedProperty property, CompanionEnvironmentTag[] values)
    {
        property.arraySize = values != null ? values.Length : 0;
        if (values == null)
            return;

        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).enumValueIndex = (int)values[i];
        }
    }
}
