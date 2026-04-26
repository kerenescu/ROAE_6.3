using UnityEngine;

public class BaristaWelcomeController : MonoBehaviour
{
    [SerializeField] private BaristaWelcomeBrain brain;
    [SerializeField] private bool debugLog = true;

    public void ResetMomentState()
    {
        BaristaWelcomeState.ResetAll();
        Log("Moment reset.");
        PrintCurrentState();
    }

    public void ResetMomentAndStats()
    {
        BaristaWelcomeState.ResetAll();

        if (CreativeCore.Instance != null)
            CreativeCore.Instance.ForceSetStats(40, 0, 0);

        Log("Moment + stats reset.");
        PrintCurrentState();
    }

    public void SetReadUnknownText(bool value)
    {
        BaristaWelcomeState.SetFlag(BaristaWelcomeKeys.ReadUnknownText01, value);
        Log("read_unknown_text_01 = " + value);
        PrintCurrentState();
    }

    public void SetIntroDone(bool value)
    {
        BaristaWelcomeState.SetFlag(BaristaWelcomeKeys.BaristaIntroDone, value);
        Log("barista_intro_done = " + value);
        PrintCurrentState();
    }

    public void SetAccepted(bool value)
    {
        BaristaWelcomeState.SetAcceptedFirstDrink(value);
        Log("accepted_first_drink = " + value);
        PrintCurrentState();
    }

    public void SetBaristaRelationship(int value)
    {
        BaristaWelcomeState.SetBaristaRelationship(value);
        Log("barista_relationship = " + value);
        PrintCurrentState();
    }

    public void SetHeldDrink(BaristaDrinkType drink)
    {
        BaristaWelcomeState.SetHeldDrink(drink);
        Log("held_drink = " + drink);
        PrintCurrentState();
    }

    public void SetPendingDrink(BaristaDrinkType drink)
    {
        BaristaWelcomeState.SetPendingDrink(drink);
        Log("pending_drink = " + drink);
        PrintCurrentState();
    }

    public void SetDrinkState(BaristaDrinkType heldDrink, BaristaDrinkType pendingDrink)
    {
        BaristaWelcomeState.SetDrinkState(heldDrink, pendingDrink);
        Log("drink_state = held:" + heldDrink + " pending:" + pendingDrink);
        PrintCurrentState();
    }

    public void DeliverPendingDrink()
    {
        bool delivered = BaristaWelcomeState.DeliverPendingDrinkIfPossible();
        Log("deliver_pending_drink = " + delivered);
        PrintCurrentState();
    }

    public void DiscardHeldDrink()
    {
        BaristaWelcomeState.DiscardHeldDrink();
        Log("held drink discarded");
        PrintCurrentState();
    }

    public void SetTone(BaristaIntroTone tone)
    {
        BaristaWelcomeState.SetIntroTone(tone);
        Log("barista_intro_tone = " + tone);
        PrintCurrentState();
    }

    public void UseValueIteration()
    {
        if (brain == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeController] Missing brain.");
            return;
        }

        brain.UseValueIteration();
        Log("planner = ValueIteration");
    }

    public void UsePolicyIteration()
    {
        if (brain == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeController] Missing brain.");
            return;
        }

        brain.UsePolicyIteration();
        Log("planner = PolicyIteration");
    }

    public void SetCorruption(int value)
    {
        if (CreativeCore.Instance == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeController] CreativeCore.Instance missing.");
            return;
        }

        CreativeCore.Instance.ForceSetStats(
            CreativeCore.Instance.Creativity,
            CreativeCore.Instance.Empathy,
            value);

        Log("corruption = " + value);
        PrintCurrentState();
    }

    public void ResolveOpeningTone()
    {
        if (brain == null)
        {
            Debug.LogWarning("[ROAE][BaristaWelcomeController] Missing brain.");
            return;
        }

        BaristaIntroTone tone = brain.DecideOpeningTone();
        BaristaWelcomeState.SetIntroTone(tone);
        Debug.Log("[ROAE][BaristaWelcomeController] Resolved tone = " + tone);
    }

    public void PrintCurrentState()
    {
        int creativity = CreativeCore.Instance != null ? CreativeCore.Instance.Creativity : -999;
        int empathy = CreativeCore.Instance != null ? CreativeCore.Instance.Empathy : -999;
        int corruption = CreativeCore.Instance != null ? CreativeCore.Instance.PlantCorruption : -999;
        bool readUnknown = BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.ReadUnknownText01);
        bool introDone = BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.BaristaIntroDone);
        bool accepted = BaristaWelcomeState.HasAcceptedFirstDrink();
        bool pendingAcknowledged = BaristaWelcomeState.HasAcknowledgedPendingDrink();
        BaristaDrinkType heldDrink = BaristaWelcomeState.GetHeldDrink();
        BaristaDrinkType pendingDrink = BaristaWelcomeState.GetPendingDrink();
        BaristaIntroTone tone = BaristaWelcomeState.GetIntroTone();
        int relationship = BaristaWelcomeState.GetBaristaRelationship();

        Debug.Log(
            "[ROAE][BaristaWelcomeController] State | creativity=" + creativity +
            " empathy=" + empathy +
            " corruption=" + corruption +
            " relationship=" + relationship +
            " readUnknownText=" + readUnknown +
            " introDone=" + introDone +
            " accepted=" + accepted +
            " hasAlreadyDrink=" + BaristaWelcomeState.HasAlreadyDrink() +
            " pendingDrink=" + pendingDrink +
            " pendingAcknowledged=" + pendingAcknowledged +
            " heldDrink=" + heldDrink +
            " tone=" + tone +
            " chapter=" + NarrativeProgressState.GetCurrentChapterId() +
            " scene=" + NarrativeProgressState.GetCurrentSceneId() +
            " moment=" + NarrativeProgressState.GetCurrentMomentId());
    }

    private void Log(string msg)
    {
        if (!debugLog) return;
        Debug.Log("[ROAE][BaristaWelcomeController] " + msg);
    }
}
