using System.Collections.Generic;
using UnityEngine;

public class NpcToneDialogueController : NpcToneDialogueControllerBase
{
    [Header("Profile")]
    [SerializeField] private NpcToneDialogueProfile profile;
    [SerializeField] private List<NpcToneDialogueProfile> momentProfiles = new List<NpcToneDialogueProfile>();

    [Header("Dependencies")]
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Debug")]
    [SerializeField] private string controllerLogPrefix = "NpcTone";
    [SerializeField] private bool auditLogs = true;
    [SerializeField] private bool verbosePlannerLogs;
    [SerializeField, HideInInspector] private bool debugLogs = true;

    public NpcToneDialogueProfile Profile => profile;
    public IReadOnlyList<NpcToneDialogueProfile> MomentProfiles => momentProfiles;
    public NpcToneDialogueProfile ActiveProfile => ResolveAssignedOrFallbackProfile();

    protected override DialogueManager DialogueManagerReference
    {
        get => dialogueManager;
        set => dialogueManager = value;
    }

    protected override NpcToneDialogueProfile AssignedProfile => ResolveAssignedOrFallbackProfile();
    protected override bool AuditLogsEnabled => auditLogs;
    protected override bool VerbosePlannerLogsEnabled => verbosePlannerLogs;
    protected override string ControllerLogPrefix => string.IsNullOrWhiteSpace(controllerLogPrefix) ? "NpcTone" : controllerLogPrefix.Trim();

    public void AssignProfile(NpcToneDialogueProfile targetProfile)
    {
        profile = targetProfile;
        InvalidateLegacyProfileCache();
    }

    public void AssignMomentProfiles(IEnumerable<NpcToneDialogueProfile> profiles)
    {
        momentProfiles.Clear();
        if (profiles != null)
        {
            foreach (NpcToneDialogueProfile candidate in profiles)
            {
                if (candidate != null && !momentProfiles.Contains(candidate))
                    momentProfiles.Add(candidate);
            }
        }

        InvalidateLegacyProfileCache();
    }

    public BaristaPlannerMode ResolvePlannerMode()
    {
        NpcToneDialogueProfile activeProfile = ResolveAssignedOrFallbackProfile();
        return activeProfile != null ? activeProfile.plannerMode : BaristaPlannerMode.PolicyIteration;
    }

    public NpcTonePlannerSettings ResolvePlannerSettings()
    {
        NpcToneDialogueProfile activeProfile = ResolveAssignedOrFallbackProfile();
        return activeProfile != null ? activeProfile.ToPlannerSettings() : NpcTonePlannerSettings.Default;
    }

    public BaristaIntroTone ResolveTone(NpcTonePlannerEvaluation evaluation)
    {
        NpcToneDialogueProfile activeProfile = ResolveAssignedOrFallbackProfile();
        NpcToneDialogueToneSelectionMode toneMode = activeProfile != null
            ? activeProfile.toneSelectionMode
            : NpcToneDialogueToneSelectionMode.UsePlanner;

        switch (toneMode)
        {
            case NpcToneDialogueToneSelectionMode.ForceWarm:
                return BaristaIntroTone.Warm;

            case NpcToneDialogueToneSelectionMode.ForceMischievous:
                return BaristaIntroTone.Mischievous;

            case NpcToneDialogueToneSelectionMode.ForceNeutral:
                return BaristaIntroTone.Neutral;

            default:
                return BaristaDialogueResolver.NormalizeTone(evaluation.mappedTone);
        }
    }

    public void SetPlannerMode(BaristaPlannerMode mode)
    {
        if (profile != null)
            profile.plannerMode = mode;

        if (momentProfiles != null)
        {
            for (int i = 0; i < momentProfiles.Count; i++)
            {
                if (momentProfiles[i] != null)
                    momentProfiles[i].plannerMode = mode;
            }
        }
    }

    public bool TryGetActiveIntroDoneFlagKey(out string key)
    {
        NpcToneDialogueProfile activeProfile = ResolveAssignedOrFallbackProfile();
        if (activeProfile?.phaseDefinitions != null)
        {
            for (int i = 0; i < activeProfile.phaseDefinitions.Count; i++)
            {
                NpcToneDialoguePhaseDefinition phase = activeProfile.phaseDefinitions[i];
                if (phase?.introDone == null)
                    continue;

                if (phase.introDone.source == NpcToneDialogueBooleanSource.PlayerPrefFlag &&
                    !string.IsNullOrWhiteSpace(phase.introDone.playerPrefKey))
                {
                    key = phase.introDone.playerPrefKey.Trim();
                    return true;
                }
            }
        }

        key = string.Empty;
        return false;
    }

    public void TriggerDialogue()
    {
        TriggerDialogueInternal();
    }

    public void TriggerNpcDialogue()
    {
        TriggerDialogueInternal();
    }

    // Compatibility alias for legacy Adventure Creator hookups while migrating older NPCs.
    public void TriggerAnticariatDialogue()
    {
        TriggerDialogueInternal();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (debugLogs && !auditLogs)
            auditLogs = true;

        InvalidateLegacyProfileCache();
    }
#endif

    [ContextMenu("ROAE/Tone NPC/Print current decision")]
    public void PrintCurrentDecision()
    {
        PrintCurrentDecisionInternal();
    }

    [ContextMenu("ROAE/Tone NPC/Reset dev state and planner cache")]
    public void ResetDevStateAndPlannerCache()
    {
        ResetDevStateAndPlannerCacheInternal();
    }

    protected override NpcToneDialogueProfile BuildLegacyProfile()
    {
        return null;
    }

    private NpcToneDialogueProfile ResolveAssignedOrFallbackProfile()
    {
        if (momentProfiles != null && momentProfiles.Count > 0)
        {
            NpcToneDialogueProfile activeMomentProfile = NarrativeMomentSelection.ResolveHighestPriorityActiveMoment(
                momentProfiles,
                ReadBoolFlag);

            if (activeMomentProfile != null)
                return activeMomentProfile;
        }

        return profile;
    }

    private static bool ReadBoolFlag(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && PlayerPrefs.GetInt(key, 0) == 1;
    }
}
