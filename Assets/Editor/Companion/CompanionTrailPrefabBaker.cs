using UnityEditor;
using UnityEngine;

public static class CompanionTrailPrefabBaker
{
    private const string PrefabPath = "Assets/ROAE_2/Prefabs/Companion/CompanionManifestation.prefab";
    private const string SessionKey = "ROAE.CompanionTrailPrefabBaker.Baked";

    [InitializeOnLoadMethod]
    private static void ScheduleAutoBake()
    {
        EditorApplication.delayCall += TryBakeOnce;
    }

    [MenuItem("ROAE/Companion/Bake Trail Into Companion Prefab")]
    public static void BakeTrailIntoCompanionPrefab()
    {
        BakeInternal(force: true);
    }

    private static void TryBakeOnce()
    {
        if (SessionState.GetBool(SessionKey, false))
            return;

        SessionState.SetBool(SessionKey, true);
        BakeInternal(force: false);
    }

    private static void BakeInternal(bool force)
    {
        GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefabRoot == null)
            return;

        GameObject loadedRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
        CompanionManifestationController controller = loadedRoot.GetComponent<CompanionManifestationController>();
        if (controller == null)
        {
            PrefabUtility.UnloadPrefabContents(loadedRoot);
            return;
        }

        Transform trailTransform = loadedRoot.transform.Find("TrailParticles");
        bool created = false;
        if (trailTransform == null)
        {
            GameObject trailObject = new GameObject("TrailParticles");
            trailObject.transform.SetParent(loadedRoot.transform, false);
            trailTransform = trailObject.transform;
            created = true;
        }

        trailTransform.localPosition = new Vector3(-0.18f, -0.1f, 0f);
        trailTransform.localRotation = Quaternion.identity;
        trailTransform.localScale = Vector3.one;

        ParticleSystem particleSystem = trailTransform.GetComponent<ParticleSystem>();
        if (particleSystem == null)
            particleSystem = trailTransform.gameObject.AddComponent<ParticleSystem>();

        ParticleSystemRenderer renderer = trailTransform.GetComponent<ParticleSystemRenderer>();
        if (renderer == null)
            renderer = trailTransform.gameObject.AddComponent<ParticleSystemRenderer>();

        ConfigureTrail(particleSystem, renderer);

        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("trailParticles").objectReferenceValue = particleSystem;
        serializedController.FindProperty("trailLocalOffset").vector2Value = new Vector2(-0.18f, -0.1f);
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        if (created || force)
        {
            PrefabUtility.SaveAsPrefabAsset(loadedRoot, PrefabPath);
            AssetDatabase.SaveAssets();
        }

        PrefabUtility.UnloadPrefabContents(loadedRoot);
    }

    private static void ConfigureTrail(ParticleSystem particleSystem, ParticleSystemRenderer renderer)
    {
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = particleSystem.main;
        main.loop = true;
        main.playOnAwake = false;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.3f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.04f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.24f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.62f, 0.9f, 1f, 0.5f));
        main.maxParticles = 256;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.gravityModifier = 0f;

        var emission = particleSystem.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.rateOverDistance = 45f;

        var shape = particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.12f;
        shape.radiusThickness = 0.7f;

        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.62f, 0.92f, 1f), 0f),
                new GradientColorKey(new Color(0.78f, 0.58f, 0.95f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.48f, 0f),
                new GradientAlphaKey(0.22f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.8f),
            new Keyframe(0.55f, 1f),
            new Keyframe(1f, 0.15f));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var noise = particleSystem.noise;
        noise.enabled = true;
        noise.strength = 0.06f;
        noise.frequency = 0.3f;
        noise.scrollSpeed = 0.12f;

        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        velocityOverLifetime.enabled = false;

        renderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortMode = ParticleSystemSortMode.Distance;
        renderer.sortingOrder = 499;

        particleSystem.Clear(true);
    }
}
