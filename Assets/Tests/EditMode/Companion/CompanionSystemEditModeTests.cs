using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[Category("ROAE.Companion")]
public class CompanionSystemEditModeTests
{
    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteAll();
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
    [Description("Companionul trebuie sa intre pe Malicious cand coruptia si contradicția interna depasesc zona de siguranta emotionala.")]
    public void HighCorruptionContextResolvesToMalicious()
    {
        CompanionProfile profile = ScriptableObject.CreateInstance<CompanionProfile>();
        try
        {
            profile.EnsureStarterRules();
            CompanionEvaluationContext context = BuildContext(
                corruption: 94,
                creativity: 55,
                empathy: 50,
                safeSpace: true);

            CompanionEmotionResult result = CompanionEmotionResolver.Resolve(profile, context);
            Assert.AreEqual(CompanionEmotionalState.Malicious, result.State);
        }
        finally
        {
            Object.DestroyImmediate(profile);
        }
    }

    [Test]
    [Description("Creativitatea foarte mica trebuie sa impinga companionul in Numbness.")]
    public void LowCreativityPrefersNumbState()
    {
        CompanionProfile profile = ScriptableObject.CreateInstance<CompanionProfile>();
        try
        {
            CompanionEvaluationContext context = BuildContext(
                creativity: 10,
                empathy: 85,
                corruption: 10,
                safeSpace: true);

            CompanionEmotionResult result = CompanionEmotionResolver.Resolve(profile, context);
            Assert.AreEqual(CompanionEmotionalState.Numb, result.State);
        }
        finally
        {
            Object.DestroyImmediate(profile);
        }
    }

    [Test]
    [Description("Creativitatea si empatia mari, cu coruptie mica, trebuie sa dea Healthy.")]
    public void HighCreativityAndEmpathyWithLowCorruptionPrefersHealthyState()
    {
        CompanionProfile profile = ScriptableObject.CreateInstance<CompanionProfile>();
        try
        {
            CompanionEvaluationContext context = BuildContext(
                creativity: 85,
                empathy: 80,
                corruption: 10,
                safeSpace: true);

            CompanionEmotionResult result = CompanionEmotionResolver.Resolve(profile, context);
            Assert.AreEqual(CompanionEmotionalState.Healthy, result.State);
        }
        finally
        {
            Object.DestroyImmediate(profile);
        }
    }

    [Test]
    [Description("Creativitatea si empatia mici, fara coruptie mare, trebuie sa dea Sadness.")]
    public void LowCreativityAndEmpathyPrefersSadState()
    {
        CompanionProfile profile = ScriptableObject.CreateInstance<CompanionProfile>();
        try
        {
            CompanionEvaluationContext context = BuildContext(
                creativity: 20,
                empathy: 15,
                corruption: 20,
                safeSpace: false);

            CompanionEmotionResult result = CompanionEmotionResolver.Resolve(profile, context);
            Assert.AreEqual(CompanionEmotionalState.Sad, result.State);
        }
        finally
        {
            Object.DestroyImmediate(profile);
        }
    }

    [Test]
    [Description("Hint-urile companionului trebuie sa ramana oblice: intr-un context de tristete ar trebui selectata replica dedicata tristetii, nu una generica.")]
    public void SadnessHintSelectsSadnessLine()
    {
        CompanionDialogueLibrary library = ScriptableObject.CreateInstance<CompanionDialogueLibrary>();
        CompanionProfile profile = ScriptableObject.CreateInstance<CompanionProfile>();
        try
        {
            profile.EnsureStarterRules();
            library.EnsureStarterEntries();
            CompanionSaveState state = CompanionPersistence.CreateDefault();
            CompanionEvaluationContext context = BuildContext(
                empathy: 75,
                safeSpace: true,
                tags: new[] { CompanionEnvironmentTag.Sadness, CompanionEnvironmentTag.Shelter });

            CompanionDialogueResult result = CompanionDialogueResolver.Resolve(
                library,
                context,
                new CompanionSpeechRequest
                {
                    intent = CompanionDialogueIntent.Hint,
                    extraTags = new List<CompanionEnvironmentTag> { CompanionEnvironmentTag.Sadness }
                },
                CompanionEmotionalState.Healthy,
                state,
                profile);

            Assert.NotNull(result.Entry);
            Assert.AreEqual("companion_hint_sadness", result.Entry.entryId);
        }
        finally
        {
            Object.DestroyImmediate(library);
            Object.DestroyImmediate(profile);
        }
    }

    private static CompanionEvaluationContext BuildContext(
        int creativity = 50,
        int empathy = 50,
        int corruption = 0,
        bool safeSpace = false,
        CompanionEnvironmentTag[] tags = null)
    {
        CompanionContextSnapshot snapshot = new CompanionContextSnapshot
        {
            creativity = creativity,
            empathy = empathy,
            corruption = corruption,
            isSafeSpace = safeSpace,
            isCompanionVisible = true,
            threatLevel = CompanionThreatLevel.None,
            presenceState = CompanionPresenceState.Manifested,
            currentEmotion = CompanionEmotionalState.Healthy,
            pointType = CompanionSummonPointType.QuietSpace,
            tags = tags != null ? new List<CompanionEnvironmentTag>(tags) : new List<CompanionEnvironmentTag>()
        };

        return new CompanionEvaluationContext(snapshot, null, null, null);
    }
}
