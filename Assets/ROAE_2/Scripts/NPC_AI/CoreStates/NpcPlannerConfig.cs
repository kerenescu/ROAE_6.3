using UnityEngine;

[CreateAssetMenu(fileName = "NewNpcPlannerConfig", menuName = "Dialogue System/NPC Planner Config")]
public class NpcPlannerConfig : ScriptableObject
{
    [Header("Model")]
    [SerializeField] private NpcRewardProfile rewardProfile;
    [SerializeField] private NpcTransitionProfile transitionProfile;

    [Header("Solver")]
    [SerializeField] private NpcPlannerMode plannerMode = NpcPlannerMode.ValueIteration;
    [Range(0f, 0.99f)] [SerializeField] private float gamma = 0.85f;
    [SerializeField] private float epsilon = 0.0001f;
    [Min(1)] [SerializeField] private int maxValueIterations = 96;
    [Min(1)] [SerializeField] private int maxPolicyIterations = 24;
    [Min(1)] [SerializeField] private int maxPolicyEvaluationSweeps = 96;
    [SerializeField] private NpcActionType fallbackAction = NpcActionType.Neutral;

    public NpcRewardProfile RewardProfile => rewardProfile;
    public NpcTransitionProfile TransitionProfile => transitionProfile;
    public NpcPlannerMode PlannerMode => plannerMode;
    public NpcActionType FallbackAction => fallbackAction;

    public void SetPlannerMode(NpcPlannerMode mode)
    {
        if (plannerMode == mode)
            return;

        plannerMode = mode;
        NpcPolicySolver.Invalidate(this);
    }

    public NpcPlannerSettings ToSettings()
    {
        return new NpcPlannerSettings(
            plannerMode,
            gamma,
            epsilon,
            maxValueIterations,
            maxPolicyIterations,
            maxPolicyEvaluationSweeps);
    }

    private void OnValidate()
    {
        NpcPolicySolver.Invalidate(this);
    }
}

public readonly struct NpcPlannerSettings
{
    public readonly NpcPlannerMode plannerMode;
    public readonly float gamma;
    public readonly float epsilon;
    public readonly int maxValueIterations;
    public readonly int maxPolicyIterations;
    public readonly int maxPolicyEvaluationSweeps;

    public NpcPlannerSettings(
        NpcPlannerMode plannerMode,
        float gamma,
        float epsilon,
        int maxValueIterations,
        int maxPolicyIterations,
        int maxPolicyEvaluationSweeps)
    {
        this.plannerMode = plannerMode;
        this.gamma = Mathf.Clamp(gamma, 0f, 0.99f);
        this.epsilon = Mathf.Max(0.00001f, epsilon);
        this.maxValueIterations = Mathf.Max(1, maxValueIterations);
        this.maxPolicyIterations = Mathf.Max(1, maxPolicyIterations);
        this.maxPolicyEvaluationSweeps = Mathf.Max(1, maxPolicyEvaluationSweeps);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = (int) plannerMode;
            hash = (hash * 397) ^ gamma.GetHashCode();
            hash = (hash * 397) ^ epsilon.GetHashCode();
            hash = (hash * 397) ^ maxValueIterations;
            hash = (hash * 397) ^ maxPolicyIterations;
            hash = (hash * 397) ^ maxPolicyEvaluationSweeps;
            return hash;
        }
    }
}
