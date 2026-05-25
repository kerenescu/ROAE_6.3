using System.Text;

public enum BaristaWelcomeOutcomeType
{
    NeutralIntro,
    WarmIntro,
    MischievousIntro,
    NeutralOrderMenu,
    WarmOrderMenu,
    MischievousOrderMenu,
    NeutralPendingDrinkInitial,
    WarmPendingDrinkInitial,
    MischievousPendingDrinkInitial,
    NeutralPendingDrinkReminder,
    WarmPendingDrinkReminder,
    MischievousPendingDrinkReminder,
    AlreadyHasCola,
    AlreadyHasPhotosyntheticSap
}

[System.Serializable]
public struct BaristaWelcomePlannerInput
{
    public int creativity;
    public int empathy;
    public int corruption;
    public int relationship;
    public bool readUnknownText;
    public bool introDone;
    public bool hasDrink;
    public BaristaDrinkType pendingDrink;
    public bool pendingDrinkAcknowledged;
    public BaristaDrinkType heldDrink;
}

public sealed class BaristaWelcomePlannerResult : INarrativePlannerResult
{
    public BaristaNarrativePhase phase;
    public BaristaNarrativeAction bestAction;
    public BaristaWelcomeOutcomeType outcomeType;
    public BaristaIntroTone introTone;
    public string dialogueId;
    public string reason;

    public float neutralScore;
    public float warmScore;
    public float mischievousScore;

    public string PlannerId => "barista_welcome";
    public string PhaseId => phase.ToString();
    public string ActionId => bestAction.ToString();
    public string OutcomeId => outcomeType.ToString();
    public string DialogueId => dialogueId;
    public string Reason => reason;
    public float NeutralScore => neutralScore;
    public float WarmScore => warmScore;
    public float MischievousScore => mischievousScore;

    public string BuildDebugString()
    {
        var sb = new StringBuilder();
        sb.Append(NarrativePlannerDebugFormatter.Format(this));
        sb.Append(" tone=").Append(introTone);
        return sb.ToString();
    }
}

public static class BaristaWelcomeOutcomeResolver
{
    public static BaristaWelcomePlannerResult Resolve(BaristaWelcomePlannerInput input)
    {
        return Resolve(input, BaristaPlannerMode.ValueIteration, NpcTonePlannerSettings.Default);
    }

    public static BaristaWelcomePlannerResult Resolve(
        BaristaWelcomePlannerInput input,
        BaristaPlannerMode plannerMode,
        NpcTonePlannerSettings settings)
    {
        return Resolve(input, plannerMode, settings, false);
    }

    public static BaristaWelcomePlannerResult Resolve(
        BaristaWelcomePlannerInput input,
        BaristaPlannerMode plannerMode,
        NpcTonePlannerSettings settings,
        bool auditCacheLogs)
    {
        NpcTonePlanningRuntimeState runtimeState = ToRuntimeState(input);
        NpcTonePlannerEvaluation evaluation = NarrativeTonePlanningSolvers.Evaluate(
            runtimeState,
            plannerMode,
            settings,
            false,
            auditCacheLogs);

        var result = new BaristaWelcomePlannerResult
        {
            phase = runtimeState.Phase,
            bestAction = evaluation.bestAction,
            introTone = evaluation.mappedTone,
            neutralScore = evaluation.neutralScore,
            warmScore = evaluation.warmScore,
            mischievousScore = evaluation.mischievousScore,
            reason = "planner=" + plannerMode +
                     " phase=" + runtimeState.Phase +
                     " " + evaluation.BuildDebugString()
        };

        ApplyOutcome(runtimeState, evaluation.mappedTone, result);
        return result;
    }

    public static BaristaIntroTone ResolveTone(BaristaWelcomePlannerInput input)
    {
        return Resolve(input).introTone;
    }

    public static BaristaIntroTone ResolveTone(
        BaristaWelcomePlannerInput input,
        BaristaPlannerMode plannerMode,
        NpcTonePlannerSettings settings)
    {
        return Resolve(input, plannerMode, settings).introTone;
    }

    private static NpcTonePlanningRuntimeState ToRuntimeState(BaristaWelcomePlannerInput input)
    {
        return new NpcTonePlanningRuntimeState
        {
            readUnknownText = input.readUnknownText,
            creativity = input.creativity,
            corruption = input.corruption,
            empathy = input.empathy,
            relationship = input.relationship,
            introDone = input.introDone,
            hasDrink = input.hasDrink || input.heldDrink != BaristaDrinkType.None || input.pendingDrink != BaristaDrinkType.None,
            pendingDrink = BaristaDrinkType.None,
            pendingDrinkAcknowledged = false,
            heldDrink = input.hasDrink || input.heldDrink != BaristaDrinkType.None
                ? (input.heldDrink != BaristaDrinkType.None ? input.heldDrink : BaristaDrinkType.Cola)
                : BaristaDrinkType.None
        };
    }

    private static void ApplyOutcome(
        NpcTonePlanningRuntimeState runtimeState,
        BaristaIntroTone tone,
        BaristaWelcomePlannerResult result)
    {
        if (runtimeState.Phase == BaristaNarrativePhase.AlreadyHasDrink)
        {
            if (runtimeState.HeldDrinkTier == 1)
            {
                result.outcomeType = BaristaWelcomeOutcomeType.AlreadyHasCola;
                result.dialogueId = "BW_Already_Has_Drink_Cola";
            }
            else
            {
                result.outcomeType = BaristaWelcomeOutcomeType.AlreadyHasPhotosyntheticSap;
                result.dialogueId = "BW_Already_Has_Drink_Sap";
            }

            return;
        }

        switch (runtimeState.Phase)
        {
            case BaristaNarrativePhase.Intro:
                ApplyIntroOutcome(tone, result);
                return;

            case BaristaNarrativePhase.PendingDrinkInitial:
                ApplyPendingInitialOutcome(tone, result);
                return;

            case BaristaNarrativePhase.PendingDrinkReminder:
                ApplyPendingReminderOutcome(tone, result);
                return;

            default:
                ApplyOrderOutcome(tone, result);
                return;
        }
    }

    private static void ApplyIntroOutcome(BaristaIntroTone tone, BaristaWelcomePlannerResult result)
    {
        switch (tone)
        {
            case BaristaIntroTone.Warm:
                result.outcomeType = BaristaWelcomeOutcomeType.WarmIntro;
                result.dialogueId = "BW_Intro_Warm";
                break;

            case BaristaIntroTone.Mischievous:
                result.outcomeType = BaristaWelcomeOutcomeType.MischievousIntro;
                result.dialogueId = "BW_Intro_Mischievous";
                break;

            default:
                result.outcomeType = BaristaWelcomeOutcomeType.NeutralIntro;
                result.dialogueId = "BW_Intro_Neutral";
                break;
        }
    }

    private static void ApplyPendingInitialOutcome(BaristaIntroTone tone, BaristaWelcomePlannerResult result)
    {
        switch (tone)
        {
            case BaristaIntroTone.Warm:
                result.outcomeType = BaristaWelcomeOutcomeType.WarmPendingDrinkInitial;
                result.dialogueId = "BW_Drink_Preparing_Warm";
                break;

            case BaristaIntroTone.Mischievous:
                result.outcomeType = BaristaWelcomeOutcomeType.MischievousPendingDrinkInitial;
                result.dialogueId = "BW_Drink_Preparing_Mischievous";
                break;

            default:
                result.outcomeType = BaristaWelcomeOutcomeType.NeutralPendingDrinkInitial;
                result.dialogueId = "BW_Drink_Preparing_Neutral";
                break;
        }
    }

    private static void ApplyPendingReminderOutcome(BaristaIntroTone tone, BaristaWelcomePlannerResult result)
    {
        switch (tone)
        {
            case BaristaIntroTone.Warm:
                result.outcomeType = BaristaWelcomeOutcomeType.WarmPendingDrinkReminder;
                result.dialogueId = "BW_Drink_Reminder_Warm";
                break;

            case BaristaIntroTone.Mischievous:
                result.outcomeType = BaristaWelcomeOutcomeType.MischievousPendingDrinkReminder;
                result.dialogueId = "BW_Drink_Reminder_Mischievous";
                break;

            default:
                result.outcomeType = BaristaWelcomeOutcomeType.NeutralPendingDrinkReminder;
                result.dialogueId = "BW_Drink_Reminder_Neutral";
                break;
        }
    }

    private static void ApplyOrderOutcome(BaristaIntroTone tone, BaristaWelcomePlannerResult result)
    {
        switch (tone)
        {
            case BaristaIntroTone.Warm:
                result.outcomeType = BaristaWelcomeOutcomeType.WarmOrderMenu;
                result.dialogueId = "BW_Order_Menu_Warm";
                break;

            case BaristaIntroTone.Mischievous:
                result.outcomeType = BaristaWelcomeOutcomeType.MischievousOrderMenu;
                result.dialogueId = "BW_Order_Menu_Strange";
                break;

            default:
                result.outcomeType = BaristaWelcomeOutcomeType.NeutralOrderMenu;
                result.dialogueId = "BW_Order_Menu_Clean";
                break;
        }
    }
}
