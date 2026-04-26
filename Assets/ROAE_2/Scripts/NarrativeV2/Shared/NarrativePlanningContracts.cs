using System;

public interface INarrativePlannerResult
{
    string PlannerId { get; }
    string PhaseId { get; }
    string ActionId { get; }
    string OutcomeId { get; }
    string DialogueId { get; }
    string Reason { get; }
    float NeutralScore { get; }
    float WarmScore { get; }
    float MischievousScore { get; }
}

public interface INarrativeMomentPlanner<TRuntimeState, TResult>
    where TResult : INarrativePlannerResult
{
    TResult ResolveCurrentResult();
    TResult ResolveResult(TRuntimeState runtimeState);
}

public static class NarrativePlannerDebugFormatter
{
    public static string Format(INarrativePlannerResult result)
    {
        if (result == null)
            return "planner_result=NULL";

        return "planner=" + Safe(result.PlannerId) +
               " phase=" + Safe(result.PhaseId) +
               " action=" + Safe(result.ActionId) +
               " outcome=" + Safe(result.OutcomeId) +
               " dialogue=" + Safe(result.DialogueId) +
               " reason=" + Safe(result.Reason) +
               " | scores neutral=" + result.NeutralScore.ToString("0.00") +
               " warm=" + result.WarmScore.ToString("0.00") +
               " mischievous=" + result.MischievousScore.ToString("0.00");
    }

    private static string Safe(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }
}
