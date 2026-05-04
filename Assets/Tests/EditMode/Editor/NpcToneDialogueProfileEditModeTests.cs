using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[Category("ROAE.AI")]
public class NpcToneDialogueProfileEditModeTests
{
    private const string BaristaWelcomeProfilePath =
        "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Profiles/barista_welcome_ToneDialogueProfile.asset";
    private const string BaristaSecretProfilePath =
        "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Profiles/barista_secret_circuit_ToneDialogueProfile.asset";
    private const string MadameProfilePath =
        "Assets/ROAE_2/Data/NarrativeV2/MadameLichenia/Profiles/MadameLichenia_ToneDialogueProfile.asset";
    private const string AnticarProfilePath =
        "Assets/ROAE_2/Data/NarrativeV2/Anticariat/Profiles/Anticariat_ToneDialogueProfile.asset";

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteAll();
        BaristaWelcomeState.ResetAll();
        NarrativeProgressState.SetCurrentChapterId(string.Empty);
        NarrativeProgressState.SetCurrentMomentId(string.Empty);
        NarrativeProgressState.ClearSceneOverride();
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteAll();
        BaristaWelcomeState.ResetAll();
        NarrativeProgressState.ClearSceneOverride();
    }

    [Test]
    [Description("Test 1: profilul principal al Baristei trebuie sa porneasca pe PolicyIteration, pentru ca greeting-ul de baza foloseste plannerul warm/neutral nou.")]
    public void Test1_BaristaWelcomeProfileStartsOnPolicyIteration()
    {
        NpcToneDialogueProfile profile = LoadProfile(BaristaWelcomeProfilePath);

        Assert.AreEqual(BaristaPlannerMode.PolicyIteration, profile.plannerMode);
    }

    [Test]
    [Description("Test 2: profilul Anticarului trebuie sa ramana pe ValueIteration, astfel incat fluxul lui separat sa nu fie uniformizat accidental cu Barista sau Madame.")]
    public void Test2_AnticarProfileStartsOnValueIteration()
    {
        NpcToneDialogueProfile profile = LoadProfile(AnticarProfilePath);

        Assert.AreEqual(BaristaPlannerMode.ValueIteration, profile.plannerMode);
    }

    [Test]
    [Description("Test 3: atat ValueIteration, cat si PolicyIteration trebuie sa poata evalua aceeasi stare runtime si sa produca un rezultat valid cu actiune, ton si scoruri.")]
    public void Test3_BothPlannerModesProduceValidEvaluationForSameRuntimeState()
    {
        NpcTonePlanningRuntimeState state = new NpcTonePlanningRuntimeState
        {
            readUnknownText = true,
            creativity = 51,
            corruption = 1,
            empathy = 1,
            relationship = 4,
            introDone = true,
            pendingDrink = BaristaDrinkType.None,
            pendingDrinkAcknowledged = false,
            heldDrink = BaristaDrinkType.None
        };

        NpcTonePlannerEvaluation valueIteration = NpcTonePlanningSolvers.Evaluate(
            state,
            BaristaPlannerMode.ValueIteration,
            NpcTonePlannerSettings.Default,
            false,
            false);

        NpcTonePlannerEvaluation policyIteration = NpcTonePlanningSolvers.Evaluate(
            state,
            BaristaPlannerMode.PolicyIteration,
            NpcTonePlannerSettings.Default,
            false,
            false);

        Assert.IsTrue(System.Enum.IsDefined(typeof(BaristaNarrativeAction), valueIteration.bestAction));
        Assert.IsTrue(System.Enum.IsDefined(typeof(BaristaNarrativeAction), policyIteration.bestAction));
        Assert.IsTrue(System.Enum.IsDefined(typeof(BaristaIntroTone), valueIteration.mappedTone));
        Assert.IsTrue(System.Enum.IsDefined(typeof(BaristaIntroTone), policyIteration.mappedTone));
        Assert.IsNotEmpty(valueIteration.actionScoreSummary);
        Assert.IsNotEmpty(policyIteration.actionScoreSummary);
        StringAssert.Contains("action=", valueIteration.BuildDebugString());
        StringAssert.Contains("tone=", valueIteration.BuildDebugString());
        StringAssert.Contains("action=", policyIteration.BuildDebugString());
        StringAssert.Contains("tone=", policyIteration.BuildDebugString());
    }

    [Test]
    [Description("Test 4: momentul secret al Baristei trebuie sa castige doar dupa ce Anticar impartaseste secretul si sa cada inapoi pe profilul normal dupa closure.")]
    public void Test4_BaristaSecretMomentWinsOnlyDuringOpenSecretCircuit()
    {
        NpcToneDialogueProfile welcomeProfile = LoadProfile(BaristaWelcomeProfilePath);
        NpcToneDialogueProfile secretProfile = LoadProfile(BaristaSecretProfilePath);

        NarrativeProgressState.SetSceneOverride("Bar_Interior");

        NpcToneDialogueProfile activeBeforeUnlock = NarrativeMomentSelection.ResolveHighestPriorityActiveMoment(
            new[] { welcomeProfile, secretProfile },
            ReadBoolFlag);

        Assert.AreEqual(welcomeProfile, activeBeforeUnlock);

        PlayerPrefs.SetInt(NarrativeFlagKeys.AnticarSharedBaristaSecret, 1);

        NpcToneDialogueProfile activeDuringSecret = NarrativeMomentSelection.ResolveHighestPriorityActiveMoment(
            new[] { welcomeProfile, secretProfile },
            ReadBoolFlag);

        Assert.AreEqual(secretProfile, activeDuringSecret);

        PlayerPrefs.SetInt(NarrativeFlagKeys.BaristaSecretClosureDone, 1);

        NpcToneDialogueProfile activeAfterClosure = NarrativeMomentSelection.ResolveHighestPriorityActiveMoment(
            new[] { welcomeProfile, secretProfile },
            ReadBoolFlag);

        Assert.AreEqual(welcomeProfile, activeAfterClosure);
    }

    [Test]
    [Description("Test 5: Barista trebuie sa porneasca pe faza Intro cat timp introDone este fals si nu exista bautura pendinte sau tinuta in mana.")]
    public void Test5_BaristaWelcomeProfileStartsInIntroPhase()
    {
        NpcToneDialogueProfile profile = LoadProfile(BaristaWelcomeProfilePath);

        AssertPhase(profile, "Intro");
    }

    [Test]
    [Description("Test 6: Barista trebuie sa treaca pe faza Order imediat dupa ce introDone devine adevarat si nu mai exista pending drink.")]
    public void Test6_BaristaWelcomeProfileMovesToOrderPhaseAfterIntro()
    {
        NpcToneDialogueProfile profile = LoadProfile(BaristaWelcomeProfilePath);
        BaristaWelcomeState.SetFlag(BaristaWelcomeKeys.BaristaIntroDone, true);

        AssertPhase(profile, "Order");
    }

    [Test]
    [Description("Test 7: Barista trebuie sa intre pe faza Preparing cand Photosynthetic Sap este pendinte, dar inca nu a fost acknowledge-uit.")
    ]
    public void Test7_BaristaWelcomeProfileUsesPreparingPhaseForUnacknowledgedPendingDrink()
    {
        NpcToneDialogueProfile profile = LoadProfile(BaristaWelcomeProfilePath);
        BaristaWelcomeState.SetFlag(BaristaWelcomeKeys.BaristaIntroDone, true);
        BaristaWelcomeState.SetPendingDrink(BaristaDrinkType.PhotosyntheticSap);

        AssertPhase(profile, "Preparing");
    }

    [Test]
    [Description("Test 8: Barista trebuie sa intre pe faza PreparingReminder dupa ce pending drink-ul a fost acknowledge-uit, dar nu a fost livrat inca.")]
    public void Test8_BaristaWelcomeProfileUsesPreparingReminderAfterAcknowledgingPendingDrink()
    {
        NpcToneDialogueProfile profile = LoadProfile(BaristaWelcomeProfilePath);
        BaristaWelcomeState.SetFlag(BaristaWelcomeKeys.BaristaIntroDone, true);
        BaristaWelcomeState.SetPendingDrink(BaristaDrinkType.PhotosyntheticSap);
        BaristaWelcomeState.AcknowledgePendingDrink();

        AssertPhase(profile, "PreparingReminder");
    }

    [Test]
    [Description("Test 9: Madame trebuie sa ramana pe faza Intro inainte ca tarotul sa fie deblocat sau citit.")]
    public void Test9_MadameStartsInIntroBeforeTarotPathBegins()
    {
        NpcToneDialogueProfile profile = LoadProfile(MadameProfilePath);

        AssertPhase(profile, "Intro");
    }

    [Test]
    [Description("Test 10: Madame trebuie sa intre pe faza AfterReadingReferral dupa citirea tarotului, atata timp cat recomandarea spre Anticar nu a fost data inca.")]
    public void Test10_MadameUsesAfterReadingReferralAfterTarotCompletion()
    {
        NpcToneDialogueProfile profile = LoadProfile(MadameProfilePath);
        PlayerPrefs.SetInt(NarrativeFlagKeys.TarotReadingCompleted, 1);

        AssertPhase(profile, "AfterReadingReferral");
    }

    [Test]
    [Description("Test 11: Madame trebuie sa intre pe faza ReferralPending dupa ce jucatorul a primit recomandarea spre Anticar, dar inainte ca bucla secreta sa fie inchisa.")]
    public void Test11_MadameUsesReferralPendingAfterReferralChoice()
    {
        NpcToneDialogueProfile profile = LoadProfile(MadameProfilePath);
        PlayerPrefs.SetInt(NarrativeFlagKeys.MadameSentToAnticar, 1);

        AssertPhase(profile, "ReferralPending");
    }

    [Test]
    [Description("Test 12: Madame trebuie sa intre pe faza CircuitComplete dupa ce Barista inchide circuitul secret pornit din tarot si anticariat.")]
    public void Test12_MadameUsesCircuitCompleteAfterBaristaClosure()
    {
        NpcToneDialogueProfile profile = LoadProfile(MadameProfilePath);
        PlayerPrefs.SetInt(NarrativeFlagKeys.BaristaSecretClosureDone, 1);

        AssertPhase(profile, "CircuitComplete");
    }

    [Test]
    [Description("Test 13: Anticar trebuie sa intre pe faza TarotReferral imediat dupa ce Madame trimite jucatorul spre anticariat, dar inainte sa-i impartaseasca secretul.")]
    public void Test13_AnticarUsesTarotReferralAfterMadameReferral()
    {
        NpcToneDialogueProfile profile = LoadProfile(AnticarProfilePath);
        PlayerPrefs.SetInt(NarrativeFlagKeys.MadameSentToAnticar, 1);

        AssertPhase(profile, "TarotReferral");
    }

    [Test]
    [Description("Test 14: Anticar trebuie sa ramana pe faza ReferralPending dupa ce a impartasit secretul Baristei, pana cand jucatorul inchide bucla inapoi la bar.")]
    public void Test14_AnticarUsesReferralPendingAfterSharingSecret()
    {
        NpcToneDialogueProfile profile = LoadProfile(AnticarProfilePath);
        PlayerPrefs.SetInt(NarrativeFlagKeys.AnticarSharedBaristaSecret, 1);

        AssertPhase(profile, "ReferralPending");
    }

    [Test]
    [Description("Test 15: Anticar trebuie sa intre pe faza CircuitComplete dupa ce Barista a consumat closure-ul narativ si bucla secreta este complet inchisa.")]
    public void Test15_AnticarUsesCircuitCompleteAfterBaristaClosure()
    {
        NpcToneDialogueProfile profile = LoadProfile(AnticarProfilePath);
        PlayerPrefs.SetInt(NarrativeFlagKeys.BaristaSecretClosureDone, 1);

        AssertPhase(profile, "CircuitComplete");
    }

    [Test]
    [Description("Test 16: toate fazele din profilele active trebuie sa aiba neutral, warm si mischievous, astfel incat plannerul sa poata loga si selecta tonul explicit peste tot.")]
    public void Test16_AllActiveProfilesExposeCompleteToneCoverage()
    {
        string[] paths =
        {
            BaristaWelcomeProfilePath,
            BaristaSecretProfilePath,
            MadameProfilePath,
            AnticarProfilePath,
            "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Profiles/barista_second_visit_ToneDialogueProfile.asset",
            "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Profiles/barista_tarot_followup_ToneDialogueProfile.asset"
        };

        for (int i = 0; i < paths.Length; i++)
        {
            NpcToneDialogueProfile profile = LoadProfile(paths[i]);
            Assert.NotNull(profile.phaseDefinitions, "Profile without phases: " + profile.name);

            for (int phaseIndex = 0; phaseIndex < profile.phaseDefinitions.Count; phaseIndex++)
            {
                NpcToneDialoguePhaseDefinition phase = profile.phaseDefinitions[phaseIndex];
                Assert.NotNull(phase, "Null phase in profile: " + profile.name);
                Assert.IsTrue(
                    phase.HasCompleteToneVariants(),
                    "Phase without full tone coverage: profile=" + profile.name + " phase=" + phase.PhaseIdOrDefault);
            }
        }
    }

    [Test]
    [Description("Test 17: daca o faza are doar o singura replica fixa, resolverul trebuie sa raporteze clar cazul special only_one in loc sa para ca exista variante tonale reale.")]
    public void Test17_FixedOnlyPhaseReportsOnlyOneRouting()
    {
        DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();
        NpcToneDialoguePhaseDefinition phase = new NpcToneDialoguePhaseDefinition
        {
            fixedDialogue = dialogue
        };

        try
        {
            NpcToneDialoguePhaseDefinition.DialogueResolution resolution =
                phase.ResolveDialogueWithRouting(BaristaIntroTone.Warm);

            Assert.AreEqual(dialogue, resolution.dialogue);
            Assert.AreEqual(NpcToneDialogueRoutingKind.FixedOnlySingleDialogue, resolution.routingKind);
            Assert.AreEqual("only_one", resolution.sourceToneLabel);
        }
        finally
        {
            ScriptableObject.DestroyImmediate(dialogue);
        }
    }

    [Test]
    [Description("Test 18: daca lipseste tonul exact, resolverul trebuie sa cada pe o varianta existenta si sa marcheze explicit fallback-ul, pentru debugging scalabil.")]
    public void Test18_MissingExactToneReportsFallbackRouting()
    {
        DialogueData neutral = ScriptableObject.CreateInstance<DialogueData>();
        NpcToneDialoguePhaseDefinition phase = new NpcToneDialoguePhaseDefinition
        {
            neutralDialogue = neutral
        };

        try
        {
            NpcToneDialoguePhaseDefinition.DialogueResolution resolution =
                phase.ResolveDialogueWithRouting(BaristaIntroTone.Mischievous);

            Assert.AreEqual(neutral, resolution.dialogue);
            Assert.AreEqual(NpcToneDialogueRoutingKind.FallbackToneVariant, resolution.routingKind);
            Assert.AreEqual("Neutral", resolution.sourceToneLabel);
        }
        finally
        {
            ScriptableObject.DestroyImmediate(neutral);
        }
    }

    private static NpcToneDialogueProfile LoadProfile(string assetPath)
    {
        NpcToneDialogueProfile profile = AssetDatabase.LoadAssetAtPath<NpcToneDialogueProfile>(assetPath);
        Assert.NotNull(profile, "Missing profile asset at path: " + assetPath);
        return profile;
    }

    private static void AssertPhase(NpcToneDialogueProfile profile, string expectedPhaseId)
    {
        Assert.IsTrue(profile.TryResolvePhase(out NpcToneDialoguePhaseDefinition phase));
        Assert.NotNull(phase);
        Assert.AreEqual(expectedPhaseId, phase.phaseId);
    }

    private static bool ReadBoolFlag(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && PlayerPrefs.GetInt(key, 0) == 1;
    }
}
