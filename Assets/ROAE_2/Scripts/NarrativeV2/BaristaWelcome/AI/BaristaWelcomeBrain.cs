using UnityEngine;

public sealed class BaristaWelcomeBrain : MonoBehaviour, INarrativeMomentPlanner<BaristaIntroPlanningRuntimeState, BaristaWelcomePlannerResult>
{
    [SerializeField] private BaristaWelcomeConfig config;
    [SerializeField] private BaristaPlannerMode plannerMode = BaristaPlannerMode.ValueIteration;
    [SerializeField] private bool verbosePlannerLogs = true;
    [SerializeField] private bool verboseStateExtraction = false;
    [SerializeField] private MonoBehaviour explicitStateSource;

    public BaristaPlannerMode PlannerMode => ResolvePlannerMode();
    public BaristaPlannerSettings CurrentPlannerSettings => ResolvePlannerSettings();

    public void SetPlannerMode(BaristaPlannerMode mode)
    {
        plannerMode = mode;

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
        BaristaIntroPlanningRuntimeState runtimeState = ExtractRuntimeState();
        BaristaPlannerEvaluation evaluation = Evaluate(runtimeState);

        Debug.Log(
            "[ROAE][BaristaWelcomeBrain] planner=" + PlannerMode +
            " extractedState={" + runtimeState.ToDebugString() + "}" +
            " evaluation={" + evaluation.BuildDebugString() + "}");

        return evaluation.mappedTone;
    }

    public string DebugDecideActionLabel()
    {
        BaristaPlannerEvaluation evaluation = Evaluate(ExtractRuntimeState());
        return PlannerMode + " -> " + evaluation.BuildDebugString();
    }

    public BaristaWelcomePlannerResult ResolveCurrentOutcome()
    {
        return ResolveOutcome(ExtractRuntimeState());
    }

    public BaristaIntroPlanningRuntimeState BuildCurrentRuntimeState()
    {
        return ExtractRuntimeState();
    }

    public BaristaWelcomePlannerResult ResolveCurrentResult()
    {
        return ResolveCurrentOutcome();
    }

    public BaristaWelcomePlannerResult ResolveOutcome(BaristaIntroPlanningRuntimeState runtimeState)
    {
        return BaristaWelcomeOutcomeResolver.Resolve(
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
            ResolvePlannerSettings());
    }

    public BaristaWelcomePlannerResult ResolveResult(BaristaIntroPlanningRuntimeState runtimeState)
    {
        return ResolveOutcome(runtimeState);
    }

    public BaristaPlannerEvaluation Evaluate(BaristaIntroPlanningRuntimeState runtimeState)
    {
        return BaristaIntroPlanningSolvers.Evaluate(
            runtimeState,
            ResolvePlannerMode(),
            ResolvePlannerSettings(),
            verbosePlannerLogs);
    }

    private BaristaIntroPlanningRuntimeState ExtractRuntimeState()
    {
        CreativeCore creativeCore = CreativeCore.Instance;

        BaristaIntroPlanningRuntimeState runtimeState = new BaristaIntroPlanningRuntimeState
        {
            readUnknownText = BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.ReadUnknownText01),
            creativity = creativeCore != null ? creativeCore.Creativity : PlayerPrefs.GetInt("creativity", 50),
            corruption = creativeCore != null ? creativeCore.PlantCorruption : PlayerPrefs.GetInt("plantCorruption", 0),
            empathy = creativeCore != null ? creativeCore.Empathy : PlayerPrefs.GetInt("empathy", 0),
            relationship = BaristaWelcomeState.GetBaristaRelationship(),
            introDone = BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.BaristaIntroDone),
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
        return config != null ? config.plannerMode : plannerMode;
    }

    private BaristaPlannerSettings ResolvePlannerSettings()
    {
        return config != null ? config.ToPlannerSettings() : BaristaPlannerSettings.Default;
    }
}
