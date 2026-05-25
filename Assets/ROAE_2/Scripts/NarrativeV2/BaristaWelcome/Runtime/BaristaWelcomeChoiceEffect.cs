using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public enum BaristaWelcomeChoiceCommand
{
    None = 0,
    MarkIntroDone = 1,
    ApplyNaiveResponse = 2,
    ApplyGuardedResponse = 3,
    GiveAcceptedDrinkIfPossible = 4,
    OrderCola = 5,
    OrderPhotosyntheticSap = 6,
    DrinkHeldDrink = 7,
    DiscardHeldDrink = 8,
    AcknowledgePendingDrink = 9
}

[CreateAssetMenu(fileName = "BaristaWelcomeChoiceEffect", menuName = "ROAE/Barista Welcome/Choice Effect")]
public class BaristaWelcomeChoiceEffect : DialogueChoiceEffect
{
    [SerializeField] private BaristaWelcomeChoiceCommand command = BaristaWelcomeChoiceCommand.None;
    [SerializeField] private bool debugLog = true;

    public override void Apply()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        string beforeState = CaptureStateSnapshot();

        switch (command)
        {
            case BaristaWelcomeChoiceCommand.MarkIntroDone:
                BaristaWelcomeState.SetFlag(BaristaWelcomeKeys.BaristaIntroDone, true);
                Log("MarkIntroDone", beforeState, stopwatch.Elapsed.TotalMilliseconds);
                break;

            case BaristaWelcomeChoiceCommand.ApplyNaiveResponse:
                BaristaWelcomeState.ApplyNaiveResponseEffects();
                Log("ApplyNaiveResponse", beforeState, stopwatch.Elapsed.TotalMilliseconds);
                break;

            case BaristaWelcomeChoiceCommand.ApplyGuardedResponse:
                BaristaWelcomeState.ApplyGuardedResponseEffects();
                Log("ApplyGuardedResponse", beforeState, stopwatch.Elapsed.TotalMilliseconds);
                break;

            case BaristaWelcomeChoiceCommand.GiveAcceptedDrinkIfPossible:
                BaristaWelcomeState.DeliverPendingDrinkIfPossible();
                Log("DeliverPendingDrinkIfPossible", beforeState, stopwatch.Elapsed.TotalMilliseconds);
                break;

            case BaristaWelcomeChoiceCommand.OrderCola:
                BaristaWelcomeState.TryOrderCola();
                Log("OrderCola", beforeState, stopwatch.Elapsed.TotalMilliseconds);
                break;

            case BaristaWelcomeChoiceCommand.OrderPhotosyntheticSap:
                BaristaWelcomeState.TryOrderPhotosyntheticSap();
                Log("OrderPhotosyntheticSap", beforeState, stopwatch.Elapsed.TotalMilliseconds);
                break;

            case BaristaWelcomeChoiceCommand.DrinkHeldDrink:
                BaristaWelcomeState.TryDrinkHeldDrink();
                Log("DrinkHeldDrink", beforeState, stopwatch.Elapsed.TotalMilliseconds);
                break;

            case BaristaWelcomeChoiceCommand.DiscardHeldDrink:
                BaristaWelcomeState.DiscardHeldDrink();
                Log("DiscardHeldDrink", beforeState, stopwatch.Elapsed.TotalMilliseconds);
                break;

            case BaristaWelcomeChoiceCommand.AcknowledgePendingDrink:
                BaristaWelcomeState.AcknowledgePendingDrink();
                Log("AcknowledgePendingDrink", beforeState, stopwatch.Elapsed.TotalMilliseconds);
                break;
        }
    }

    private void Log(string msg, string beforeState, double durationMs)
    {
        if (!debugLog) return;

        Debug.Log(
            "[ROAE][AI][BaristaWelcomeChoiceEffect][SUCCESS] command=" + msg +
            " before={" + beforeState + "}" +
            " after={" + CaptureStateSnapshot() + "}" +
            " durationMs=" + durationMs.ToString("0.00"));
    }

    private static string CaptureStateSnapshot()
    {
        return "heldDrink=" + BaristaWelcomeState.GetHeldDrink() +
               " pendingDrink=" + BaristaWelcomeState.GetPendingDrink() +
               " hasAlreadyDrink=" + BaristaWelcomeState.HasAlreadyDrink() +
               " introDone=" + BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.BaristaIntroDone) +
               " accepted=" + BaristaWelcomeState.HasAcceptedFirstDrink() +
               " pendingAcknowledged=" + BaristaWelcomeState.HasAcknowledgedPendingDrink() +
               " drankCola=" + BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.DrankCola) +
               " drankPhotosynthetic=" + BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.DrankPhotosyntheticDrink) +
               " tone=" + BaristaWelcomeState.GetIntroTone();
    }
}
