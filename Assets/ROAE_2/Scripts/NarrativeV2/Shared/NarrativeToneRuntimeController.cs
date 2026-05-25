using UnityEngine;

public class NarrativeToneRuntimeController : MonoBehaviour
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
            CreativeCore.Instance.ForceSetStats(
                CreativeStatScale.DevResetCreativity,
                CreativeStatScale.DevResetEmpathy,
                CreativeStatScale.DevResetCorruption);

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

    public void SetRelationship(int value)
    {
        string npcId = ResolveActiveNpcId();
        NpcRelationshipState.SetRelationship(npcId, value);

        if (npcId == "barista")
            BaristaWelcomeState.SetBaristaRelationship(value);

        Log("relationship[" + npcId + "] = " + value);
        PrintCurrentState();
    }

    public void SetBaristaRelationship(int value)
    {
        SetRelationship(value);
    }

    public void SetHasDrink(bool value)
    {
        BaristaWelcomeState.SetHasDrink(value);
        Log("has_drink = " + value);
        PrintCurrentState();
    }

    public void SetAccepted(bool value)
    {
        SetHasDrink(value);
    }

    public void SetHeldDrink(BaristaDrinkType drink)
    {
        SetHasDrink(drink != BaristaDrinkType.None);
    }

    public void SetPendingDrink(BaristaDrinkType drink)
    {
        SetHasDrink(drink != BaristaDrinkType.None);
    }

    public void SetDrinkState(BaristaDrinkType heldDrink, BaristaDrinkType pendingDrink)
    {
        SetHasDrink(heldDrink != BaristaDrinkType.None || pendingDrink != BaristaDrinkType.None);
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
        Log("stored_tone = " + tone);
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
            Debug.LogWarning("[ROAE][NarrativeToneRuntimeController] Missing brain.");
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
            Debug.LogWarning("[ROAE][NarrativeToneRuntimeController] Missing brain.");
            return;
        }

        brain.UsePolicyIteration();
        Log("planner = PolicyIteration");
    }

    public void SetCorruption(int value)
    {
        if (CreativeCore.Instance == null)
        {
            Debug.LogWarning("[ROAE][NarrativeToneRuntimeController] CreativeCore.Instance missing.");
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
            NpcTonePlanningRuntimeState runtimeState = BuildRuntimeState();
            NpcTonePlannerEvaluation evaluation = NarrativeTonePlanningSolvers.Evaluate(
                runtimeState,
                toneController.ActiveProfile,
                false,
                true);

            BaristaIntroTone resolvedTone = toneController.ResolveTone(evaluation);
            BaristaWelcomeState.SetIntroTone(resolvedTone);
            Debug.Log("[ROAE][NarrativeToneRuntimeController] Resolved tone = " + resolvedTone);
            return;
        }

        if (brain == null)
        {
            Debug.LogWarning("[ROAE][NarrativeToneRuntimeController] Missing brain.");
            return;
        }

        BaristaIntroTone tone = brain.DecideOpeningTone();
        BaristaWelcomeState.SetIntroTone(tone);
        Debug.Log("[ROAE][NarrativeToneRuntimeController] Resolved tone = " + tone);
    }

    public void PrintCurrentState()
    {
        CreativeCore creativeCore = CreativeCore.Instance;
        int creativity = creativeCore != null ? creativeCore.Creativity : -999;
        int empathy = creativeCore != null ? creativeCore.Empathy : -999;
        int corruption = creativeCore != null ? creativeCore.PlantCorruption : -999;
        string npcId = ResolveActiveNpcId();
        int relationship = NpcRelationshipState.GetRelationshipScore(npcId);

        Debug.Log(
            "[ROAE][NarrativeToneRuntimeController] State | npc=" + npcId +
            " creativity=" + creativity +
            " empathy=" + empathy +
            " corruption=" + corruption +
            " relationship=" + relationship +
            " readUnknownText=" + BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.ReadUnknownText01) +
            " introDone=" + ReadCurrentIntroDoneState() +
            " hasDrink=" + BaristaWelcomeState.HasDrink() +
            " tone=" + BaristaWelcomeState.GetIntroTone() +
            " chapter=" + NarrativeProgressState.GetCurrentChapterId() +
            " scene=" + NarrativeProgressState.GetCurrentSceneId() +
            " moment=" + NarrativeProgressState.GetCurrentMomentId());
    }

    private NpcTonePlanningRuntimeState BuildRuntimeState()
    {
        NpcFactContext factContext = NpcFactContext.BuildLive(ResolveActiveNpcId());
        return new NpcTonePlanningRuntimeState
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
    }

    private void Log(string msg)
    {
        if (!debugLog)
            return;

        Debug.Log("[ROAE][NarrativeToneRuntimeController] " + msg);
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
        return PlayerPrefs.GetInt(ResolveIntroDoneFlagKey(), 0) == 1;
    }

    private string ResolveActiveNpcId()
    {
        if (toneController != null && toneController.ActiveProfile != null)
            return toneController.ActiveProfile.NpcIdOrDefault;

        return "barista";
    }
}
