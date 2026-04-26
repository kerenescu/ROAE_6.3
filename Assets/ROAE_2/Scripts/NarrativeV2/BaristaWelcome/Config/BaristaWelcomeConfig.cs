using UnityEngine;

[CreateAssetMenu(fileName = "BaristaWelcomeConfig", menuName = "ROAE/Barista Welcome Config")]
public class BaristaWelcomeConfig : ScriptableObject
{
    [Header("Planner")]
    public BaristaPlannerMode plannerMode = BaristaPlannerMode.ValueIteration;
    [Range(0f, 0.99f)] public float gamma = 0.85f;
    public float epsilon = 0.0001f;
    [Min(1)] public int maxValueIterations = 96;
    [Min(1)] public int maxPolicyIterations = 24;
    [Min(1)] public int maxPolicyEvaluationSweeps = 96;

    [Header("Legacy tuning (unused in runtime solver)")]
    [HideInInspector] public int maxIterations = 50;
    [HideInInspector]
    public int corruptionThresholdForMischief = 2;
    [HideInInspector]
    public float rewardNeutralBase = 2f;
    [HideInInspector]
    public float rewardNeutralIfLowCorruption = 2f;
    [HideInInspector]
    public float rewardMischievousBase = 1f;
    [HideInInspector]
    public float rewardMischievousIfReadUnknownText = 4f;
    [HideInInspector]
    public float rewardMischievousIfCorruptionHigh = 3f;
    [HideInInspector] [Range(0.5f, 0.95f)] public float selfLoopProbability = 0.70f;

    public BaristaPlannerSettings ToPlannerSettings()
    {
        int fallbackIterations = Mathf.Max(1, maxIterations);
        int resolvedValueIterations = Mathf.Max(1, maxValueIterations > 0 ? maxValueIterations : fallbackIterations);
        int resolvedPolicyIterations = Mathf.Max(1, maxPolicyIterations > 0 ? maxPolicyIterations : fallbackIterations);
        int resolvedPolicyEvaluationSweeps = Mathf.Max(1, maxPolicyEvaluationSweeps > 0 ? maxPolicyEvaluationSweeps : fallbackIterations);

        return new BaristaPlannerSettings(
            Mathf.Clamp(gamma, 0f, 0.99f),
            Mathf.Max(0.00001f, epsilon),
            resolvedValueIterations,
            resolvedPolicyIterations,
            resolvedPolicyEvaluationSweeps);
    }
}
