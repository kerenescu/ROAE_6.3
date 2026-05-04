using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public sealed class BaristaWelcomeBrain : MonoBehaviour, INarrativeMomentPlanner<NpcTonePlanningRuntimeState, BaristaWelcomePlannerResult>
{
    [SerializeField] private BaristaWelcomeConfig config;
    [SerializeField] private NpcToneDialogueController toneController;
    [SerializeField] private BaristaPlannerMode plannerMode = BaristaPlannerMode.ValueIteration;
    [SerializeField] private bool auditLogs = true;
    [SerializeField] private bool verbosePlannerLogs = false;
    [SerializeField] private bool verboseStateExtraction = false;
    [SerializeField] private MonoBehaviour explicitStateSource;

    public BaristaPlannerMode PlannerMode => ResolvePlannerMode();
    public NpcTonePlannerSettings CurrentPlannerSettings => ResolvePlannerSettings();

    public void SetPlannerMode(BaristaPlannerMode mode)
    {
        plannerMode = mode;
        if (toneController != null)
            toneController.SetPlannerMode(mode);

        if (config != null)
            config.plannerMode = mode;
    }

    public void UseValueIteration()
    {
        SetPlannerMode(BaristaPlannerMode.ValueIteration);
    }

    public void UsePolicyIteration()
    {
        SetPlannerMode(BaristaPlannerMode.PolicyIteration);
    }

    public BaristaIntroTone DecideOpeningTone()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        NpcTonePlanningRuntimeState runtimeState = ExtractRuntimeState();
        NpcTonePlannerEvaluation evaluation = Evaluate(runtimeState);
        stopwatch.Stop();

        if (auditLogs)
        {
            Debug.Log(
                "[ROAE][AI][BaristaWelcomeBrain][SUCCESS] planner=" + PlannerMode +
                " extractedState={" + runtimeState.ToDebugString() + "}" +
                " evaluation={" + evaluation.BuildDebugString() + "}" +
                " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
        }

        return evaluation.mappedTone;
    }

    public string DebugDecideActionLabel()
    {
        NpcTonePlannerEvaluation evaluation = Evaluate(ExtractRuntimeState());
        return PlannerMode + " -> " + evaluation.BuildDebugString();
    }

    public BaristaWelcomePlannerResult ResolveCurrentOutcome()
    {
        return ResolveOutcome(ExtractRuntimeState());
    }

    public NpcTonePlanningRuntimeState BuildCurrentRuntimeState()
    {
        return ExtractRuntimeState();
    }

    public BaristaWelcomePlannerResult ResolveCurrentResult()
    {
        return ResolveCurrentOutcome();
    }

    public BaristaWelcomePlannerResult ResolveOutcome(NpcTonePlanningRuntimeState runtimeState)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        BaristaWelcomePlannerResult result = BaristaWelcomeOutcomeResolver.Resolve(
            new BaristaWelcomePlannerInput
            {
                creativity = runtimeState.creativity,
                empathy = runtimeState.empathy,
                corruption = runtimeState.corruption,
                relationship = runtimeState.relationship,
                readUnknownText = runtimeState.readUnknownText,
                introDone = runtimeState.introDone,
                pendingDrink = runtimeState.pendingDrink,
                pendingDrinkAcknowledged = runtimeState.pendingDrinkAcknowledged,
                heldDrink = runtimeState.heldDrink
            },
            ResolvePlannerMode(),
            ResolvePlannerSettings(),
            auditLogs);

        stopwatch.Stop();
        if (auditLogs)
        {
            Debug.Log(
                "[ROAE][AI][BaristaOutcome][SUCCESS] planner=" + PlannerMode +
                " state={" + runtimeState.ToDebugString() + "}" +
                " result={" + result.BuildDebugString() + "}" +
                " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
        }

        return result;
    }

    public BaristaWelcomePlannerResult ResolveResult(NpcTonePlanningRuntimeState runtimeState)
    {
        return ResolveOutcome(runtimeState);
    }

    public NpcTonePlannerEvaluation Evaluate(NpcTonePlanningRuntimeState runtimeState)
    {
        return NpcTonePlanningSolvers.Evaluate(
            runtimeState,
            ResolvePlannerMode(),
            ResolvePlannerSettings(),
            verbosePlannerLogs,
            auditLogs);
    }

    private NpcTonePlanningRuntimeState ExtractRuntimeState()
    {
        CreativeCore creativeCore = CreativeCore.Instance;

        NpcTonePlanningRuntimeState runtimeState = new NpcTonePlanningRuntimeState
        {
            readUnknownText = BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.ReadUnknownText01),
            creativity = creativeCore != null ? creativeCore.Creativity : PlayerPrefs.GetInt("creativity", 50),
            corruption = creativeCore != null ? creativeCore.PlantCorruption : PlayerPrefs.GetInt("plantCorruption", 0),
            empathy = creativeCore != null ? creativeCore.Empathy : PlayerPrefs.GetInt("empathy", 0),
            relationship = BaristaWelcomeState.GetBaristaRelationship(),
            introDone = ReadCurrentIntroDoneState(),
            pendingDrink = BaristaWelcomeState.GetPendingDrink(),
            pendingDrinkAcknowledged = BaristaWelcomeState.HasAcknowledgedPendingDrink(),
            heldDrink = BaristaWelcomeState.GetHeldDrink()
        };

        if (verboseStateExtraction)
        {
            string stateSource = explicitStateSource != null
                ? explicitStateSource.name + " (legacy_source_ignored)"
                : "canonical_runtime_state";

            Debug.Log(
                "[ROAE][BaristaWelcomeBrain][StateExtraction] source=" + stateSource +
                " runtimeState={" + runtimeState.ToDebugString() + "}");
        }

        return runtimeState;
    }

    private BaristaPlannerMode ResolvePlannerMode()
    {
        if (toneController != null)
            return toneController.ResolvePlannerMode();

        return config != null ? config.plannerMode : plannerMode;
    }

    private NpcTonePlannerSettings ResolvePlannerSettings()
    {
        if (toneController != null)
            return toneController.ResolvePlannerSettings();

        return config != null ? config.ToPlannerSettings() : NpcTonePlannerSettings.Default;
    }

    private bool ReadCurrentIntroDoneState()
    {
        if (toneController != null &&
            toneController.TryGetActiveIntroDoneFlagKey(out string introDoneFlagKey) &&
            !string.IsNullOrWhiteSpace(introDoneFlagKey))
        {
            return PlayerPrefs.GetInt(introDoneFlagKey, 0) == 1;
        }

        return BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.BaristaIntroDone);
    }
}
