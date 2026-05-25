using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class CompanionDialogueEntry
{
    public string entryId = Guid.NewGuid().ToString("N");
    [TextArea(2, 4)]
    public string line = string.Empty;
    public CompanionDialogueIntent intent = CompanionDialogueIntent.Ambient;
    public CompanionNarrativeReliability reliability = CompanionNarrativeReliability.Ambiguous;
    public int baseWeight = 10;
    public float empathyScale;
    public float creativityScale;
    public float corruptionScale;
    public float cooldownSeconds = 45f;
    public bool unlocksOnHear;
    public bool contradictionCandidate;
    public string unlockPoolId = string.Empty;
    public List<CompanionEmotionalState> supportedStates = new List<CompanionEmotionalState>();
    public CompanionConditionSet conditions = new CompanionConditionSet();

    public int EvaluateWeight(
        CompanionEvaluationContext context,
        CompanionSpeechRequest request,
        CompanionEmotionalState emotion,
        float secondsSinceLastUse)
    {
        if (context == null || request == null || !conditions.Matches(context))
            return int.MinValue;

        if (intent != request.intent)
            return int.MinValue;

        if (cooldownSeconds > 0f && secondsSinceLastUse < cooldownSeconds)
            return int.MinValue;

        CompanionEmotionalState normalizedEmotion = CompanionEmotionStateUtility.Normalize(emotion);
        if (supportedStates != null && supportedStates.Count > 0 && !SupportsState(normalizedEmotion))
            return int.MinValue;

        CompanionContextSnapshot snapshot = context.Snapshot;
        float score = baseWeight +
                      (snapshot.empathy - 50f) * empathyScale +
                      (snapshot.creativity - 50f) * creativityScale +
                      snapshot.corruption * corruptionScale;

        if (request.extraTags != null)
        {
            for (int i = 0; i < request.extraTags.Count; i++)
            {
                if (snapshot.HasTag(request.extraTags[i]))
                    score += 1f;
            }
        }

        return Mathf.RoundToInt(score);
    }

    private bool SupportsState(CompanionEmotionalState emotion)
    {
        if (supportedStates == null || supportedStates.Count == 0)
            return true;

        for (int i = 0; i < supportedStates.Count; i++)
        {
            if (CompanionEmotionStateUtility.Normalize(supportedStates[i]) == emotion)
                return true;
        }

        return false;
    }
}

[CreateAssetMenu(fileName = "CompanionDialogueLibrary", menuName = "ROAE/Companion/Dialogue Library")]
public class CompanionDialogueLibrary : ScriptableObject
{
    [SerializeField] private List<CompanionDialogueEntry> entries = new List<CompanionDialogueEntry>();

    public IReadOnlyList<CompanionDialogueEntry> Entries => entries;

    public void SetEntries(IList<CompanionDialogueEntry> source)
    {
        entries = source != null ? new List<CompanionDialogueEntry>(source) : new List<CompanionDialogueEntry>();
    }

    public void EnsureStarterEntries()
    {
        if (entries != null && entries.Count > 0)
            return;

        entries = new List<CompanionDialogueEntry>
        {
            new CompanionDialogueEntry
            {
                entryId = "companion_arrival_reflection",
                intent = CompanionDialogueIntent.Arrival,
                line = "The glass remembered us first.",
                baseWeight = 16,
                supportedStates = new List<CompanionEmotionalState> { CompanionEmotionalState.Healthy, CompanionEmotionalState.Sad },
                conditions = new CompanionConditionSet
                {
                    requireSafeSpace = true,
                    requiredTags = new List<CompanionEnvironmentTag> { CompanionEnvironmentTag.Reflection }
                }
            },
            new CompanionDialogueEntry
            {
                entryId = "companion_hint_sadness",
                intent = CompanionDialogueIntent.Hint,
                line = "This place sounds tired.",
                baseWeight = 20,
                empathyScale = 0.25f,
                supportedStates = new List<CompanionEmotionalState> { CompanionEmotionalState.Sad, CompanionEmotionalState.Healthy },
                conditions = new CompanionConditionSet
                {
                    requiredTags = new List<CompanionEnvironmentTag> { CompanionEnvironmentTag.Sadness }
                }
            },
            new CompanionDialogueEntry
            {
                entryId = "companion_hint_falsehood",
                intent = CompanionDialogueIntent.Hint,
                line = "The mirror doesn't want to look at us.",
                baseWeight = 24,
                supportedStates = new List<CompanionEmotionalState> { CompanionEmotionalState.Malicious, CompanionEmotionalState.Sad },
                conditions = new CompanionConditionSet
                {
                    requiredTags = new List<CompanionEnvironmentTag>
                    {
                        CompanionEnvironmentTag.Falsehood,
                        CompanionEnvironmentTag.Reflection
                    }
                }
            },
            new CompanionDialogueEntry
            {
                entryId = "companion_hint_bloom",
                intent = CompanionDialogueIntent.Hint,
                line = "The flowers are hiding something softer than they look.",
                baseWeight = 18,
                creativityScale = 0.25f,
                supportedStates = new List<CompanionEmotionalState> { CompanionEmotionalState.Healthy },
                conditions = new CompanionConditionSet
                {
                    requiredTags = new List<CompanionEnvironmentTag> { CompanionEnvironmentTag.Bloom }
                }
            },
            new CompanionDialogueEntry
            {
                entryId = "companion_comfort_lonely",
                intent = CompanionDialogueIntent.Comfort,
                line = "We don't have to fill every silence. Some silences are already holding us.",
                baseWeight = 20,
                empathyScale = 0.4f,
                supportedStates = new List<CompanionEmotionalState> { CompanionEmotionalState.Healthy, CompanionEmotionalState.Sad },
                conditions = new CompanionConditionSet
                {
                    requireSafeSpace = true,
                    requiredTags = new List<CompanionEnvironmentTag> { CompanionEnvironmentTag.Loneliness }
                }
            },
            new CompanionDialogueEntry
            {
                entryId = "companion_warning_danger",
                intent = CompanionDialogueIntent.Warning,
                line = "Something here wants us smaller.",
                baseWeight = 22,
                corruptionScale = 0.1f,
                supportedStates = new List<CompanionEmotionalState> { CompanionEmotionalState.Malicious, CompanionEmotionalState.Sad },
                conditions = new CompanionConditionSet
                {
                    requiredTags = new List<CompanionEnvironmentTag> { CompanionEnvironmentTag.Danger }
                }
            },
            new CompanionDialogueEntry
            {
                entryId = "companion_contradiction_self",
                intent = CompanionDialogueIntent.Contradiction,
                line = "You keep calling me here like I wasn't already inside the room.",
                baseWeight = 28,
                corruptionScale = 0.15f,
                supportedStates = new List<CompanionEmotionalState> { CompanionEmotionalState.Malicious },
                contradictionCandidate = true,
                reliability = CompanionNarrativeReliability.SelfAware,
                conditions = new CompanionConditionSet()
            },
            new CompanionDialogueEntry
            {
                entryId = "companion_departure_soft",
                intent = CompanionDialogueIntent.Departure,
                line = "I'll go quiet before I go away.",
                baseWeight = 14,
                supportedStates = new List<CompanionEmotionalState> { CompanionEmotionalState.Numb, CompanionEmotionalState.Sad }
            }
        };
    }
}
