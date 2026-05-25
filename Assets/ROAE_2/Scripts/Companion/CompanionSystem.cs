using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CompanionSystem : MonoBehaviour
{
    private struct BootstrapConfig
    {
        public CompanionProfile profile;
        public CompanionDialogueLibrary dialogueLibrary;
        public CompanionManifestationController manifestationPrefab;
        public bool dontDestroyOnLoad;
        public bool dismissOnSceneChange;
        public bool debugLogs;
        public bool isValid;
    }

    public static CompanionSystem Instance { get; private set; }
    private static BootstrapConfig pendingBootstrapConfig;

    [Header("Authoring")]
    [SerializeField] private CompanionProfile profile;
    [SerializeField] private CompanionDialogueLibrary dialogueLibrary;
    [SerializeField] private CompanionManifestationController manifestationPrefab;

    [Header("Runtime")]
    [SerializeField] private bool dontDestroyOnLoad = true;
    [SerializeField] private bool dismissOnSceneChange = true;
    [SerializeField] private bool debugLogs;

    private readonly List<CompanionSummonPoint> summonPoints = new List<CompanionSummonPoint>();
    private readonly List<CompanionEnvironmentZone> activeZones = new List<CompanionEnvironmentZone>();
    private readonly HashSet<string> narrativeFlags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> unlockedPools = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> discoveredInteractions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    private CompanionSaveState saveState;
    private CompanionManifestationController manifestationInstance;
    private CaptionUIManager captionUI;
    private CompanionSummonPoint currentNearbyPoint;
    private CompanionSummonPoint activePoint;
    private CompanionEmotionalState currentEmotion = CompanionEmotionalState.Healthy;
    private string lastPlannerDebugKey = string.Empty;
    private float nextSpeechAllowedAt;
    private float nextEmotionRefreshAt;
    private Coroutine sceneReanchorCoroutine;
    private bool presentationObscuredByUi;

    public event Action<CompanionPresenceState> PresenceChanged;
    public event Action<CompanionEmotionalState> EmotionChanged;
    public event Action<string> Spoke;

    public bool IsCompanionVisible => saveState != null && saveState.lastPresenceState == CompanionPresenceState.Manifested;

    public CompanionSaveState CurrentState => saveState;
    public bool DebugLoggingEnabled => debugLogs;

    public static CompanionSystem EnsureBootstrapHost(
        CompanionProfile profile,
        CompanionDialogueLibrary dialogueLibrary,
        CompanionManifestationController manifestationPrefab,
        bool keepAcrossScenes = true,
        bool dismissOnSceneChange = false,
        bool debugLogs = false)
    {
        if (Instance != null)
        {
            Instance.ConfigureRuntime(keepAcrossScenes, dismissOnSceneChange, debugLogs);
            Instance.AssignAuthoringAssets(profile, dialogueLibrary, manifestationPrefab);
            return Instance;
        }

        pendingBootstrapConfig = new BootstrapConfig
        {
            profile = profile,
            dialogueLibrary = dialogueLibrary,
            manifestationPrefab = manifestationPrefab,
            dontDestroyOnLoad = keepAcrossScenes,
            dismissOnSceneChange = dismissOnSceneChange,
            debugLogs = debugLogs,
            isValid = true
        };

        GameObject host = new GameObject("ROAE Companion System");
        return host.AddComponent<CompanionSystem>();
    }

    public void AssignAuthoringAssets(
        CompanionProfile newProfile,
        CompanionDialogueLibrary newLibrary,
        CompanionManifestationController newManifestationPrefab = null)
    {
        profile = newProfile;
        dialogueLibrary = newLibrary;
        if (newManifestationPrefab != null)
            manifestationPrefab = newManifestationPrefab;

        EnsureAuthoring();
    }

    public void ConfigureRuntime(bool keepAcrossScenes, bool shouldDismissOnSceneChange, bool enableDebugLogs)
    {
        dontDestroyOnLoad = keepAcrossScenes;
        dismissOnSceneChange = shouldDismissOnSceneChange;
        debugLogs = enableDebugLogs;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ApplyPendingBootstrapConfig();
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        EnsureAuthoring();
        saveState = CompanionPersistence.Load();
        SyncRuntimeSetsFromSave();
        currentEmotion = saveState.lastKnownEmotion;

        if (dismissOnSceneChange)
            saveState.lastPresenceState = CompanionPresenceState.Hidden;

        RegisterSceneSummonPoints();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (saveState == null || profile == null)
            return;

        if (IsCompanionVisible && manifestationInstance != null)
        {
            if (!presentationObscuredByUi)
            {
                manifestationInstance.AttachTo(activePoint != null ? activePoint.GetAnchorPosition() : manifestationInstance.transform.position);
                manifestationInstance.TickPresentation();
            }
        }

        if (Time.unscaledTime >= nextEmotionRefreshAt)
        {
            nextEmotionRefreshAt = Time.unscaledTime + Mathf.Max(0.15f, profile.emotionRefreshIntervalSeconds);
            RefreshEmotion();
        }
    }

    public void RegisterSummonPoint(CompanionSummonPoint point)
    {
        if (point != null && !summonPoints.Contains(point))
            summonPoints.Add(point);
    }

    public void UnregisterSummonPoint(CompanionSummonPoint point)
    {
        summonPoints.Remove(point);
        if (currentNearbyPoint == point)
            currentNearbyPoint = null;
        if (activePoint == point)
            activePoint = null;
    }

    public void NotifyPointAvailability(CompanionSummonPoint point, bool isAvailable)
    {
        if (point == null)
            return;

        if (isAvailable)
            currentNearbyPoint = point;
        else if (currentNearbyPoint == point)
            currentNearbyPoint = FindFallbackNearbyPoint();
    }

    public void EnterZone(CompanionEnvironmentZone zone)
    {
        if (zone != null && !activeZones.Contains(zone))
            activeZones.Add(zone);
    }

    public void ExitZone(CompanionEnvironmentZone zone)
    {
        activeZones.Remove(zone);
    }

    public bool TrySummonAtNearestPoint()
    {
        CompanionSummonPoint point = currentNearbyPoint != null ? currentNearbyPoint : FindFallbackNearbyPoint();
        return TrySummon(point);
    }

    public bool TrySummon(CompanionSummonPoint point)
    {
        if (point == null || !point.AllowManualSummon)
            return false;

        if (!CanSummon(point, out string reason))
        {
            Log("Summon blocked at point=" + point.PointId + " reason=" + reason);
            ShowSummonFailureCaption(reason);
            return false;
        }

        EnsureManifestationInstance();
        Log("After EnsureManifestationInstance instance=" + (manifestationInstance != null ? manifestationInstance.name : "NULL"));
        activePoint = point;

        saveState.totalSummons++;
        saveState.lastSummonUtcTicks = DateTime.UtcNow.Ticks;
        saveState.lastSummonSceneId = SceneManager.GetActiveScene().name;
        saveState.lastActivePointId = point.PointId;
        saveState.summonHistoryUtcTicks.Add(saveState.lastSummonUtcTicks);
        TrimSummonHistory();

        saveState.lastPresenceState = CompanionPresenceState.Summoning;
        PresenceChanged?.Invoke(saveState.lastPresenceState);

        manifestationInstance.AttachTo(point.GetAnchorPosition());
        manifestationInstance.PlaySummon();
        Log("Summon success point=" + point.PointId + " position=" + point.GetAnchorPosition());

        saveState.lastPresenceState = CompanionPresenceState.Manifested;
        PresenceChanged?.Invoke(saveState.lastPresenceState);

        RefreshEmotion();
        manifestationInstance.Pulse(0.9f);
        ShowCaption(BuildSummonArrivalCaption(currentEmotion));

        SaveState();
        return true;
    }

    public void Despawn(string reason)
    {
        if (!IsCompanionVisible)
            return;

        saveState.lastPresenceState = CompanionPresenceState.Withdrawing;
        PresenceChanged?.Invoke(saveState.lastPresenceState);

        manifestationInstance?.PlayDespawn();
        saveState.lastPresenceState = CompanionPresenceState.Hidden;
        PresenceChanged?.Invoke(saveState.lastPresenceState);

        activePoint = null;
        SaveState();
        Log("Despawn reason=" + reason);
    }

    public bool TrySpeak(CompanionSpeechRequest request)
    {
        if (request == null || profile == null || dialogueLibrary == null)
            return false;

        if (Time.unscaledTime < nextSpeechAllowedAt)
            return false;

        CompanionEvaluationContext context = BuildContext(request.extraTags, request.focusId);
        CompanionDialogueResult result = CompanionDialogueResolver.Resolve(
            dialogueLibrary,
            context,
            request,
            currentEmotion,
            saveState,
            profile);

        if (result.Entry == null || string.IsNullOrWhiteSpace(result.Entry.line))
            return false;

        ShowCaption(result.Entry.line);
        RememberDialogue(result.Entry);
        if (result.Entry.unlocksOnHear && !string.IsNullOrWhiteSpace(result.Entry.unlockPoolId))
            UnlockDialoguePool(result.Entry.unlockPoolId);

        nextSpeechAllowedAt = Time.unscaledTime + Mathf.Max(0.15f, profile.reactiveSpeechCooldownSeconds);
        Spoke?.Invoke(result.Entry.line);
        SaveState();
        return true;
    }

    public void ObserveTarget(CompanionObservationTarget target)
    {
        if (target == null || !target.CanObserve())
            return;

        if (manifestationInstance != null && IsCompanionVisible)
        {
            manifestationInstance.FocusAt(target.GetFocusPosition());
            manifestationInstance.Pulse(target.PulseStrength);
        }

        if (!string.IsNullOrWhiteSpace(target.DiscoveryId))
            MarkInteractionDiscovered(target.DiscoveryId);

        if (target.RequestHintOnObserve)
        {
            TrySpeak(new CompanionSpeechRequest
            {
                intent = CompanionDialogueIntent.Hint,
                extraTags = new List<CompanionEnvironmentTag>(target.Tags),
                focusId = target.TargetId
            });
        }
    }

    public void PulseCompanion(float strength)
    {
        if (IsCompanionVisible)
            manifestationInstance?.Pulse(strength);
    }

    public void ObserveNpcPlannerFeedback(
        string npcId,
        float neutralScore,
        float warmScore,
        float mischievousScore,
        BaristaIntroTone resolvedTone)
    {
        if (saveState == null || profile == null)
            return;

        Vector3 observedSignals = NormalizeNarrativeSignals(neutralScore, warmScore, mischievousScore, resolvedTone);
        GetCurrentSocialSignals(out float currentWarm, out float currentNeutral, out float currentMischievous);

        float blend = Mathf.Clamp01(profile.socialSignalBlend);
        saveState.warmSignal = Mathf.Lerp(currentWarm, observedSignals.y, blend);
        saveState.neutralSignal = Mathf.Lerp(currentNeutral, observedSignals.x, blend);
        saveState.mischievousSignal = Mathf.Lerp(currentMischievous, observedSignals.z, blend);
        saveState.socialSignalUpdatedUtcTicks = DateTime.UtcNow.Ticks;
        saveState.lastNpcSourceId = npcId ?? string.Empty;
        saveState.lastSocialTone = DetermineDominantSocialTone(
            saveState.warmSignal,
            saveState.neutralSignal,
            saveState.mischievousSignal);

        Log(
            "Observed NPC planner feedback npc=" + saveState.lastNpcSourceId +
            " warm=" + saveState.warmSignal.ToString("0.0") +
            " neutral=" + saveState.neutralSignal.ToString("0.0") +
            " mischievous=" + saveState.mischievousSignal.ToString("0.0") +
            " dominant=" + saveState.lastSocialTone);

        RefreshEmotion();
        SaveState();
    }

    public void SetPresentationObscuredByUi(bool obscured)
    {
        presentationObscuredByUi = obscured;

        if (manifestationInstance == null)
            return;

        manifestationInstance.SetUiObscured(obscured);

        if (!obscured && IsCompanionVisible)
            manifestationInstance.TrySnapToCurrentPlayer(true);
    }

    public void MarkInteractionDiscovered(string interactionId)
    {
        if (string.IsNullOrWhiteSpace(interactionId))
            return;

        if (discoveredInteractions.Add(interactionId.Trim()))
        {
            saveState.discoveredInteractionIds.Add(interactionId.Trim());
            SaveState();
        }
    }

    public void UnlockDialoguePool(string poolId)
    {
        if (string.IsNullOrWhiteSpace(poolId))
            return;

        if (unlockedPools.Add(poolId.Trim()))
        {
            saveState.unlockedDialoguePools.Add(poolId.Trim());
            SaveState();
        }
    }

    public void SetNarrativeFlag(string flagKey, bool enabled)
    {
        if (string.IsNullOrWhiteSpace(flagKey))
            return;

        if (enabled)
        {
            if (narrativeFlags.Add(flagKey.Trim()))
                saveState.narrativeFlags.Add(flagKey.Trim());
        }
        else
        {
            narrativeFlags.Remove(flagKey.Trim());
            saveState.narrativeFlags.RemoveAll(x => string.Equals(x, flagKey.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        SaveState();
    }

    public void ResetSocialStateForDebug()
    {
        if (saveState == null)
            return;

        saveState.warmSignal = 0f;
        saveState.neutralSignal = 0f;
        saveState.mischievousSignal = 0f;
        saveState.socialSignalUpdatedUtcTicks = 0L;
        saveState.lastNpcSourceId = string.Empty;
        saveState.lastSocialTone = CompanionSocialTone.None;
        SaveState();
        RefreshEmotion();
        Log("Reset social planner state for debug");
    }

    public void AdjustSelfAwareness(int delta)
    {
        // Deprecated in the small 3-stat companion model.
    }

    public void AdjustContradiction(int delta)
    {
        // Deprecated in the small 3-stat companion model.
    }

    private bool CanSummon(CompanionSummonPoint point, out string reason)
    {
        reason = string.Empty;
        if (point == null)
        {
            reason = "missing-point";
            return false;
        }

        if (!point.IsPlayerNearby)
        {
            reason = "player-not-near";
            return false;
        }

        float sinceLastSummon = CompanionMath.SecondsSince(saveState.lastSummonUtcTicks, DateTime.UtcNow);
        if (sinceLastSummon < profile.baseSummonCooldownSeconds)
        {
            reason = "cooldown";
            return false;
        }

        int empathyValue = GetCreativityCoreValue(
            core => core.Empathy,
            "empathy",
            CreativeStatScale.DefaultEmpathy);
        int corruptionValue = GetCreativityCoreValue(
            core => core.PlantCorruption,
            "plantCorruption",
            CreativeStatScale.DefaultCorruption);

        Log(
            "Summon check point=" + point.PointId +
            " ignoreCorruption=" + point.IgnoreCorruptionRestrictionForTesting +
            " corruption=" + corruptionValue +
            " empathy=" + empathyValue);

        if (ShouldUseNpcSignalSummonGate())
        {
            GetCurrentSocialSignals(out float warmSignal, out float neutralSignal, out float mischievousSignal);
            CompanionSocialTone dominantSocialTone = DetermineDominantSocialTone(warmSignal, neutralSignal, mischievousSignal);

            Log(
                "Summon social gate point=" + point.PointId +
                " warm=" + warmSignal.ToString("0.0") +
                " neutral=" + neutralSignal.ToString("0.0") +
                " mischievous=" + mischievousSignal.ToString("0.0") +
                " dominant=" + dominantSocialTone);

            if (dominantSocialTone != CompanionSocialTone.Warm)
            {
                reason = "npc_signal_not_warm(" + dominantSocialTone + ")";
                return false;
            }
        }
        else if (profile.blockSummonWhenEmpathyTooLow &&
            empathyValue <= profile.lowEmpathySummonBlockThreshold)
        {
            reason = "low_empathy(" + empathyValue + ")";
            return false;
        }

        CompanionEvaluationContext context = BuildContext(point.BuildTags(), point.PointId, point);
        context.Snapshot.isSafeSpace = point.IsEmotionallySafeSpace;
        context.Snapshot.pointType = point.PointType;
        context.Snapshot.threatLevel = MaxThreat(context.Snapshot.threatLevel, point.AmbientThreatLevel);

        Log(
            "Summon safety point=" + point.PointId +
            " pointSafe=" + point.IsEmotionallySafeSpace +
            " snapshotSafe=" + context.Snapshot.isSafeSpace);

        if (!point.SummonConditions.Matches(context))
        {
            reason = "point-conditions";
            return false;
        }

        if (!context.Snapshot.isSafeSpace)
        {
            reason = "unsafe-space";
            return false;
        }

        return true;
    }

    private bool ShouldUseNpcSignalSummonGate()
    {
        return profile != null &&
            profile.useSocialPlanner &&
            profile.socialPlannerUsesNpcSignalsOnly;
    }

    private void RefreshEmotion()
    {
        CompanionEvaluationContext context = BuildContext(activePoint != null ? activePoint.BuildTags() : null, activePoint != null ? activePoint.PointId : string.Empty);
        CompanionEmotionResult result = CompanionEmotionResolver.Resolve(profile, context);
        if (!string.IsNullOrWhiteSpace(result.DebugSummary) &&
            !string.Equals(lastPlannerDebugKey, result.DebugKey, StringComparison.Ordinal))
        {
            lastPlannerDebugKey = result.DebugKey;
            if (debugLogs)
                Debug.Log(result.DebugSummary);
        }

        currentEmotion = result.State;
        saveState.lastKnownEmotion = currentEmotion;
        manifestationInstance?.SetEmotion(currentEmotion, Mathf.InverseLerp(0f, 120f, result.Score));
        manifestationInstance?.SetShellHidden(currentEmotion == CompanionEmotionalState.Numb);
        EmotionChanged?.Invoke(currentEmotion);
    }

    private CompanionEvaluationContext BuildContext(
        List<CompanionEnvironmentTag> extraTags,
        string focusId,
        CompanionSummonPoint overridePoint = null)
    {
        GetCurrentSocialSignals(out float warmSignal, out float neutralSignal, out float mischievousSignal);
        CompanionContextSnapshot snapshot = new CompanionContextSnapshot
        {
            creativity = GetCreativityCoreValue(core => core.Creativity, "creativity", CreativeStatScale.DefaultCreativity),
            empathy = GetCreativityCoreValue(core => core.Empathy, "empathy", CreativeStatScale.DefaultEmpathy),
            corruption = GetCreativityCoreValue(core => core.PlantCorruption, "plantCorruption", CreativeStatScale.DefaultCorruption),
            isSafeSpace = false,
            isCompanionVisible = IsCompanionVisible,
            threatLevel = CompanionThreatLevel.None,
            presenceState = saveState.lastPresenceState,
            currentEmotion = currentEmotion,
            chapterId = NarrativeProgressState.GetCurrentChapterId(),
            sceneId = NarrativeProgressState.GetCurrentSceneId(),
            momentId = NarrativeProgressState.GetCurrentMomentId(),
            summonPointId = focusId ?? string.Empty,
            pointType = overridePoint != null ? overridePoint.PointType : (activePoint != null ? activePoint.PointType : CompanionSummonPointType.Generic),
            warmSignal = warmSignal,
            neutralSignal = neutralSignal,
            mischievousSignal = mischievousSignal,
            dominantSocialTone = DetermineDominantSocialTone(warmSignal, neutralSignal, mischievousSignal),
            lastNpcSourceId = saveState != null ? saveState.lastNpcSourceId : string.Empty,
            tags = new List<CompanionEnvironmentTag>()
        };

        if (overridePoint != null)
        {
            snapshot.isSafeSpace = overridePoint.IsEmotionallySafeSpace;
            snapshot.threatLevel = MaxThreat(snapshot.threatLevel, overridePoint.AmbientThreatLevel);
            MergeTags(snapshot.tags, overridePoint.BuildTags());
        }
        else if (activePoint != null)
        {
            snapshot.isSafeSpace = activePoint.IsEmotionallySafeSpace;
            snapshot.threatLevel = MaxThreat(snapshot.threatLevel, activePoint.AmbientThreatLevel);
            MergeTags(snapshot.tags, activePoint.BuildTags());
        }

        for (int i = 0; i < activeZones.Count; i++)
        {
            CompanionEnvironmentZone zone = activeZones[i];
            if (zone == null)
                continue;

            if (zone.ContributesSafeSpace)
                snapshot.isSafeSpace = true;

            snapshot.threatLevel = MaxThreat(snapshot.threatLevel, zone.ThreatLevel);
            MergeTags(snapshot.tags, zone.Tags);
        }

        MergeTags(snapshot.tags, extraTags);
        return new CompanionEvaluationContext(snapshot, narrativeFlags, unlockedPools, discoveredInteractions);
    }

    private void RememberDialogue(CompanionDialogueEntry entry)
    {
        if (entry == null)
            return;

        long now = DateTime.UtcNow.Ticks;
        for (int i = 0; i < saveState.dialogueMemory.Count; i++)
        {
            if (!string.Equals(saveState.dialogueMemory[i].entryId, entry.entryId, StringComparison.OrdinalIgnoreCase))
                continue;

            CompanionDialogueMemoryEntry memory = saveState.dialogueMemory[i];
            memory.lastUsedUtcTicks = now;
            memory.heardCount += 1;
            saveState.dialogueMemory[i] = memory;
            return;
        }

        saveState.dialogueMemory.Add(new CompanionDialogueMemoryEntry
        {
            entryId = entry.entryId,
            lastUsedUtcTicks = now,
            heardCount = 1
        });
    }

    private void EnsureManifestationInstance()
    {
        if (manifestationInstance != null)
            return;

        if (manifestationPrefab != null)
        {
            manifestationInstance = Instantiate(manifestationPrefab);
            Log("Instantiated manifestation from prefab=" + manifestationPrefab.name);
        }
        else
        {
            GameObject fallback = new GameObject("CompanionManifestationRuntime");
            manifestationInstance = fallback.AddComponent<CompanionManifestationController>();
            Log("Instantiated fallback manifestation object");
        }

        manifestationInstance.gameObject.name = "CompanionManifestationRuntime";
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(manifestationInstance.gameObject);

        manifestationInstance.SetUiObscured(presentationObscuredByUi);
    }

    private void EnsureAuthoring()
    {
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<CompanionProfile>();
            profile.name = "RuntimeCompanionProfile";
            profile.EnsureStarterRules();
        }

        if (dialogueLibrary == null)
        {
            dialogueLibrary = ScriptableObject.CreateInstance<CompanionDialogueLibrary>();
            dialogueLibrary.name = "RuntimeCompanionDialogueLibrary";
            dialogueLibrary.EnsureStarterEntries();
        }
    }

    private void ApplyPendingBootstrapConfig()
    {
        if (!pendingBootstrapConfig.isValid)
            return;

        profile = pendingBootstrapConfig.profile;
        dialogueLibrary = pendingBootstrapConfig.dialogueLibrary;
        manifestationPrefab = pendingBootstrapConfig.manifestationPrefab;
        dontDestroyOnLoad = pendingBootstrapConfig.dontDestroyOnLoad;
        dismissOnSceneChange = pendingBootstrapConfig.dismissOnSceneChange;
        debugLogs = pendingBootstrapConfig.debugLogs;
        pendingBootstrapConfig = default;
    }

    private void ShowCaption(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (captionUI == null)
            captionUI = CaptionUIManager.Instance ?? UnityEngine.Object.FindFirstObjectByType<CaptionUIManager>();

        if (captionUI != null)
            captionUI.ShowCaption(text);
        else
            Debug.Log("[ROAE][Companion] " + text);
    }

    private void ShowSummonFailureCaption(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return;

        if (reason.StartsWith("low_empathy", StringComparison.OrdinalIgnoreCase))
        {
            ShowCaption("The snail will not come. Rina is too closed off right now.");
            return;
        }

        if (reason.StartsWith("npc_signal_not_warm", StringComparison.OrdinalIgnoreCase))
        {
            ShowCaption("The snail waits for a warmer echo before surfacing.");
            return;
        }

        if (string.Equals(reason, "unsafe-space", StringComparison.OrdinalIgnoreCase))
        {
            ShowCaption("The snail recoils from this place.");
            return;
        }

        if (string.Equals(reason, "cooldown", StringComparison.OrdinalIgnoreCase))
        {
            ShowCaption("The snail is not ready to surface again yet.");
        }
    }

    private static string BuildSummonArrivalCaption(CompanionEmotionalState emotion)
    {
        switch (emotion)
        {
            case CompanionEmotionalState.Malicious:
                return "The snail emerges with a violet, malicious gleam.";

            case CompanionEmotionalState.Sad:
                return "The snail appears in a hush of gray, heavy with sadness.";

            case CompanionEmotionalState.Numb:
                return "The snail drifts in, blue and distant.";

            case CompanionEmotionalState.Healthy:
            default:
                return "The snail arrives bright and steady, glowing red.";
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        activeZones.Clear();
        currentNearbyPoint = null;
        activePoint = null;
        RegisterSceneSummonPoints();

        if (dismissOnSceneChange)
        {
            Despawn("scene-change");
            return;
        }

        if (!IsCompanionVisible || manifestationInstance == null)
            return;

        if (sceneReanchorCoroutine != null)
            StopCoroutine(sceneReanchorCoroutine);

        manifestationInstance.AttachTo(manifestationInstance.transform.position);
        sceneReanchorCoroutine = StartCoroutine(ReanchorVisibleCompanionAfterSceneLoad(scene.name));
    }

    private IEnumerator ReanchorVisibleCompanionAfterSceneLoad(string sceneName)
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            if (!IsCompanionVisible || manifestationInstance == null)
            {
                sceneReanchorCoroutine = null;
                yield break;
            }

            if (manifestationInstance.TrySnapToCurrentPlayer(forceRefresh: true))
            {
                Log("Reanchored visible companion to player after scene load scene=" + sceneName + " attempt=" + (attempt + 1));
                sceneReanchorCoroutine = null;
                yield break;
            }

            yield return null;
        }

        Log("Visible companion could not find a player to follow after scene load scene=" + sceneName);
        sceneReanchorCoroutine = null;
    }

    private void SyncRuntimeSetsFromSave()
    {
        narrativeFlags.Clear();
        unlockedPools.Clear();
        discoveredInteractions.Clear();

        AddAll(narrativeFlags, saveState.narrativeFlags);
        AddAll(unlockedPools, saveState.unlockedDialoguePools);
        AddAll(discoveredInteractions, saveState.discoveredInteractionIds);
    }

    private void SaveState()
    {
        CompanionPersistence.Save(saveState);
    }

    private void TrimSummonHistory()
    {
        if (saveState == null || saveState.summonHistoryUtcTicks == null)
            return;

        saveState.summonHistoryUtcTicks.RemoveAll(ticks => ticks <= 0L);
        const int maxHistoryEntries = 8;
        if (saveState.summonHistoryUtcTicks.Count > maxHistoryEntries)
            saveState.summonHistoryUtcTicks.RemoveRange(0, saveState.summonHistoryUtcTicks.Count - maxHistoryEntries);
    }

    private CompanionSummonPoint FindFallbackNearbyPoint()
    {
        for (int i = 0; i < summonPoints.Count; i++)
        {
            if (summonPoints[i] != null && summonPoints[i].IsPlayerNearby)
                return summonPoints[i];
        }

        return null;
    }

    private static CompanionThreatLevel MaxThreat(CompanionThreatLevel a, CompanionThreatLevel b)
    {
        return (CompanionThreatLevel)Mathf.Max((int)a, (int)b);
    }

    private static void MergeTags(List<CompanionEnvironmentTag> target, IReadOnlyList<CompanionEnvironmentTag> source)
    {
        if (target == null || source == null)
            return;

        for (int i = 0; i < source.Count; i++)
        {
            if (!target.Contains(source[i]))
                target.Add(source[i]);
        }
    }

    private static void AddAll(HashSet<string> target, List<string> source)
    {
        if (target == null || source == null)
            return;

        for (int i = 0; i < source.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(source[i]))
                target.Add(source[i].Trim());
        }
    }

    private static int GetCreativityCoreValue(Func<CreativeCore, int> accessor, string prefsKey, int fallbackValue)
    {
        CreativeCore core = CreativeCore.Instance ?? UnityEngine.Object.FindFirstObjectByType<CreativeCore>();
        if (core != null)
            return accessor(core);

        return PlayerPrefs.GetInt(prefsKey, fallbackValue);
    }

    private void RegisterSceneSummonPoints()
    {
        summonPoints.Clear();
        CompanionSummonPoint[] scenePoints = UnityEngine.Object.FindObjectsByType<CompanionSummonPoint>(FindObjectsSortMode.None);
        for (int i = 0; i < scenePoints.Length; i++)
        {
            if (scenePoints[i] != null && !summonPoints.Contains(scenePoints[i]))
                summonPoints.Add(scenePoints[i]);
        }
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log("[ROAE][CompanionSystem] " + message);
    }

    private void GetCurrentSocialSignals(out float warm, out float neutral, out float mischievous)
    {
        warm = saveState != null ? saveState.warmSignal : 0f;
        neutral = saveState != null ? saveState.neutralSignal : 0f;
        mischievous = saveState != null ? saveState.mischievousSignal : 0f;

        if (saveState == null || profile == null || saveState.socialSignalUpdatedUtcTicks <= 0L)
            return;

        float decayPerSecond = Mathf.Max(0f, profile.socialSignalDecayPerMinute) / 60f;
        if (decayPerSecond <= 0f)
            return;

        float secondsSinceUpdate = CompanionMath.SecondsSince(saveState.socialSignalUpdatedUtcTicks, DateTime.UtcNow);
        if (secondsSinceUpdate <= 0f)
            return;

        float decayAmount = secondsSinceUpdate * decayPerSecond;
        warm = Mathf.Max(0f, warm - decayAmount);
        neutral = Mathf.Max(0f, neutral - decayAmount);
        mischievous = Mathf.Max(0f, mischievous - decayAmount);
    }

    private static Vector3 NormalizeNarrativeSignals(
        float neutralScore,
        float warmScore,
        float mischievousScore,
        BaristaIntroTone resolvedTone)
    {
        float neutral = Mathf.Max(0f, neutralScore);
        float warm = Mathf.Max(0f, warmScore);
        float mischievous = Mathf.Max(0f, mischievousScore);
        float total = neutral + warm + mischievous;

        if (total <= 0.001f)
        {
            switch (resolvedTone)
            {
                case BaristaIntroTone.Warm:
                    return new Vector3(0f, 100f, 0f);

                case BaristaIntroTone.Mischievous:
                    return new Vector3(0f, 0f, 100f);

                default:
                    return new Vector3(100f, 0f, 0f);
            }
        }

        float scale = 100f / total;
        return new Vector3(
            neutral * scale,
            warm * scale,
            mischievous * scale);
    }

    private static CompanionSocialTone DetermineDominantSocialTone(float warm, float neutral, float mischievous)
    {
        float max = Mathf.Max(warm, Mathf.Max(neutral, mischievous));
        if (max <= 0.01f)
            return CompanionSocialTone.None;

        int ties = 0;
        if (Mathf.Abs(warm - max) < 0.01f)
            ties++;
        if (Mathf.Abs(neutral - max) < 0.01f)
            ties++;
        if (Mathf.Abs(mischievous - max) < 0.01f)
            ties++;

        if (ties > 1)
            return CompanionSocialTone.Mixed;

        if (Mathf.Abs(warm - max) < 0.01f)
            return CompanionSocialTone.Warm;

        if (Mathf.Abs(neutral - max) < 0.01f)
            return CompanionSocialTone.Neutral;

        return CompanionSocialTone.Mischievous;
    }
}
