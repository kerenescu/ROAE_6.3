// ============================================================
//  BaristaWelcomeDebugMenu  –  ROAE refactor
//  Adăugat: previewRelationship + preset Warm corect
// ============================================================

using UnityEngine;

public class BaristaWelcomeDebugMenu : MonoBehaviour
{
    private const string BaristaSecondMomentId = "barista_second_visit";
    private const string BarInteriorSceneId = "Bar_Interior";

    [Header("Optional refs")]
    [SerializeField] private CreativeCore creativeCore;
    [SerializeField] private BaristaWelcomeController controller;
    [SerializeField] private BaristaWelcomeBrain brain;

    [Header("Preview input")]
    [SerializeField] private int previewCreativity = 40;
    [SerializeField] private int previewEmpathy = 0;
    [SerializeField] private int previewCorruption = 0;
    [SerializeField] private int previewRelationship = 0;   // ← NOU
    [SerializeField] private bool previewReadUnknownText = false;
    [SerializeField] private bool previewIntroDone = false;
    [SerializeField] private bool previewAccepted = false;
    [SerializeField] private bool previewPendingAcknowledged = false;
    [SerializeField] private BaristaDrinkType previewHeldDrink = BaristaDrinkType.None;

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
            " pendingDrink=" + input.pendingDrink +
            " pendingAcknowledged=" + input.pendingDrinkAcknowledged +
            " heldDrink=" + input.heldDrink +
            "} -> " + result.BuildDebugString());
    }

    [ContextMenu("ROAE/Barista/Apply preview to runtime")]
    public void ApplyPreviewToRuntime()
    {
        var result = ResolvePreview();
        BaristaDrinkType pendingDrink = previewAccepted && previewHeldDrink == BaristaDrinkType.None
            ? BaristaDrinkType.PhotosyntheticSap
            : BaristaDrinkType.None;
        CreativeCore activeCreativeCore = ResolveCreativeCore();

        if (activeCreativeCore != null)
            activeCreativeCore.ForceSetStats(previewCreativity, previewEmpathy, previewCorruption);

        if (controller != null)
        {
            controller.SetReadUnknownText(previewReadUnknownText);
            controller.SetIntroDone(previewIntroDone);
            controller.SetBaristaRelationship(previewRelationship);
            controller.SetDrinkState(previewHeldDrink, pendingDrink);
            controller.SetTone(result.introTone);
            controller.PrintCurrentState();
        }
        else
        {
            BaristaWelcomeState.SetFlag(BaristaWelcomeKeys.ReadUnknownText01, previewReadUnknownText);
            BaristaWelcomeState.SetFlag(BaristaWelcomeKeys.BaristaIntroDone, previewIntroDone);
            BaristaWelcomeState.SetBaristaRelationship(previewRelationship);
            BaristaWelcomeState.SetDrinkState(previewHeldDrink, pendingDrink);
            BaristaWelcomeState.SetIntroTone(result.introTone);
        }

        if (previewPendingAcknowledged && pendingDrink != BaristaDrinkType.None)
            BaristaWelcomeState.AcknowledgePendingDrink();

        PreviewCurrentInput();
    }

    // ── Preseturi ─────────────────────────────────────────────────────────────

    [ContextMenu("ROAE/Barista/Neutral preset")]
    public void SetNeutralCase()
    {
        previewCreativity = 40; previewEmpathy = 0; previewCorruption = 0;
        previewRelationship = 0; previewReadUnknownText = false;
        previewIntroDone = false; previewAccepted = false; previewPendingAcknowledged = false; previewHeldDrink = BaristaDrinkType.None;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Neutral preset loaded.");
    }

    /// Warm: empathy ridicat SAU relationship pozitiv
    [ContextMenu("ROAE/Barista/Warm preset (high empathy)")]
    public void SetWarmCaseEmpathy()
    {
        previewCreativity = 40; previewEmpathy = 3; previewCorruption = 0;
        previewRelationship = 0; previewReadUnknownText = false;
        previewIntroDone = false; previewAccepted = false; previewPendingAcknowledged = false; previewHeldDrink = BaristaDrinkType.None;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Warm preset (high empathy) loaded.");
    }

    [ContextMenu("ROAE/Barista/Warm preset (good relationship)")]
    public void SetWarmCaseRelationship()
    {
        previewCreativity = 40; previewEmpathy = 1; previewCorruption = 0;
        previewRelationship = 5; previewReadUnknownText = false;
        previewIntroDone = false; previewAccepted = false; previewPendingAcknowledged = false; previewHeldDrink = BaristaDrinkType.None;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Warm preset (good relationship) loaded.");
    }

    [ContextMenu("ROAE/Barista/Mischievous preset by knowledge")]
    public void SetMischievousKnowledgeCase()
    {
        previewCreativity = 40; previewEmpathy = 0; previewCorruption = 0;
        previewRelationship = 0; previewReadUnknownText = true;
        previewIntroDone = false; previewAccepted = false; previewPendingAcknowledged = false; previewHeldDrink = BaristaDrinkType.None;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Mischievous preset by knowledge loaded.");
    }

    [ContextMenu("ROAE/Barista/Mischievous preset by corruption")]
    public void SetMischievousCorruptionCase()
    {
        previewCreativity = 40; previewEmpathy = -1; previewCorruption = 5;
        previewRelationship = 0; previewReadUnknownText = false;
        previewIntroDone = false; previewAccepted = false; previewPendingAcknowledged = false; previewHeldDrink = BaristaDrinkType.None;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Mischievous preset by corruption loaded.");
    }

    [ContextMenu("ROAE/Barista/Mischievous preset (guaranteed)")]
    public void SetMischievousGuaranteedCase()
    {
        previewCreativity = 70; previewEmpathy = -2; previewCorruption = 5;
        previewRelationship = -3; previewReadUnknownText = true;
        previewIntroDone = false; previewAccepted = false; previewPendingAcknowledged = false; previewHeldDrink = BaristaDrinkType.None;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Mischievous preset (guaranteed) loaded.");
    }

    [ContextMenu("ROAE/Barista/Pending sap delivery (warm)")]
    public void SetPendingSapCase()
    {
        previewCreativity = 40; previewEmpathy = 3; previewCorruption = 0;
        previewRelationship = 3; previewReadUnknownText = false;
        previewIntroDone = true; previewAccepted = true; previewPendingAcknowledged = false; previewHeldDrink = BaristaDrinkType.None;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Pending sap delivery (warm) preset loaded.");
    }

    [ContextMenu("ROAE/Barista/Holding cola preset")]
    public void SetHoldingColaCase()
    {
        previewCreativity = 40; previewEmpathy = 0; previewCorruption = 0;
        previewRelationship = 0; previewReadUnknownText = false;
        previewIntroDone = true; previewAccepted = false; previewPendingAcknowledged = false; previewHeldDrink = BaristaDrinkType.Cola;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Holding cola preset loaded.");
    }

    [ContextMenu("ROAE/Barista/Holding sap preset")]
    public void SetHoldingSapCase()
    {
        previewCreativity = 40; previewEmpathy = 0; previewCorruption = 2;
        previewRelationship = 0; previewReadUnknownText = true;
        previewIntroDone = true; previewAccepted = false; previewPendingAcknowledged = false; previewHeldDrink = BaristaDrinkType.PhotosyntheticSap;
        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Holding sap preset loaded.");
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

    [ContextMenu("ROAE/Barista/Reset state and stats")]
    public void ResetStateAndStats()
    {
        previewCreativity = 40; previewEmpathy = 0; previewCorruption = 0;
        previewRelationship = 0; previewReadUnknownText = false;
        previewIntroDone = false; previewAccepted = false; previewPendingAcknowledged = false; previewHeldDrink = BaristaDrinkType.None;

        if (creativeCore != null) creativeCore.ForceSetStats(40, 0, 0);
        if (controller != null) controller.ResetMomentAndStats();
        else BaristaWelcomeState.ResetAll();

        NarrativeProgressState.SetCurrentMomentId(string.Empty);
        NarrativeProgressState.ClearSceneOverride();

        Debug.Log("[ROAE][BaristaWelcomeDebugMenu] Preview and runtime reset.");
    }

    // ── Builder ───────────────────────────────────────────────────────────────

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
            pendingDrink = previewAccepted && previewHeldDrink == BaristaDrinkType.None
                ? BaristaDrinkType.PhotosyntheticSap
                : BaristaDrinkType.None,
            pendingDrinkAcknowledged = previewPendingAcknowledged,
            heldDrink = previewHeldDrink
        };
    }

    private BaristaWelcomePlannerResult ResolvePreview()
    {
        BaristaWelcomePlannerInput input = BuildInput();
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
        previewAccepted = false;
        previewPendingAcknowledged = false;
        previewHeldDrink = BaristaDrinkType.None;

        NarrativeProgressState.SetCurrentMomentId(BaristaSecondMomentId);
        NarrativeProgressState.SetSceneOverride(BarInteriorSceneId);

        ApplyPreviewToRuntime();

        Debug.Log(
            "[ROAE][BaristaWelcomeDebugMenu] Second barista moment armed." +
            " moment=" + NarrativeProgressState.GetCurrentMomentId() +
            " scene=" + NarrativeProgressState.GetCurrentSceneId());
    }
}
