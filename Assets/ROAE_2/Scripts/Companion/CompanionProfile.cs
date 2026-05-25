using UnityEngine;

[CreateAssetMenu(fileName = "CompanionProfile", menuName = "ROAE/Companion/Profile")]
public class CompanionProfile : ScriptableObject
{
    [Header("Identity")]
    public string companionId = "rina_snail";
    public string displayName = "The Snail";

    [Header("Summon")]
    public float baseSummonCooldownSeconds = 30f;
    public float reactiveSpeechCooldownSeconds = 8f;
    public float emotionRefreshIntervalSeconds = 0.5f;
    public bool blockSummonWhenEmpathyTooLow = true;
    [Range(0, 100)] public int lowEmpathySummonBlockThreshold = 15;

    [Header("Mood")]
    [Range(0, 100)] public int maliciousCorruptionOverrideThreshold = 75;
    [Range(0f, 1f)] public float emotionalUnpredictability = 0.02f;
    [Range(0f, 1f)] public float dialogueUnpredictability = 0.08f;
    public bool useSocialPlanner = true;

    [Header("Social Planner")]
    public CompanionPlannerMode plannerMode = CompanionPlannerMode.PolicyIteration;
    [Range(0.1f, 0.95f)] public float plannerDiscount = 0.72f;
    [Range(0f, 1f)] public float socialSignalBlend = 0.45f;
    [Range(0f, 40f)] public float socialSignalDecayPerMinute = 8f;
    [Range(0, 40)] public int emotionSwitchThreshold = 12;
    public bool socialPlannerUsesNpcSignalsOnly = false;

    public void EnsureStarterRules()
    {
        // Intentionally empty. The companion now uses a tiny fixed 4-state model.
    }
}
