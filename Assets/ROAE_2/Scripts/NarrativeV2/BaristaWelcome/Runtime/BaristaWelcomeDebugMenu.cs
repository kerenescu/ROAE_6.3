// ============================================================
//  BaristaWelcomeDebugMenu  –  ROAE refactor
//  Adăugat: previewRelationship + preset Warm corect
// ============================================================

using UnityEngine;

public class BaristaWelcomeDebugMenu : MonoBehaviour
{
    private const string BaristaNpcId = "barista";
    private const string BaristaSecondMomentId = "barista_second_visit";
    private const string BarInteriorSceneId = "Bar_Interior";

    [Header("Optional refs")]
    [SerializeField] private CreativeCore creativeCore;
    [SerializeField] private NarrativeToneRuntimeController controller;
    [SerializeField] private BaristaWelcomeBrain brain;
    [SerializeField] private NpcToneDialogueController toneController;

    [Header("Preview input")]
    [SerializeField] private int previewCreativity = CreativeStatScale.DevResetCreativity;
    [SerializeField] private int previewEmpathy = CreativeStatScale.DevResetEmpathy;
    [SerializeField] private int previewCorruption = CreativeStatScale.DevResetCorruption;
    [SerializeField] private int previewRelationship = 0;   // ← NOU
    [SerializeField] private bool previewReadUnknownText = false;
    [SerializeField] private bool previewIntroDone = false;
    [SerializeField] private bool previewHasDrink = false;

    // ── Context menu actions ──────────────────────────────────────────────────

    [ContextMenu("ROAE/Barista/Preview current input")]
    public void PreviewCurrentInput()
    {
        var input = BuildInput();
        var result = ResolvePreview();
        Debug.Log(
            "[ROAE][BaristaWelcomeDebugMenu] input{" +
            "creativity=" + input.creativity +
            " empathy=" + input.empathy +
            " corruption=" + input.corruption +
            " relationship=" + input.relationship +
            " readUnknownText=" + input.readUnknownText +
            " introDone=" + input.introDone +
            " hasDrink=" + previewHasDrink +
            "} -> " + result.BuildDebugString());
    }

    [ContextMenu("ROAE/Barista/Apply preview to runtime")]
    public void ApplyPreviewToRuntime()
    {
        var result = ResolvePreview();
        CreativeCore activeCreativeCore = ResolveCreativeCore();

        if (activeCreativeCore != null)
            activeCreativeCore.ForceSetStats(previewCreativity, previewEmpathy, previewCorruption);

        if (controller != null)
        {
            controller.SetReadUnknownText(previewReadUnknownText);
            controller.SetIntroDone(previewIntroDone);
            controller.SetBaristaRelationship(previewRelationship);
            controller.SetHasDrink(previewHasDrink);
            controller.SetTone(result.introTone);
            controller.PrintCurrentState();
        }
        else
        {
            BaristaWelcomeState.SetFlag(BaristaWelcomeKeys.ReadUnknownText01, previewReadUnknownText);
            BaristaWelcomeState.SetFlag(BaristaWelcomeKeys.BaristaIntroDone, previewIntroDone);
            BaristaWelcomeState.SetBaristaRelationship(previewRelationship);
            BaristaWelcomeState.SetHasDrink(previewHasDrink);
            BaristaWelcomeState.SetIntroTone(result.introTone);
        }

        PreviewCurrentInput();
    }

    // ── Preseturi ─────────────────────────────────────────────────────────────

    [ContextMenu("ROAE/Barista/Neutral preset")]
    public void SetNeutralCase()
    {
        previewCreativity = 40; previewEmpathy = 50; previewCorruption = 0;
        previewRelationship = 0; previewReadUnknownText = false;
        previewIntroDone = false; previewHasDrink = false;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Neutral preset loaded.");
    }

    /// Warm: empathy ridicat SAU relationship pozitiv
    [ContextMenu("ROAE/Barista/Warm preset (high empathy)")]
    public void SetWarmCaseEmpathy()
    {
        previewCreativity = 40; previewEmpathy = 80; previewCorruption = 0;
        previewRelationship = 0; previewReadUnknownText = false;
        previewIntroDone = false; previewHasDrink = false;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Warm preset (high empathy) loaded.");
    }

    [ContextMenu("ROAE/Barista/Warm preset (good relationship)")]
    public void SetWarmCaseRelationship()
    {
        previewCreativity = 40; previewEmpathy = 60; previewCorruption = 0;
        previewRelationship = 5; previewReadUnknownText = false;
        previewIntroDone = false; previewHasDrink = false;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Warm preset (good relationship) loaded.");
    }

    [ContextMenu("ROAE/Barista/Mischievous preset by knowledge")]
    public void SetMischievousKnowledgeCase()
    {
        previewCreativity = 40; previewEmpathy = 50; previewCorruption = 0;
        previewRelationship = 0; previewReadUnknownText = true;
        previewIntroDone = false; previewHasDrink = false;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Mischievous preset by knowledge loaded.");
    }

    [ContextMenu("ROAE/Barista/Mischievous preset by corruption")]
    public void SetMischievousCorruptionCase()
    {
        previewCreativity = 40; previewEmpathy = 40; previewCorruption = 80;
        previewRelationship = 0; previewReadUnknownText = false;
        previewIntroDone = false; previewHasDrink = false;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Mischievous preset by corruption loaded.");
    }

    [ContextMenu("ROAE/Barista/Mischievous preset (guaranteed)")]
    public void SetMischievousGuaranteedCase()
    {
        previewCreativity = 70; previewEmpathy = 30; previewCorruption = 80;
        previewRelationship = -3; previewReadUnknownText = true;
        previewIntroDone = false; previewHasDrink = false;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Mischievous preset (guaranteed) loaded.");
    }

    [ContextMenu("ROAE/Barista/Pending sap delivery (warm)")]
    public void SetPendingSapCase()
    {
        previewCreativity = 40; previewEmpathy = 80; previewCorruption = 0;
        previewRelationship = 3; previewReadUnknownText = false;
        previewIntroDone = true; previewHasDrink = true;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Has-drink preset (warm) loaded.");
    }

    [ContextMenu("ROAE/Barista/Holding cola preset")]
    public void SetHoldingColaCase()
    {
        previewCreativity = 40; previewEmpathy = 50; previewCorruption = 0;
        previewRelationship = 0; previewReadUnknownText = false;
        previewIntroDone = true; previewHasDrink = true;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Holding drink preset loaded.");
    }

    [ContextMenu("ROAE/Barista/Holding sap preset")]
    public void SetHoldingSapCase()
    {
        previewCreativity = 40; previewEmpathy = 50; previewCorruption = 70;
        previewRelationship = 0; previewReadUnknownText = true;
        previewIntroDone = true; previewHasDrink = true;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Holding drink preset loaded.");
    }

    [ContextMenu("ROAE/Barista/Go to moment 2 (neutral)")]
    public void GoToSecondMomentNeutral()
    {
        SetNeutralCase();
        EnterSecondMomentFromPreview();
    }

    [ContextMenu("ROAE/Barista/Go to moment 2 (warm)")]
    public void GoToSecondMomentWarm()
    {
        SetWarmCaseEmpathy();
        EnterSecondMomentFromPreview();
    }

    [ContextMenu("ROAE/Barista/Go to moment 2 (mischievous)")]
    public void GoToSecondMomentMischievous()
    {
        SetMischievousGuaranteedCase();
        EnterSecondMomentFromPreview();
    }

    [ContextMenu("ROAE/Barista/Clear forced narrative moment")]
    public void ClearForcedNarrativeMoment()
    {
        NarrativeProgressState.SetCurrentMomentId(string.Empty);
        NarrativeProgressState.ClearSceneOverride();

        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Cleared forced narrative moment.");
    }

    [ContextMenu("ROAE/Barista/Discard held drink at runtime")]
    public void DiscardHeldDrinkAtRuntime()
    {
        if (controller != null) controller.DiscardHeldDrink();
        else BaristaWelcomeState.DiscardHeldDrink();
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Held drink discarded at runtime.");
    }

    [ContextMenu("ROAE/Barista/Print runtime planner summary")]
    public void PrintRuntimePlannerSummary()
    {
        if (brain == null) { Debug.LogWarning("[ROAE][BaristaWelcomeDebugMenu] Missing brain."); return; }
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] " + brain.DebugDecideActionLabel());
    }

    [ContextMenu("ROAE/Barista AI/Print current decision")]
    public void PrintCurrentAIDecision()
    {
        if (toneController != null)
        {
            toneController.PrintCurrentDecision();
            return;
        }

        if (brain == null) { Debug.LogWarning("[ROAE][AI][BaristaDev][FAIL] reason=missing_brain"); return; }

        NpcTonePlanningRuntimeState state = brain.BuildCurrentRuntimeState();
        BaristaWelcomePlannerResult result = brain.ResolveResult(state);
        Debug.Log(
            "[ROAE][AI][BaristaDev][SUMMARY] state={" + state.ToDebugString() +
            "} result={" + result.BuildDebugString() + "}");
    }

    [ContextMenu("ROAE/Barista AI/Reset dev state and planner cache")]
    public void ResetDevStateAndPlannerCache()
    {
        ResetPreviewFields();
        NpcAIDevTools.ResetRuntimeState(40, 0, 0, new[] { BaristaNpcId, "anticar", "madame_lichenia" });
        Debug.Log("[ROAE][AI][BaristaDevReset][SUCCESS] preview=neutral runtime=reset");
    }

    [ContextMenu("ROAE/Barista/Reset state and stats")]
    public void ResetStateAndStats()
    {
        ResetDevStateAndPlannerCache();
    }

    // ── Builder ───────────────────────────────────────────────────────────────

    private void ResetPreviewFields()
    {
        previewCreativity = CreativeStatScale.DevResetCreativity;
        previewEmpathy = CreativeStatScale.DevResetEmpathy;
        previewCorruption = CreativeStatScale.DevResetCorruption;
        previewRelationship = 0;
        previewReadUnknownText = false;
        previewIntroDone = false;
        previewHasDrink = false;
    }

    private BaristaWelcomePlannerInput BuildInput()
    {
        return new BaristaWelcomePlannerInput
        {
            creativity = previewCreativity,
            empathy = previewEmpathy,
            corruption = previewCorruption,
            relationship = previewRelationship,
            readUnknownText = previewReadUnknownText,
            introDone = previewIntroDone,
            hasDrink = previewHasDrink,
            pendingDrink = BaristaDrinkType.None,
            pendingDrinkAcknowledged = false,
            heldDrink = previewHasDrink ? BaristaDrinkType.Cola : BaristaDrinkType.None
        };
    }

    private BaristaWelcomePlannerResult ResolvePreview()
    {
        BaristaWelcomePlannerInput input = BuildInput();
        if (toneController != null)
        {
            return BaristaWelcomeOutcomeResolver.Resolve(
                input,
                toneController.ResolvePlannerMode(),
                toneController.ResolvePlannerSettings());
        }

        if (brain != null)
            return BaristaWelcomeOutcomeResolver.Resolve(input, brain.PlannerMode, brain.CurrentPlannerSettings);

        return BaristaWelcomeOutcomeResolver.Resolve(input);
    }

    private CreativeCore ResolveCreativeCore()
    {
        if (CreativeCore.Instance != null)
            return CreativeCore.Instance;

        if (creativeCore != null)
            return creativeCore;

        return Object.FindFirstObjectByType<CreativeCore>();
    }

    private void EnterSecondMomentFromPreview()
    {
        previewIntroDone = false;
        previewHasDrink = false;

        NarrativeProgressState.SetCurrentMomentId(BaristaSecondMomentId);
        NarrativeProgressState.SetSceneOverride(BarInteriorSceneId);

        ApplyPreviewToRuntime();

        Debug.Log(
            "[ROAE][BaristaWelcomeDebugMenu] Second barista moment armed." +
            " moment=" + NarrativeProgressState.GetCurrentMomentId() +
            " scene=" + NarrativeProgressState.GetCurrentSceneId());
    }
}
