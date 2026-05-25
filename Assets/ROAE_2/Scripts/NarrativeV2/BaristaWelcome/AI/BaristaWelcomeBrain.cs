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
                hasDrink = runtimeState.hasDrink,
                pendingDrink = BaristaDrinkType.None,
                pendingDrinkAcknowledged = false,
                heldDrink = runtimeState.hasDrink ? runtimeState.heldDrink : BaristaDrinkType.None
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
        NpcToneDialogueProfile activeProfile = toneController != null ? toneController.ActiveProfile : null;

        if (activeProfile != null)
        {
            return NarrativeTonePlanningSolvers.Evaluate(
                runtimeState,
                activeProfile,
                verbosePlannerLogs,
                auditLogs);
        }

        return NarrativeTonePlanningSolvers.Evaluate(
            runtimeState,
            ResolvePlannerMode(),
            ResolvePlannerSettings(),
            verbosePlannerLogs,
            auditLogs);
    }

    private NpcTonePlanningRuntimeState ExtractRuntimeState()
    {
        NpcFactContext factContext = NpcFactContext.BuildLive("barista");
        NpcTonePlanningRuntimeState runtimeState = new NpcTonePlanningRuntimeState
        {
            readUnknownText = factContext.readUnknownText,
            creativity = factContext.creativity,
            corruption = factContext.corruption,
            empathy = factContext.empathy,
            relationship = factContext.relationship,
            introDone = ReadCurrentIntroDoneState(),
            hasDrink = factContext.hasDrink,
            pendingDrink = BaristaDrinkType.None,
            pendingDrinkAcknowledged = false,
            heldDrink = factContext.hasDrink ? factContext.heldDrink : BaristaDrinkType.None
        };

        if (verboseStateExtraction)
        {
            string stateSource = explicitStateSource != null
                ? explicitStateSource.name + " (legacy_source_ignored)"
                : "canonical_runtime_state";

            Debug.Log(
                "[ROAE][BaristaWelcomeBrain][StateExtraction] source=" + stateSource +
                " factContext={" + factContext.ToDebugString() + "}" +
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
