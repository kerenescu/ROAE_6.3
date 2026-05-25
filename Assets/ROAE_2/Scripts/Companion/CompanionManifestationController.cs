using UnityEngine;
using System.Collections;

public class CompanionManifestationController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform artRoot;
    [SerializeField] private Transform shellRoot;
    [SerializeField] private SpriteRenderer[] glowRenderers;
    [SerializeField] private ParticleSystem summonParticles;
    [SerializeField] private ParticleSystem pulseParticles;
    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private CompanionAudioController audioController;

    [Header("Motion")]
    [SerializeField] private float hoverAmplitude = 0.08f;
    [SerializeField] private float hoverSpeed = 1.8f;
    [SerializeField] private float followLerp = 10f;
    [SerializeField] private float summonVerticalOffset = 1.1f;
    [SerializeField] private float summonScale = 0.105f;
    [SerializeField] private float renderPlaneOffsetZ = -8f;
    [SerializeField] private float shellHideScaleMultiplier = 0.92f;

    [SerializeField] private bool followPlayer = true;
    [SerializeField] private Vector2 playerOffset = new Vector2(1.2f, 0.4f);
    [SerializeField] private bool mirrorFollowOffsetXFromPlayerMotion = true;
    [SerializeField] private bool facePlayerWhenSideSwitchCompletes = true;
    [SerializeField] private bool orbitAcrossHeadOnSideSwitch = true;
    [SerializeField] private float sideSwitchArcHeight = 1.25f;
    [SerializeField] private float sideSwitchDuration = 0.45f;

    [Header("Trail")]
    [SerializeField] private bool overrideTrailAnchorPosition = false;
    [SerializeField] private Vector2 trailLocalOffset = new Vector2(-0.18f, -0.1f);
    [SerializeField] private bool mirrorTrailXFromPlayerMotion = true;
    [SerializeField] private float playerMotionDirectionThreshold = 0.0005f;

    private Vector3 targetPosition;
    private Vector3 baseScale = Vector3.one;
    private bool shellHidden;
    private float emotionalIntensity = 0.35f;
    private SpriteRenderer artRenderer;
    private SpriteRenderer cachedPlayerRenderer;
    private Transform cachedPlayerTransform;
    private Coroutine freezeCoroutine;
    private Vector2 basePlayerOffset;
    private Vector3 baseTrailLocalPosition;
    private Vector3 previousPlayerWorldPosition;
    private bool hasPreviousPlayerWorldPosition;
    private float lastPlayerHorizontalDirection = -1f;
    private Vector2 currentFollowOffset;
    private float orbitStartSideSign = 1f;
    private float orbitTargetSideSign = 1f;
    private float orbitProgress = 1f;
    private Color currentTrailEmotionColor = new Color(0.8f, 0.92f, 1f, 0.32f);
    private CompanionEmotionalState lastLoggedTrailState;
    private bool hasLoggedTrailState;
    private bool uiObscured;

    private static readonly int SummonTrigger = Animator.StringToHash("Summon");
    private static readonly int DespawnTrigger = Animator.StringToHash("Despawn");
    private static readonly int MoodHash = Animator.StringToHash("Mood");
    private static readonly int ShellHash = Animator.StringToHash("Shell");

    private void Awake()
    {
        if (artRoot == null)
            artRoot = transform;

        artRoot.localScale = new Vector3(summonScale, summonScale, 1f);
        baseScale = artRoot.localScale;
        targetPosition = transform.position;
        basePlayerOffset = playerOffset;
        currentFollowOffset = new Vector2(Mathf.Abs(basePlayerOffset.x), basePlayerOffset.y);
        artRenderer = artRoot.GetComponent<SpriteRenderer>();

        if (artRenderer != null)
        {
            artRenderer.color = Color.white;
            artRenderer.sortingOrder = Mathf.Max(artRenderer.sortingOrder, 500);
        }

        if (trailParticles != null)
            baseTrailLocalPosition = trailParticles.transform.localPosition;

        CachePlayerPresentation();
        SyncPresentationToPlayer();
        ResetBodyTint();
        ApplyTrailColor(currentTrailEmotionColor, 1f);
    }

    public void AttachTo(Vector3 worldPosition)
    {
        CachePlayerPresentation();
        SyncPresentationToPlayer();

        Vector3 adjustedPosition;

        if (followPlayer && cachedPlayerTransform != null)
        {
            Vector2 effectiveOffset = GetEffectivePlayerOffset();
            adjustedPosition = cachedPlayerTransform.position + new Vector3(effectiveOffset.x, effectiveOffset.y, 0f);
        }
        else
        {
            adjustedPosition = worldPosition;
            adjustedPosition.y += summonVerticalOffset;
        }

        adjustedPosition.z = cachedPlayerTransform != null
            ? cachedPlayerTransform.position.z + renderPlaneOffsetZ
            : renderPlaneOffsetZ;

        targetPosition = adjustedPosition;
        transform.position = adjustedPosition;
    }

    public bool TrySnapToCurrentPlayer(bool forceRefresh = false)
    {
        CachePlayerPresentation(forceRefresh);
        SyncPresentationToPlayer();

        if (!followPlayer || cachedPlayerTransform == null)
            return false;

        Vector2 effectiveOffset = GetEffectivePlayerOffset();
        Vector3 adjustedPosition = cachedPlayerTransform.position + new Vector3(effectiveOffset.x, effectiveOffset.y, 0f);
        adjustedPosition.z = cachedPlayerTransform.position.z + renderPlaneOffsetZ;

        targetPosition = adjustedPosition;
        transform.position = adjustedPosition;
        previousPlayerWorldPosition = cachedPlayerTransform.position;
        hasPreviousPlayerWorldPosition = true;
        return true;
    }

    public void PlaySummon()
    {
        Debug.Log("[ROAE][CompanionManifestation] PlaySummon start object=" + gameObject.name);
        gameObject.SetActive(true);
        if (animator != null)
        {
            animator.enabled = true;
            animator.SetTrigger(SummonTrigger);
        }

        if (summonParticles != null)
            summonParticles.Play();

        if (trailParticles != null)
        {
            trailParticles.Clear();
            trailParticles.Play();
        }

        if (audioController != null)
            audioController.PlaySummon();

        SetShellHidden(false);

        if (freezeCoroutine != null)
            StopCoroutine(freezeCoroutine);

        freezeCoroutine = StartCoroutine(FreezeAfterSummon());
        ApplyUiObscuredState();
        Debug.Log("[ROAE][CompanionManifestation] PlaySummon end active=" + gameObject.activeInHierarchy);
    }

    public void PlayDespawn()
    {
        if (animator != null)
        {
            if (freezeCoroutine != null)
            {
                StopCoroutine(freezeCoroutine);
                freezeCoroutine = null;
            }

            animator.SetTrigger(DespawnTrigger);
        }

        if (trailParticles != null)
        {
            trailParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (audioController != null)
            audioController.PlayDespawn();

        gameObject.SetActive(false);
    }

    public void SetEmotion(CompanionEmotionalState state, float intensity)
    {
        emotionalIntensity = Mathf.Clamp01(intensity);
        if (animator != null)
            animator.SetInteger(MoodHash, (int)state);

        ResetBodyTint();
        ApplyTrailColorForState(state);

        if (audioController != null)
            audioController.SetEmotion(state);
    }

    public void FocusAt(Vector3 worldPosition)
    {
        Vector3 scale = artRoot.localScale;
        scale.x = Mathf.Abs(baseScale.x) * (worldPosition.x >= transform.position.x ? 1f : -1f);
        artRoot.localScale = scale;
    }

    public void SetShellHidden(bool hidden)
    {
        shellHidden = hidden;
        if (animator != null)
            animator.SetBool(ShellHash, hidden);

        if (shellRoot != null)
            ApplyShellScale(hidden ? shellHideScaleMultiplier : 1f);
    }

    public void Pulse(float strength)
    {
        float clamped = Mathf.Clamp01(strength);
        if (pulseParticles != null)
            pulseParticles.Play();

        if (audioController != null)
            audioController.PlayPulse(clamped);

        ResetBodyTint();
        ApplyTrailColor(currentTrailEmotionColor, 1f + (0.55f * clamped));
    }

    public void SetUiObscured(bool obscured)
    {
        if (uiObscured == obscured)
            return;

        uiObscured = obscured;
        ApplyUiObscuredState();
    }

    public void TickPresentation()
    {
        if (uiObscured)
            return;

        RefreshPlayerMotionDirection();
        UpdateFollowOffsetOrbit();

        Vector3 currentPosition = transform.position;
        Vector3 newPosition = Vector3.Lerp(currentPosition, targetPosition, Time.unscaledDeltaTime * followLerp);
        transform.position = newPosition;

        if (artRoot == null)
            return;

        float hover = Mathf.Sin(Time.unscaledTime * hoverSpeed) * hoverAmplitude * (shellHidden ? 0.45f : 1f);
        Vector3 localPosition = artRoot.localPosition;
        localPosition.y = hover;
        artRoot.localPosition = localPosition;

        UpdateFacingFromFollowSide();
        UpdateTrailMirrorFromPlayerMotion();

        if (overrideTrailAnchorPosition)
            UpdateTrailAnchor();
    }

    private void ApplyTrailColorForState(CompanionEmotionalState state)
    {
        Color color = Color.white;
        switch (state)
        {
            case CompanionEmotionalState.Healthy:
                color = new Color(0.96f, 0.34f, 0.34f, 0.42f + emotionalIntensity * 0.35f);
                break;

            case CompanionEmotionalState.Malicious:
                color = new Color(0.82f, 0.52f, 1f, 0.5f + emotionalIntensity * 0.35f);
                break;

            case CompanionEmotionalState.Sad:
                color = new Color(0.72f, 0.72f, 0.76f, 0.32f + emotionalIntensity * 0.25f);
                break;

            case CompanionEmotionalState.Numb:
                color = new Color(0.48f, 0.74f, 1f, 0.32f + emotionalIntensity * 0.25f);
                break;

            default:
                color = new Color(0.48f, 0.74f, 1f, 0.25f + emotionalIntensity * 0.2f);
                break;
        }

        currentTrailEmotionColor = color;
        ApplyTrailColor(currentTrailEmotionColor, 1f);

        if (ShouldLogTrailColorDecision(state))
        {
            Debug.Log(
                "[ROAE][CompanionManifestation] Trail color state=" + state +
                " reason=" + DescribeTrailColorReason(state) +
                " intensity=" + emotionalIntensity.ToString("0.00") +
                " rgba=(" +
                color.r.ToString("0.00") + "," +
                color.g.ToString("0.00") + "," +
                color.b.ToString("0.00") + "," +
                color.a.ToString("0.00") + ")");
        }
    }

    private void CachePlayerPresentation(bool forceRefresh = false)
    {
        if (!forceRefresh && cachedPlayerRenderer != null && cachedPlayerTransform != null)
            return;

        cachedPlayerRenderer = null;
        cachedPlayerTransform = null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        cachedPlayerTransform = player.transform;
        cachedPlayerRenderer = player.GetComponentInChildren<SpriteRenderer>();
        previousPlayerWorldPosition = cachedPlayerTransform.position;
        hasPreviousPlayerWorldPosition = true;
    }

    private void SyncPresentationToPlayer()
    {
        if (cachedPlayerRenderer == null)
            return;

        gameObject.layer = cachedPlayerRenderer.gameObject.layer;
        if (artRoot != null)
            artRoot.gameObject.layer = cachedPlayerRenderer.gameObject.layer;

        SpriteRenderer ownRenderer = artRoot != null ? artRoot.GetComponent<SpriteRenderer>() : null;
        if (ownRenderer == null)
            return;

        ownRenderer.sortingLayerID = cachedPlayerRenderer.sortingLayerID;
        ownRenderer.sortingOrder = Mathf.Max(cachedPlayerRenderer.sortingOrder + 50, 500);

        if (trailParticles != null)
        {
            ParticleSystemRenderer trailRenderer = trailParticles.GetComponent<ParticleSystemRenderer>();
            if (trailRenderer != null)
            {
                trailRenderer.sortingLayerID = ownRenderer.sortingLayerID;
                trailRenderer.sortingOrder = Mathf.Max(ownRenderer.sortingOrder - 1, 0);
            }
        }

        if (uiObscured)
            PushBehindModalUi();
    }

    private IEnumerator FreezeAfterSummon()
    {
        float duration = 2f;
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null && clips[i].name == "MelcSummon")
                {
                    duration = Mathf.Max(0.1f, clips[i].length);
                    break;
                }
            }
        }

        yield return new WaitForSeconds(duration);

        if (animator != null)
            animator.enabled = false;

        freezeCoroutine = null;
    }

    private void ApplyShellScale(float multiplier)
    {
        if (shellRoot == null)
            return;

        Vector3 currentScale = shellRoot.localScale;
        float xSign = Mathf.Sign(currentScale.x);
        if (Mathf.Approximately(xSign, 0f))
            xSign = 1f;

        shellRoot.localScale = new Vector3(
            xSign * Mathf.Abs(baseScale.x) * multiplier,
            Mathf.Abs(baseScale.y) * multiplier,
            Mathf.Abs(baseScale.z));
    }

    private void ResetBodyTint()
    {
        if (artRenderer != null)
            artRenderer.color = Color.white;
    }

    private void ApplyUiObscuredState()
    {
        SetSpriteVisibility(!uiObscured);
        SetTrailVisibility(!uiObscured);

        if (uiObscured)
        {
            PushBehindModalUi();
            return;
        }

        CachePlayerPresentation(true);
        SyncPresentationToPlayer();
        ResetBodyTint();
        ApplyTrailColor(currentTrailEmotionColor, 1f);
    }

    private void PushBehindModalUi()
    {
        if (artRenderer != null)
            artRenderer.sortingOrder = -1000;

        if (glowRenderers != null)
        {
            for (int i = 0; i < glowRenderers.Length; i++)
            {
                if (glowRenderers[i] != null)
                    glowRenderers[i].sortingOrder = -1001 - i;
            }
        }

        if (trailParticles != null)
        {
            ParticleSystemRenderer trailRenderer = trailParticles.GetComponent<ParticleSystemRenderer>();
            if (trailRenderer != null)
                trailRenderer.sortingOrder = -1100;
        }
    }

    private void SetSpriteVisibility(bool visible)
    {
        if (artRenderer != null)
            artRenderer.enabled = visible;

        if (glowRenderers == null)
            return;

        for (int i = 0; i < glowRenderers.Length; i++)
        {
            if (glowRenderers[i] != null)
                glowRenderers[i].enabled = visible;
        }
    }

    private void SetTrailVisibility(bool visible)
    {
        if (trailParticles == null)
            return;

        ParticleSystemRenderer trailRenderer = trailParticles.GetComponent<ParticleSystemRenderer>();
        if (trailRenderer != null)
            trailRenderer.enabled = visible;

        if (!visible)
        {
            trailParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return;
        }

        if (gameObject.activeInHierarchy)
        {
            trailParticles.Clear();
            trailParticles.Play();
        }
    }

    private void ApplyTrailColor(Color baseColor, float alphaMultiplier)
    {
        if (trailParticles == null)
            return;

        Color startColor = baseColor;
        startColor.a = Mathf.Clamp01(baseColor.a * alphaMultiplier);

        ParticleSystem.MainModule main = trailParticles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(startColor);

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = trailParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradient = new Gradient();
        Color midColor = baseColor;
        midColor.a = Mathf.Clamp01(baseColor.a * 0.45f * alphaMultiplier);

        Color endColor = baseColor;
        endColor.a = 0f;

        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(startColor, 0f),
                new GradientColorKey(midColor, 0.45f),
                new GradientColorKey(endColor, 1f)
            },
            new[]
            {
                new GradientAlphaKey(startColor.a, 0f),
                new GradientAlphaKey(midColor.a, 0.45f),
                new GradientAlphaKey(0f, 1f)
            });

        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    private bool ShouldLogTrailColorDecision(CompanionEmotionalState state)
    {
        CompanionSystem system = CompanionSystem.Instance;
        if (system == null || !system.DebugLoggingEnabled)
            return false;

        if (hasLoggedTrailState && lastLoggedTrailState == state)
            return false;

        hasLoggedTrailState = true;
        lastLoggedTrailState = state;
        return true;
    }

    private static string DescribeTrailColorReason(CompanionEmotionalState state)
    {
        switch (state)
        {
            case CompanionEmotionalState.Healthy:
                return "healthy inner state -> warm red trail";

            case CompanionEmotionalState.Malicious:
                return "malicious inner spiral -> sharp violet trail";

            case CompanionEmotionalState.Sad:
                return "sad inner state -> muted gray trail";

            case CompanionEmotionalState.Numb:
                return "numb inner state -> cold blue trail";

            default:
                return "fallback mood -> cold blue trail";
        }
    }

    private void ApplyFacingSign(float facingSign)
    {
        if (artRoot == null)
            return;

        Vector3 artScale = artRoot.localScale;
        float normalizedSign = Mathf.Sign(Mathf.Approximately(facingSign, 0f) ? 1f : facingSign);
        artScale.x = Mathf.Abs(baseScale.x) * normalizedSign;
        artRoot.localScale = artScale;

        if (shellRoot != null)
            ApplyShellScale(shellHidden ? shellHideScaleMultiplier : 1f);
    }

    private void UpdateTrailAnchor()
    {
        if (trailParticles == null)
            return;

        float facingSign = 1f;
        if (artRoot != null)
            facingSign = Mathf.Sign(artRoot.localScale.x);

        Vector3 anchoredOffset = new Vector3(trailLocalOffset.x * facingSign, trailLocalOffset.y, 0f);
        trailParticles.transform.localPosition = anchoredOffset;
    }

    private void UpdateTrailMirrorFromPlayerMotion()
    {
        if (!mirrorTrailXFromPlayerMotion || trailParticles == null)
            return;

        Vector3 localPosition = trailParticles.transform.localPosition;
        localPosition.y = baseTrailLocalPosition.y;
        localPosition.z = baseTrailLocalPosition.z;

        if (lastPlayerHorizontalDirection > 0f)
            localPosition.x = -baseTrailLocalPosition.x;
        else
            localPosition.x = baseTrailLocalPosition.x;

        trailParticles.transform.localPosition = localPosition;
    }

    private Vector2 GetEffectivePlayerOffset()
    {
        if (!mirrorFollowOffsetXFromPlayerMotion)
            return playerOffset;

        return currentFollowOffset;
    }

    private void RefreshPlayerMotionDirection()
    {
        if (cachedPlayerTransform == null)
            return;

        Vector3 currentPlayerWorldPosition = cachedPlayerTransform.position;
        if (!hasPreviousPlayerWorldPosition)
        {
            previousPlayerWorldPosition = currentPlayerWorldPosition;
            hasPreviousPlayerWorldPosition = true;
            return;
        }

        float deltaX = currentPlayerWorldPosition.x - previousPlayerWorldPosition.x;
        previousPlayerWorldPosition = currentPlayerWorldPosition;

        if (Mathf.Abs(deltaX) < playerMotionDirectionThreshold)
            return;

        lastPlayerHorizontalDirection = deltaX > 0f ? 1f : -1f;
    }

    private void UpdateFacingFromFollowSide()
    {
        if (!followPlayer || !facePlayerWhenSideSwitchCompletes)
            return;

        float sideSign;
        bool isSwitchingSides = orbitProgress < 1f && !Mathf.Approximately(orbitStartSideSign, orbitTargetSideSign);
        if (isSwitchingSides)
            sideSign = orbitStartSideSign;
        else
            sideSign = currentFollowOffset.x >= 0f ? 1f : -1f;

        ApplyFacingSign(sideSign);
    }

    private void UpdateFollowOffsetOrbit()
    {
        if (!followPlayer || !mirrorFollowOffsetXFromPlayerMotion)
            return;

        float desiredSideSign = lastPlayerHorizontalDirection > 0f ? -1f : 1f;

        if (!Mathf.Approximately(desiredSideSign, orbitTargetSideSign))
        {
            orbitStartSideSign = Mathf.Sign(Mathf.Approximately(currentFollowOffset.x, 0f) ? orbitTargetSideSign : currentFollowOffset.x);
            if (Mathf.Approximately(orbitStartSideSign, 0f))
                orbitStartSideSign = 1f;

            orbitTargetSideSign = desiredSideSign;
            orbitProgress = 0f;
        }

        if (!orbitAcrossHeadOnSideSwitch)
        {
            currentFollowOffset = new Vector2(
                orbitTargetSideSign * Mathf.Abs(basePlayerOffset.x),
                basePlayerOffset.y);
            return;
        }

        if (Mathf.Approximately(orbitStartSideSign, orbitTargetSideSign))
        {
            currentFollowOffset = new Vector2(
                orbitTargetSideSign * Mathf.Abs(basePlayerOffset.x),
                basePlayerOffset.y);
            orbitProgress = 1f;
            return;
        }

        orbitProgress = Mathf.Clamp01(
            orbitProgress + (Time.unscaledDeltaTime / Mathf.Max(0.01f, sideSwitchDuration)));

        float easedProgress = Mathf.SmoothStep(0f, 1f, orbitProgress);
        float startAngle = orbitStartSideSign > 0f ? 0f : Mathf.PI;
        float endAngle = orbitTargetSideSign > 0f ? 0f : Mathf.PI;
        float angle = Mathf.Lerp(startAngle, endAngle, easedProgress);

        currentFollowOffset = new Vector2(
            Mathf.Cos(angle) * Mathf.Abs(basePlayerOffset.x),
            basePlayerOffset.y + (Mathf.Sin(angle) * sideSwitchArcHeight));

        if (orbitProgress >= 1f)
        {
            orbitStartSideSign = orbitTargetSideSign;
            currentFollowOffset = new Vector2(
                orbitTargetSideSign * Mathf.Abs(basePlayerOffset.x),
                basePlayerOffset.y);
        }
    }
}
