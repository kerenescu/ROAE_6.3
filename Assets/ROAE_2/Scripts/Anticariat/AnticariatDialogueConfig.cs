using UnityEngine;

[CreateAssetMenu(fileName = "AnticariatDialogueConfig", menuName = "ROAE/Anticariat Dialogue Config")]
public class AnticariatDialogueConfig : ScriptableObject
{
    [Header("Planner")]
    public BaristaPlannerMode plannerMode = BaristaPlannerMode.ValueIteration;
    [Range(0f, 0.99f)] public float gamma = 0.85f;
    public float epsilon = 0.0001f;
    [Min(1)] public int maxValueIterations = 96;
    [Min(1)] public int maxPolicyIterations = 24;
    [Min(1)] public int maxPolicyEvaluationSweeps = 96;

    public BaristaPlannerSettings ToPlannerSettings()
    {
        return new BaristaPlannerSettings(
            Mathf.Clamp(gamma, 0f, 0.99f),
            Mathf.Max(0.00001f, epsilon),
            Mathf.Max(1, maxValueIterations),
            Mathf.Max(1, maxPolicyIterations),
            Mathf.Max(1, maxPolicyEvaluationSweeps));
    }
}
