using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BarInteriorLightingRig : MonoBehaviour
{
    [Header("Scene Lighting")]
    [SerializeField] private bool applyLitMaterialToSceneSprites = false;
    [SerializeField] private Color ambientLightColor = new Color(0.72f, 0.62f, 0.46f, 1f);
    [SerializeField] private float ambientLightIntensity = 0.38f;

    [Header("Lamp Glow")]
    [SerializeField] private string lampPrefix = "Licori_";
    [SerializeField] private Color lampLightColor = new Color(0.29f, 0.95f, 1f, 1f);
    [SerializeField] private float lampLightIntensity = 0.9f;
    [SerializeField] private float lampOuterRadius = 2.25f;
    [SerializeField] private float lampInnerRadius = 0.45f;
    [SerializeField] private float glowScale = 1.45f;
    [SerializeField] private float glowAlpha = 0.5f;
    [SerializeField] private float coreGlowScale = 1.15f;
    [SerializeField] private float coreGlowAlpha = 0.8f;
    [SerializeField] private bool useAdditiveGlowMaterial = true;
    [SerializeField] private float haloIntensityMultiplier = 1.4f;
    [SerializeField] private float coreIntensityMultiplier = 2.1f;

    [Header("Pulse")]
    [SerializeField] private bool pulseEnabled = true;
    [SerializeField] private float pulseSpeed = 1.35f;
    [SerializeField] private float pulseAmount = 0.14f;

    [Header("Post Processing")]
    [SerializeField] private bool addRuntimeVolume = true;
    [SerializeField] private float bloomThreshold = 0.82f;
    [SerializeField] private float bloomIntensity = 2.6f;
    [SerializeField] private float vignetteIntensity = 0.16f;
    [SerializeField] private bool allowHdrOnCamera = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private readonly List<LampRuntimeData> lampRuntimeData = new List<LampRuntimeData>();

    private Material litSpriteMaterial;
    private Material unlitSpriteMaterial;
    private Material additiveGlowMaterial;
    private Volume runtimeVolume;
    private VolumeProfile runtimeProfile;
    private float nextDiagnosticLogTime;

    private void Awake()
    {
        EnsureMaterials();

        bool canUseLitSprites = CanUse2DLightingSafely();

        if (applyLitMaterialToSceneSprites && canUseLitSprites)
            ApplyLitMaterialToSceneSprites();

        if (canUseLitSprites)
            EnsureGlobalLight();

        EnsureLampSetups();

        if (addRuntimeVolume)
            EnsureRuntimeVolume();

        nextDiagnosticLogTime = Time.time + 1f;
        LogSceneDiagnostics(canUseLitSprites);
    }

    private void Update()
    {
        if (!pulseEnabled || lampRuntimeData.Count == 0)
            return;

        float time = Time.time * pulseSpeed;
        for (int i = 0; i < lampRuntimeData.Count; i++)
        {
            LampRuntimeData data = lampRuntimeData[i];
            if (data == null)
                continue;

            float pulse = 1f + Mathf.Sin(time + data.phaseOffset) * pulseAmount;
            if (data.light != null)
                data.light.intensity = lampLightIntensity * pulse;

            if (data.halo != null)
                data.halo.color = GetGlowColor(glowAlpha * pulse, haloIntensityMultiplier);

            if (data.core != null)
                data.core.color = GetGlowColor(coreGlowAlpha * pulse, coreIntensityMultiplier);
        }

        if (debugLogs && Time.time >= nextDiagnosticLogTime)
        {
            nextDiagnosticLogTime = Time.time + 2.5f;
            LogPulseSnapshot();
        }
    }

    private void EnsureMaterials()
    {
        if (litSpriteMaterial == null)
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
            if (litShader != null)
                litSpriteMaterial = new Material(litShader) { name = "BarInterior_RuntimeSpriteLit" };
        }

        if (unlitSpriteMaterial == null)
        {
            Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (unlitShader != null)
                unlitSpriteMaterial = new Material(unlitShader) { name = "BarInterior_RuntimeSpriteUnlit" };
        }

        if (additiveGlowMaterial == null)
        {
            Shader additiveShader =
                Shader.Find("Legacy Shaders/Particles/Additive") ??
                Shader.Find("Particles/Additive") ??
                Shader.Find("Sprites/Default");

            if (additiveShader != null)
                additiveGlowMaterial = new Material(additiveShader) { name = "BarInterior_RuntimeGlowAdditive" };
        }

        Log(
            "EnsureMaterials | lit=" + (litSpriteMaterial != null) +
            " | unlit=" + (unlitSpriteMaterial != null) +
            " | additive=" + (additiveGlowMaterial != null ? additiveGlowMaterial.shader.name : "missing"));
    }

    private void ApplyLitMaterialToSceneSprites()
    {
        if (litSpriteMaterial == null)
        {
            Log("Sprite-Lit shader not found. Skipping scene material pass.");
            return;
        }

        SpriteRenderer[] spriteRenderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer == null || renderer.transform == null)
                continue;

            if (!renderer.gameObject.scene.IsValid() || renderer.gameObject.scene.name != "Bar_Interior")
                continue;

            if (renderer.transform.IsChildOf(transform) && renderer.gameObject.name.StartsWith("Glow_", System.StringComparison.Ordinal))
                continue;

            renderer.sharedMaterial = litSpriteMaterial;
        }

        Log("Applied Sprite-Lit material to scene SpriteRenderers.");
    }

    private void EnsureGlobalLight()
    {
        Light2D globalLight = FindOrCreateChildLight("BarInterior_GlobalLight", Light2D.LightType.Global);
        if (globalLight == null)
            return;

        globalLight.color = ambientLightColor;
        globalLight.intensity = ambientLightIntensity;
    }

    private void EnsureLampSetups()
    {
        lampRuntimeData.Clear();

        foreach (Transform child in transform)
        {
            if (child == null || !child.name.StartsWith(lampPrefix, System.StringComparison.Ordinal))
                continue;

            SpriteRenderer sourceRenderer = child.GetComponent<SpriteRenderer>();
            if (sourceRenderer == null || sourceRenderer.sprite == null)
                continue;

            Light2D pointLight = FindOrCreateChildLight(child, "LampLight2D", Light2D.LightType.Point);
            if (pointLight != null)
            {
                pointLight.color = lampLightColor;
                pointLight.intensity = lampLightIntensity;
                pointLight.pointLightOuterRadius = lampOuterRadius;
                pointLight.pointLightInnerRadius = lampInnerRadius;
                pointLight.falloffIntensity = 0.65f;
                pointLight.shadowIntensity = 0f;
            }

            SpriteRenderer halo = FindOrCreateGlowSprite(
                child,
                sourceRenderer,
                "Glow_Halo",
                glowScale,
                glowAlpha,
                sourceRenderer.sortingOrder + 1,
                haloIntensityMultiplier);
            SpriteRenderer core = FindOrCreateGlowSprite(
                child,
                sourceRenderer,
                "Glow_Core",
                coreGlowScale,
                coreGlowAlpha,
                sourceRenderer.sortingOrder + 2,
                coreIntensityMultiplier);

            lampRuntimeData.Add(new LampRuntimeData
            {
                name = child.name,
                source = sourceRenderer,
                light = pointLight,
                halo = halo,
                core = core,
                phaseOffset = child.GetSiblingIndex() * 0.7f
            });

            Log(
                "LampSetup | name=" + child.name +
                " | sourceOrder=" + sourceRenderer.sortingOrder +
                " | haloOrder=" + (halo != null ? halo.sortingOrder : -1) +
                " | coreOrder=" + (core != null ? core.sortingOrder : -1) +
                " | lightCreated=" + (pointLight != null) +
                " | sprite=" + sourceRenderer.sprite.name);
        }

        Log("Configured lamp glow for " + lampRuntimeData.Count + " Licori sprites.");
    }

    private void EnsureRuntimeVolume()
    {
        Camera targetCamera = Camera.main;
        if (targetCamera != null)
        {
            if (allowHdrOnCamera)
                targetCamera.allowHDR = true;

            UniversalAdditionalCameraData cameraData = targetCamera.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData != null)
                cameraData.renderPostProcessing = true;
        }

        if (runtimeVolume == null)
        {
            Transform volumeTransform = transform.Find("BarInterior_RuntimeVolume");
            GameObject volumeObject = volumeTransform != null ? volumeTransform.gameObject : new GameObject("BarInterior_RuntimeVolume");
            if (volumeTransform == null)
                volumeObject.transform.SetParent(transform, false);

            runtimeVolume = volumeObject.GetComponent<Volume>();
            if (runtimeVolume == null)
                runtimeVolume = volumeObject.AddComponent<Volume>();
        }

        if (runtimeProfile == null)
        {
            runtimeProfile = ScriptableObject.CreateInstance<VolumeProfile>();

            Bloom bloom = runtimeProfile.Add<Bloom>(true);
            bloom.threshold.Override(bloomThreshold);
            bloom.intensity.Override(bloomIntensity);
            bloom.scatter.Override(0.92f);

            Vignette vignette = runtimeProfile.Add<Vignette>(true);
            vignette.intensity.Override(vignetteIntensity);
            vignette.smoothness.Override(0.72f);
        }

        runtimeVolume.isGlobal = true;
        runtimeVolume.priority = 5f;
        runtimeVolume.weight = 1f;
        runtimeVolume.sharedProfile = runtimeProfile;
    }

    private Light2D FindOrCreateChildLight(string objectName, Light2D.LightType lightType)
    {
        return FindOrCreateChildLight(transform, objectName, lightType);
    }

    private Light2D FindOrCreateChildLight(Transform parent, string objectName, Light2D.LightType lightType)
    {
        Transform existing = parent.Find(objectName);
        GameObject lightObject = existing != null ? existing.gameObject : new GameObject(objectName);
        if (existing == null)
            lightObject.transform.SetParent(parent, false);

        Light2D light = lightObject.GetComponent<Light2D>();
        if (light == null)
            light = lightObject.AddComponent<Light2D>();

        light.lightType = lightType;
        return light;
    }

    private SpriteRenderer FindOrCreateGlowSprite(
        Transform parent,
        SpriteRenderer sourceRenderer,
        string objectName,
        float targetScale,
        float targetAlpha,
        int sortingOrder,
        float intensityMultiplier)
    {
        Transform existing = parent.Find(objectName);
        GameObject glowObject = existing != null ? existing.gameObject : new GameObject(objectName);
        if (existing == null)
            glowObject.transform.SetParent(parent, false);

        SpriteRenderer glowRenderer = glowObject.GetComponent<SpriteRenderer>();
        if (glowRenderer == null)
            glowRenderer = glowObject.AddComponent<SpriteRenderer>();

        glowRenderer.sprite = sourceRenderer.sprite;
        glowRenderer.sharedMaterial = useAdditiveGlowMaterial && additiveGlowMaterial != null
            ? additiveGlowMaterial
            : (unlitSpriteMaterial != null ? unlitSpriteMaterial : sourceRenderer.sharedMaterial);
        glowRenderer.color = GetGlowColor(targetAlpha, intensityMultiplier);
        glowRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        glowRenderer.sortingOrder = sortingOrder;

        glowObject.transform.localPosition = Vector3.zero;
        glowObject.transform.localRotation = Quaternion.identity;
        glowObject.transform.localScale = Vector3.one * targetScale;

        return glowRenderer;
    }

    private Color GetGlowColor(float alpha, float intensityMultiplier)
    {
        Color color = lampLightColor * intensityMultiplier;
        color.a = alpha;
        return color;
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log("[ROAE][BarInteriorLightingRig] " + message);
    }

    private void LogSceneDiagnostics(bool canUseLitSprites)
    {
        Camera targetCamera = Camera.main;
        UniversalAdditionalCameraData cameraData = targetCamera != null ? targetCamera.GetComponent<UniversalAdditionalCameraData>() : null;
        string rendererName = cameraData != null && cameraData.scriptableRenderer != null
            ? cameraData.scriptableRenderer.GetType().Name
            : "missing";

        Log(
            "Diagnostics | canUse2DLighting=" + canUseLitSprites +
            " | lampCount=" + lampRuntimeData.Count +
            " | mainCamera=" + (targetCamera != null ? targetCamera.name : "missing") +
            " | allowHDR=" + (targetCamera != null && targetCamera.allowHDR) +
            " | renderer=" + rendererName +
            " | runtimeVolume=" + (runtimeVolume != null) +
            " | bloomIntensity=" + bloomIntensity);
    }

    private void LogPulseSnapshot()
    {
        for (int i = 0; i < lampRuntimeData.Count; i++)
        {
            LampRuntimeData data = lampRuntimeData[i];
            if (data == null)
                continue;

            Log(
                "Pulse | name=" + data.name +
                " | lightIntensity=" + (data.light != null ? data.light.intensity.ToString("0.00") : "n/a") +
                " | haloAlpha=" + (data.halo != null ? data.halo.color.a.ToString("0.00") : "n/a") +
                " | coreAlpha=" + (data.core != null ? data.core.color.a.ToString("0.00") : "n/a") +
                " | haloShader=" + (data.halo != null && data.halo.sharedMaterial != null ? data.halo.sharedMaterial.shader.name : "missing") +
                " | sourceEnabled=" + (data.source != null && data.source.enabled));
        }
    }

    private bool CanUse2DLightingSafely()
    {
        Camera targetCamera = Camera.main;
        if (targetCamera == null)
        {
            Log("No main camera found. Falling back to unlit lamp glow only.");
            return false;
        }

        UniversalAdditionalCameraData cameraData = targetCamera.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null || cameraData.scriptableRenderer == null)
        {
            Log("Camera has no URP additional data / renderer. Falling back to unlit lamp glow only.");
            return false;
        }

        string rendererTypeName = cameraData.scriptableRenderer.GetType().Name;
        bool is2DRenderer = rendererTypeName.Contains("Renderer2D", System.StringComparison.Ordinal);
        if (!is2DRenderer)
            Log("Active renderer is '" + rendererTypeName + "', not a 2D renderer. Skipping scene-wide lit sprite pass.");

        return is2DRenderer;
    }

    [System.Serializable]
    private sealed class LampRuntimeData
    {
        public string name;
        public SpriteRenderer source;
        public Light2D light;
        public SpriteRenderer halo;
        public SpriteRenderer core;
        public float phaseOffset;
    }
}
