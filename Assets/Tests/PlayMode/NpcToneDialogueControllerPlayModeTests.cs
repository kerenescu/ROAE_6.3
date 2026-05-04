using NUnit.Framework;
using UnityEngine;

[Category("ROAE.AI")]
public class NpcToneDialogueControllerPlayModeTests
{
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
        NarrativeProgressState.ClearSceneOverride();
    }

    [Test]
    [Description("Test 1: controllerul generic trebuie sa comute pe momentul secret al Baristei cand vine flag-ul din anticariat.")]
    public void Test1_ActiveProfileSwitchesToSecretMomentWhenFlagChanges()
    {
        GameObject host = new GameObject("ToneControllerHost");
        NpcToneDialogueController controller = host.AddComponent<NpcToneDialogueController>();
        NpcToneDialogueProfile welcomeProfile = ScriptableObject.CreateInstance<NpcToneDialogueProfile>();
        NpcToneDialogueProfile secretProfile = ScriptableObject.CreateInstance<NpcToneDialogueProfile>();

        try
        {
            welcomeProfile.momentId = "barista_welcome";
            welcomeProfile.priority = 100;
            welcomeProfile.requiredSceneId = "Bar_Interior";

            secretProfile.momentId = "barista_secret_circuit";
            secretProfile.priority = 550;
            secretProfile.requiredSceneId = "Bar_Interior";
            secretProfile.requiredTrueFlags.Add(NarrativeFlagKeys.AnticarSharedBaristaSecret);
            secretProfile.requiredFalseFlags.Add(NarrativeFlagKeys.BaristaSecretClosureDone);

            controller.AssignProfile(welcomeProfile);
            controller.AssignMomentProfiles(new[] { welcomeProfile, secretProfile });

            NarrativeProgressState.SetSceneOverride("Bar_Interior");

            Assert.AreEqual(welcomeProfile, controller.ActiveProfile);

            PlayerPrefs.SetInt(NarrativeFlagKeys.AnticarSharedBaristaSecret, 1);

            Assert.AreEqual(secretProfile, controller.ActiveProfile);

            PlayerPrefs.SetInt(NarrativeFlagKeys.BaristaSecretClosureDone, 1);

            Assert.AreEqual(welcomeProfile, controller.ActiveProfile);
        }
        finally
        {
            Object.DestroyImmediate(welcomeProfile);
            Object.DestroyImmediate(secretProfile);
            Object.Destroy(host);
        }
    }

    [Test]
    [Description("Test 2: controllerul generic trebuie sa lase profilul de baza activ cand faza secreta a Anticarului nu e inca deblocata.")]
    public void Test2_ControllerKeepsFallbackProfileBeforeReferralFlag()
    {
        GameObject host = new GameObject("FallbackProfileHost");
        NpcToneDialogueController controller = host.AddComponent<NpcToneDialogueController>();
        NpcToneDialogueProfile fallbackProfile = ScriptableObject.CreateInstance<NpcToneDialogueProfile>();
        NpcToneDialogueProfile referralProfile = ScriptableObject.CreateInstance<NpcToneDialogueProfile>();

        try
        {
            fallbackProfile.momentId = "anticar_return";
            fallbackProfile.priority = 10;
            fallbackProfile.requiredSceneId = "Anticariat";

            referralProfile.momentId = "anticar_secret_referral";
            referralProfile.priority = 200;
            referralProfile.requiredSceneId = "Anticariat";
            referralProfile.requiredTrueFlags.Add(NarrativeFlagKeys.MadameSentToAnticar);
            referralProfile.requiredFalseFlags.Add(NarrativeFlagKeys.BaristaSecretClosureDone);

            controller.AssignProfile(fallbackProfile);
            controller.AssignMomentProfiles(new[] { fallbackProfile, referralProfile });
            NarrativeProgressState.SetSceneOverride("Anticariat");

            Assert.AreEqual(fallbackProfile, controller.ActiveProfile);

            PlayerPrefs.SetInt(NarrativeFlagKeys.MadameSentToAnticar, 1);
            Assert.AreEqual(referralProfile, controller.ActiveProfile);
        }
        finally
        {
            Object.DestroyImmediate(fallbackProfile);
            Object.DestroyImmediate(referralProfile);
            Object.Destroy(host);
        }
    }

    [Test]
    [Description("Test 3: schimbarea modului de planner trebuie sa actualizeze profilul de baza si toate moment profiles, inclusiv pe cel secret.")]
    public void Test3_SetPlannerModeUpdatesFallbackAndSecretProfiles()
    {
        GameObject host = new GameObject("PlannerModeHost");
        NpcToneDialogueController controller = host.AddComponent<NpcToneDialogueController>();
        NpcToneDialogueProfile fallbackProfile = ScriptableObject.CreateInstance<NpcToneDialogueProfile>();
        NpcToneDialogueProfile secretProfile = ScriptableObject.CreateInstance<NpcToneDialogueProfile>();

        try
        {
            fallbackProfile.plannerMode = BaristaPlannerMode.ValueIteration;
            secretProfile.plannerMode = BaristaPlannerMode.ValueIteration;

            controller.AssignProfile(fallbackProfile);
            controller.AssignMomentProfiles(new[] { secretProfile });
            controller.SetPlannerMode(BaristaPlannerMode.PolicyIteration);

            Assert.AreEqual(BaristaPlannerMode.PolicyIteration, fallbackProfile.plannerMode);
            Assert.AreEqual(BaristaPlannerMode.PolicyIteration, secretProfile.plannerMode);
        }
        finally
        {
            Object.DestroyImmediate(fallbackProfile);
            Object.DestroyImmediate(secretProfile);
            Object.Destroy(host);
        }
    }
}
