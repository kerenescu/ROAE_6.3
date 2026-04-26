using UnityEngine;

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
        switch (command)
        {
            case BaristaWelcomeChoiceCommand.MarkIntroDone:
                BaristaWelcomeState.SetFlag(BaristaWelcomeKeys.BaristaIntroDone, true);
                Log("MarkIntroDone");
                break;

            case BaristaWelcomeChoiceCommand.ApplyNaiveResponse:
                BaristaWelcomeState.ApplyNaiveResponseEffects();
                Log("ApplyNaiveResponse");
                break;

            case BaristaWelcomeChoiceCommand.ApplyGuardedResponse:
                BaristaWelcomeState.ApplyGuardedResponseEffects();
                Log("ApplyGuardedResponse");
                break;

            case BaristaWelcomeChoiceCommand.GiveAcceptedDrinkIfPossible:
                BaristaWelcomeState.DeliverPendingDrinkIfPossible();
                Log("DeliverPendingDrinkIfPossible");
                break;

            case BaristaWelcomeChoiceCommand.OrderCola:
                BaristaWelcomeState.TryOrderCola();
                Log("OrderCola");
                break;

            case BaristaWelcomeChoiceCommand.OrderPhotosyntheticSap:
                BaristaWelcomeState.TryOrderPhotosyntheticSap();
                Log("OrderPhotosyntheticSap");
                break;

            case BaristaWelcomeChoiceCommand.DrinkHeldDrink:
                BaristaWelcomeState.TryDrinkHeldDrink();
                Log("DrinkHeldDrink");
                break;

            case BaristaWelcomeChoiceCommand.DiscardHeldDrink:
                BaristaWelcomeState.DiscardHeldDrink();
                Log("DiscardHeldDrink");
                break;

            case BaristaWelcomeChoiceCommand.AcknowledgePendingDrink:
                BaristaWelcomeState.AcknowledgePendingDrink();
                Log("AcknowledgePendingDrink");
                break;
        }
    }

    private void Log(string msg)
    {
        if (!debugLog) return;

        Debug.Log(
            "[ROAE][BaristaWelcomeChoiceEffect] " + msg +
            " | heldDrink=" + BaristaWelcomeState.GetHeldDrink() +
            " | pendingDrink=" + BaristaWelcomeState.GetPendingDrink() +
            " | hasAlreadyDrink=" + BaristaWelcomeState.HasAlreadyDrink() +
            " | introDone=" + BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.BaristaIntroDone) +
            " | accepted=" + BaristaWelcomeState.HasAcceptedFirstDrink() +
            " | drankCola=" + BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.DrankCola) +
            " | drankPhotosynthetic=" + BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.DrankPhotosyntheticDrink) +
            " | tone=" + BaristaWelcomeState.GetIntroTone());
    }
}
