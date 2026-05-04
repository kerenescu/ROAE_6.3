using UnityEngine;

public class BaristaWelcomeController : MonoBehaviour
{
    [SerializeField] private BaristaWelcomeBrain brain;
    [SerializeField] private NpcToneDialogueController toneController;
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
        string introDoneFlagKey = ResolveIntroDoneFlagKey();
        PlayerPrefs.SetInt(introDoneFlagKey, value ? 1 : 0);
        PlayerPrefs.Save();
        Log(introDoneFlagKey + " = " + value);
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
        if (toneController != null)
        {
            toneController.SetPlannerMode(BaristaPlannerMode.ValueIteration);
            Log("planner = ValueIteration");
            return;
        }

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
        if (toneController != null)
        {
            toneController.SetPlannerMode(BaristaPlannerMode.PolicyIteration);
            Log("planner = PolicyIteration");
            return;
        }

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
        if (toneController != null)
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

            NpcTonePlannerEvaluation evaluation = NpcTonePlanningSolvers.Evaluate(
                runtimeState,
                toneController.ResolvePlannerMode(),
                toneController.ResolvePlannerSettings(),
                false,
                true);

            BaristaIntroTone resolvedTone = toneController.ResolveTone(evaluation);
            BaristaWelcomeState.SetIntroTone(resolvedTone);
            Debug.Log("[ROAE][BaristaWelcomeController] Resolved tone = " + resolvedTone);
            return;
        }

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
        bool introDone = ReadCurrentIntroDoneState();
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

    private string ResolveIntroDoneFlagKey()
    {
        if (toneController != null &&
            toneController.TryGetActiveIntroDoneFlagKey(out string introDoneFlagKey) &&
            !string.IsNullOrWhiteSpace(introDoneFlagKey))
        {
            return introDoneFlagKey;
        }

        return BaristaWelcomeKeys.BaristaIntroDone;
    }

    private bool ReadCurrentIntroDoneState()
    {
        string introDoneFlagKey = ResolveIntroDoneFlagKey();
        return PlayerPrefs.GetInt(introDoneFlagKey, 0) == 1;
    }
}
