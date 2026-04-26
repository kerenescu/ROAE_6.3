using UnityEngine;

public interface IBaristaDialogueCollection
{
    DialogueData NeutralIntroDialogue { get; }
    DialogueData WarmIntroDialogue { get; }
    DialogueData MischievousIntroDialogue { get; }

    DialogueData NeutralOrderMenuDialogue { get; }
    DialogueData WarmOrderMenuDialogue { get; }
    DialogueData MischievousOrderMenuDialogue { get; }

    DialogueData NeutralPreparingDialogue { get; }
    DialogueData WarmPreparingDialogue { get; }
    DialogueData MischievousPreparingDialogue { get; }

    DialogueData NeutralPendingReminderDialogue { get; }
    DialogueData WarmPendingReminderDialogue { get; }
    DialogueData MischievousPendingReminderDialogue { get; }

    DialogueData AlreadyHasColaDialogue { get; }
    DialogueData AlreadyHasSapDialogue { get; }
    DialogueData GenericAlreadyHasDrinkDialogue { get; }
}

public enum BaristaDialogueLoop
{
    Intro = 0,
    Order = 1,
    Preparing = 2,
    PreparingReminder = 3,
    AlreadyHasDrink = 4
}

public readonly struct BaristaDialogueResolution
{
    public readonly BaristaDialogueLoop loop;
    public readonly BaristaIntroTone tone;
    public readonly DialogueData dialogue;
    public readonly string reason;

    public BaristaDialogueResolution(
        BaristaDialogueLoop loop,
        BaristaIntroTone tone,
        DialogueData dialogue,
        string reason)
    {
        this.loop = loop;
        this.tone = tone;
        this.dialogue = dialogue;
        this.reason = reason ?? string.Empty;
    }

    public string ToDebugString()
    {
        return "loop=" + loop +
               " tone=" + tone +
               " dialogue=" + NameOf(dialogue) +
               " reason=" + reason;
    }

    private static string NameOf(Object obj)
    {
        return obj != null ? obj.name : "NULL";
    }
}

public static class BaristaDialogueResolver
{
    public static BaristaDialogueResolution Resolve(
        IBaristaDialogueCollection collection,
        BaristaWelcomeBrain brain,
        BaristaMomentToneMode toneMode)
    {
        if (collection == null)
            return new BaristaDialogueResolution(
                BaristaDialogueLoop.Intro,
                BaristaIntroTone.Neutral,
                null,
                "missing_dialogue_collection");

        bool momentIntroDone = IsMomentIntroDone(collection);

        if (toneMode == BaristaMomentToneMode.UseBrainOnIntroAndStoredLoop && brain != null)
        {
            BaristaIntroPlanningRuntimeState runtimeState = brain.BuildCurrentRuntimeState();
            runtimeState.introDone = momentIntroDone;

            BaristaWelcomePlannerResult plannerResult = brain.ResolveOutcome(runtimeState);
            BaristaIntroTone plannerTone = NormalizeTone(plannerResult.introTone);
            DialogueData plannerDialogue = ResolveDialogueByOutcome(collection, plannerResult.outcomeType);

            if (plannerDialogue != null)
            {
                BaristaWelcomeState.SetIntroTone(plannerTone);

                return new BaristaDialogueResolution(
                    MapOutcomeToLoop(plannerResult.outcomeType),
                    plannerTone,
                    plannerDialogue,
                    plannerResult.reason);
            }
        }

        BaristaDrinkType heldDrink = BaristaWelcomeState.GetHeldDrink();
        if (heldDrink == BaristaDrinkType.Cola)
        {
            return new BaristaDialogueResolution(
                BaristaDialogueLoop.AlreadyHasDrink,
                ResolveLoopTone(brain, toneMode),
                FirstAssigned(collection.AlreadyHasColaDialogue, collection.GenericAlreadyHasDrinkDialogue),
                "heldDrink=Cola");
        }

        if (heldDrink == BaristaDrinkType.PhotosyntheticSap)
        {
            return new BaristaDialogueResolution(
                BaristaDialogueLoop.AlreadyHasDrink,
                ResolveLoopTone(brain, toneMode),
                FirstAssigned(collection.AlreadyHasSapDialogue, collection.GenericAlreadyHasDrinkDialogue),
                "heldDrink=PhotosyntheticSap");
        }

        if (!momentIntroDone)
        {
            BaristaIntroTone introTone = ResolveIntroTone(brain, toneMode);
            BaristaWelcomeState.SetIntroTone(introTone);

            return new BaristaDialogueResolution(
                BaristaDialogueLoop.Intro,
                introTone,
                ResolveIntroDialogueByTone(collection, introTone),
                "intro_pending");
        }

        BaristaIntroTone loopTone = ResolveLoopTone(brain, toneMode);
        if (BaristaWelcomeState.HasAcceptedFirstDrink())
        {
            if (BaristaWelcomeState.HasAcknowledgedPendingDrink())
            {
                return new BaristaDialogueResolution(
                    BaristaDialogueLoop.PreparingReminder,
                    loopTone,
                    ResolvePendingReminderDialogueByTone(collection, loopTone),
                    "drink_delivery_reminder");
            }

            return new BaristaDialogueResolution(
                BaristaDialogueLoop.Preparing,
                loopTone,
                ResolvePreparingDialogueByTone(collection, loopTone),
                "drink_delivery_initial_ack");
        }

        return new BaristaDialogueResolution(
            BaristaDialogueLoop.Order,
            loopTone,
            ResolveOrderDialogueByTone(collection, loopTone),
            "order_loop");
    }

    public static BaristaIntroTone ResolveIntroTone(BaristaWelcomeBrain brain, BaristaMomentToneMode toneMode)
    {
        switch (toneMode)
        {
            case BaristaMomentToneMode.ForceNeutral:
                return BaristaIntroTone.Neutral;
            case BaristaMomentToneMode.ForceWarm:
                return BaristaIntroTone.Warm;
            case BaristaMomentToneMode.ForceMischievous:
                return BaristaIntroTone.Mischievous;
            default:
                return brain != null
                    ? NormalizeTone(brain.DecideOpeningTone())
                    : BaristaIntroTone.Neutral;
        }
    }

    public static BaristaIntroTone ResolveLoopTone(BaristaWelcomeBrain brain, BaristaMomentToneMode toneMode)
    {
        switch (toneMode)
        {
            case BaristaMomentToneMode.ForceNeutral:
                return BaristaIntroTone.Neutral;
            case BaristaMomentToneMode.ForceWarm:
                return BaristaIntroTone.Warm;
            case BaristaMomentToneMode.ForceMischievous:
                return BaristaIntroTone.Mischievous;
            default:
                BaristaIntroTone storedTone = NormalizeTone(BaristaWelcomeState.GetIntroTone());
                if (storedTone == BaristaIntroTone.Neutral && brain != null)
                {
                    BaristaIntroTone predictedTone = NormalizeTone(brain.DecideOpeningTone());
                    BaristaWelcomeState.SetIntroTone(predictedTone);
                    return predictedTone;
                }

                return storedTone;
        }
    }

    public static BaristaIntroTone NormalizeTone(BaristaIntroTone tone)
    {
        if (tone != BaristaIntroTone.Neutral &&
            tone != BaristaIntroTone.Warm &&
            tone != BaristaIntroTone.Mischievous)
            return BaristaIntroTone.Neutral;

        return tone;
    }

    public static DialogueData ResolveIntroDialogueByTone(IBaristaDialogueCollection collection, BaristaIntroTone tone)
    {
        switch (tone)
        {
            case BaristaIntroTone.Warm:
                return FirstAssigned(collection.WarmIntroDialogue, collection.NeutralIntroDialogue, collection.MischievousIntroDialogue);
            case BaristaIntroTone.Mischievous:
                return FirstAssigned(collection.MischievousIntroDialogue, collection.NeutralIntroDialogue, collection.WarmIntroDialogue);
            default:
                return FirstAssigned(collection.NeutralIntroDialogue, collection.WarmIntroDialogue, collection.MischievousIntroDialogue);
        }
    }

    public static DialogueData ResolveOrderDialogueByTone(IBaristaDialogueCollection collection, BaristaIntroTone tone)
    {
        switch (tone)
        {
            case BaristaIntroTone.Warm:
                return FirstAssigned(collection.WarmOrderMenuDialogue, collection.NeutralOrderMenuDialogue, collection.MischievousOrderMenuDialogue);
            case BaristaIntroTone.Mischievous:
                return FirstAssigned(collection.MischievousOrderMenuDialogue, collection.NeutralOrderMenuDialogue, collection.WarmOrderMenuDialogue);
            default:
                return FirstAssigned(collection.NeutralOrderMenuDialogue, collection.WarmOrderMenuDialogue, collection.MischievousOrderMenuDialogue);
        }
    }

    public static DialogueData ResolvePreparingDialogueByTone(IBaristaDialogueCollection collection, BaristaIntroTone tone)
    {
        switch (tone)
        {
            case BaristaIntroTone.Warm:
                return FirstAssigned(collection.WarmPreparingDialogue, collection.NeutralPreparingDialogue, collection.MischievousPreparingDialogue);
            case BaristaIntroTone.Mischievous:
                return FirstAssigned(collection.MischievousPreparingDialogue, collection.NeutralPreparingDialogue, collection.WarmPreparingDialogue);
            default:
                return FirstAssigned(collection.NeutralPreparingDialogue, collection.WarmPreparingDialogue, collection.MischievousPreparingDialogue);
        }
    }

    public static DialogueData ResolvePendingReminderDialogueByTone(IBaristaDialogueCollection collection, BaristaIntroTone tone)
    {
        switch (tone)
        {
            case BaristaIntroTone.Warm:
                return FirstAssigned(
                    collection.WarmPendingReminderDialogue,
                    collection.NeutralPendingReminderDialogue,
                    collection.MischievousPendingReminderDialogue,
                    collection.WarmPreparingDialogue,
                    collection.NeutralPreparingDialogue,
                    collection.MischievousPreparingDialogue);
            case BaristaIntroTone.Mischievous:
                return FirstAssigned(
                    collection.MischievousPendingReminderDialogue,
                    collection.NeutralPendingReminderDialogue,
                    collection.WarmPendingReminderDialogue,
                    collection.MischievousPreparingDialogue,
                    collection.NeutralPreparingDialogue,
                    collection.WarmPreparingDialogue);
            default:
                return FirstAssigned(
                    collection.NeutralPendingReminderDialogue,
                    collection.WarmPendingReminderDialogue,
                    collection.MischievousPendingReminderDialogue,
                    collection.NeutralPreparingDialogue,
                    collection.WarmPreparingDialogue,
                    collection.MischievousPreparingDialogue);
        }
    }

    public static DialogueData FirstAssigned(params DialogueData[] options)
    {
        if (options == null)
            return null;

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null)
                return options[i];
        }

        return null;
    }

    private static bool IsMomentIntroDone(IBaristaDialogueCollection collection)
    {
        if (collection is BaristaMomentDefinition momentDefinition &&
            !string.IsNullOrWhiteSpace(momentDefinition.IntroCompletionFlagKey))
        {
            return PlayerPrefs.GetInt(momentDefinition.IntroCompletionFlagKey, 0) == 1;
        }

        return BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.BaristaIntroDone);
    }

    private static BaristaDialogueLoop MapOutcomeToLoop(BaristaWelcomeOutcomeType outcomeType)
    {
        switch (outcomeType)
        {
            case BaristaWelcomeOutcomeType.NeutralIntro:
            case BaristaWelcomeOutcomeType.WarmIntro:
            case BaristaWelcomeOutcomeType.MischievousIntro:
                return BaristaDialogueLoop.Intro;

            case BaristaWelcomeOutcomeType.NeutralPendingDrinkInitial:
            case BaristaWelcomeOutcomeType.WarmPendingDrinkInitial:
            case BaristaWelcomeOutcomeType.MischievousPendingDrinkInitial:
                return BaristaDialogueLoop.Preparing;

            case BaristaWelcomeOutcomeType.NeutralPendingDrinkReminder:
            case BaristaWelcomeOutcomeType.WarmPendingDrinkReminder:
            case BaristaWelcomeOutcomeType.MischievousPendingDrinkReminder:
                return BaristaDialogueLoop.PreparingReminder;

            case BaristaWelcomeOutcomeType.AlreadyHasCola:
            case BaristaWelcomeOutcomeType.AlreadyHasPhotosyntheticSap:
                return BaristaDialogueLoop.AlreadyHasDrink;

            default:
                return BaristaDialogueLoop.Order;
        }
    }

    private static DialogueData ResolveDialogueByOutcome(
        IBaristaDialogueCollection collection,
        BaristaWelcomeOutcomeType outcomeType)
    {
        switch (outcomeType)
        {
            case BaristaWelcomeOutcomeType.WarmIntro:
                return FirstAssigned(collection.WarmIntroDialogue, collection.NeutralIntroDialogue, collection.MischievousIntroDialogue);
            case BaristaWelcomeOutcomeType.MischievousIntro:
                return FirstAssigned(collection.MischievousIntroDialogue, collection.NeutralIntroDialogue, collection.WarmIntroDialogue);
            case BaristaWelcomeOutcomeType.NeutralIntro:
                return FirstAssigned(collection.NeutralIntroDialogue, collection.WarmIntroDialogue, collection.MischievousIntroDialogue);

            case BaristaWelcomeOutcomeType.WarmOrderMenu:
                return FirstAssigned(collection.WarmOrderMenuDialogue, collection.NeutralOrderMenuDialogue, collection.MischievousOrderMenuDialogue);
            case BaristaWelcomeOutcomeType.MischievousOrderMenu:
                return FirstAssigned(collection.MischievousOrderMenuDialogue, collection.NeutralOrderMenuDialogue, collection.WarmOrderMenuDialogue);
            case BaristaWelcomeOutcomeType.NeutralOrderMenu:
                return FirstAssigned(collection.NeutralOrderMenuDialogue, collection.WarmOrderMenuDialogue, collection.MischievousOrderMenuDialogue);

            case BaristaWelcomeOutcomeType.WarmPendingDrinkInitial:
                return FirstAssigned(collection.WarmPreparingDialogue, collection.NeutralPreparingDialogue, collection.MischievousPreparingDialogue);
            case BaristaWelcomeOutcomeType.MischievousPendingDrinkInitial:
                return FirstAssigned(collection.MischievousPreparingDialogue, collection.NeutralPreparingDialogue, collection.WarmPreparingDialogue);
            case BaristaWelcomeOutcomeType.NeutralPendingDrinkInitial:
                return FirstAssigned(collection.NeutralPreparingDialogue, collection.WarmPreparingDialogue, collection.MischievousPreparingDialogue);

            case BaristaWelcomeOutcomeType.WarmPendingDrinkReminder:
                return FirstAssigned(
                    collection.WarmPendingReminderDialogue,
                    collection.NeutralPendingReminderDialogue,
                    collection.MischievousPendingReminderDialogue,
                    collection.WarmPreparingDialogue,
                    collection.NeutralPreparingDialogue,
                    collection.MischievousPreparingDialogue);
            case BaristaWelcomeOutcomeType.MischievousPendingDrinkReminder:
                return FirstAssigned(
                    collection.MischievousPendingReminderDialogue,
                    collection.NeutralPendingReminderDialogue,
                    collection.WarmPendingReminderDialogue,
                    collection.MischievousPreparingDialogue,
                    collection.NeutralPreparingDialogue,
                    collection.WarmPreparingDialogue);
            case BaristaWelcomeOutcomeType.NeutralPendingDrinkReminder:
                return FirstAssigned(
                    collection.NeutralPendingReminderDialogue,
                    collection.WarmPendingReminderDialogue,
                    collection.MischievousPendingReminderDialogue,
                    collection.NeutralPreparingDialogue,
                    collection.WarmPreparingDialogue,
                    collection.MischievousPreparingDialogue);

            case BaristaWelcomeOutcomeType.AlreadyHasCola:
                return FirstAssigned(collection.AlreadyHasColaDialogue, collection.GenericAlreadyHasDrinkDialogue);
            case BaristaWelcomeOutcomeType.AlreadyHasPhotosyntheticSap:
                return FirstAssigned(collection.AlreadyHasSapDialogue, collection.GenericAlreadyHasDrinkDialogue);

            default:
                return null;
        }
    }
}
