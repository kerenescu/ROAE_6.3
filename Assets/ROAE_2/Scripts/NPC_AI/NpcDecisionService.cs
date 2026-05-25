using System.Collections.Generic;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public readonly struct NpcDecisionResult
{
    public readonly NpcDefinition definition;
    public readonly NpcDecisionContext context;
    public readonly NpcActionType action;
    public readonly DialogueData dialogue;
    public readonly string reason;

    public NpcDecisionResult(
        NpcDefinition definition,
        NpcDecisionContext context,
        NpcActionType action,
        DialogueData dialogue,
        string reason)
    {
        this.definition = definition;
        this.context = context;
        this.action = action;
        this.dialogue = dialogue;
        this.reason = reason ?? string.Empty;
    }

    public bool HasDialogue => dialogue != null;

    public string ToDebugString()
    {
        return "npc=" + (definition != null ? definition.NpcId : "NULL") +
               " action=" + action +
               " dialogue=" + (dialogue != null ? dialogue.name : "NULL") +
               " reason=" + reason +
               " state={" + context.ToDebugString() + "}";
    }
}

public static class NpcDecisionService
{
    public static NpcDecisionResult Decide(NpcDefinition definition, bool auditLogs = false)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        if (definition == null)
        {
            NpcDecisionContext emptyContext = NpcDecisionContext.Build(string.Empty);
            NpcDecisionResult missingDefinitionResult = new NpcDecisionResult(
                null,
                emptyContext,
                NpcActionType.Neutral,
                null,
                "missing_definition");

            stopwatch.Stop();
            if (auditLogs)
                LogDecisionResult(missingDefinitionResult, false, stopwatch.Elapsed.TotalMilliseconds);

            return missingDefinitionResult;
        }

        NpcDecisionContext context = NpcDecisionContext.Build(definition);
        if (auditLogs)
            LogDecisionInput(definition, context);

        NpcActionType action = SelectAction(definition, context, auditLogs);
        DialogueData dialogue = ResolveDialogue(definition, context, action);

        NpcDecisionResult result = new NpcDecisionResult(
            definition,
            context,
            action,
            dialogue,
            BuildReason(definition, context, action, dialogue));

        stopwatch.Stop();
        if (auditLogs)
            LogDecisionResult(result, result.HasDialogue, stopwatch.Elapsed.TotalMilliseconds);

        return result;
    }

    public static NpcActionType SelectAction(
        NpcDefinition definition,
        NpcDecisionContext context,
        bool auditLogs = false)
    {
        if (definition == null)
            return NpcActionType.Neutral;

        if (definition.PlannerConfig != null)
        {
            NpcActionType planned = NpcPolicySolver.GetBestAction(
                definition.PlannerConfig,
                context.ToDecisionState(),
                false,
                auditLogs);

            if (definition.HasAction(planned))
                return planned;
        }

        return SelectPersonalityAction(definition, context);
    }

    public static DialogueData ResolveDialogue(
        NpcDefinition definition,
        NpcDecisionContext context,
        NpcActionType action)
    {
        if (definition == null || definition.ResponseSet == null)
            return null;

        DialogueData selected = definition.ResponseSet.ResolveDialogue(context, action);
        if (selected != null)
            return selected;

        return ResolveFirstAvailableDialogue(definition, context);
    }

    private static NpcActionType SelectPersonalityAction(NpcDefinition definition, NpcDecisionContext context)
    {
        IReadOnlyList<NpcActionType> actions = definition.AvailableActions;
        if (actions == null || actions.Count == 0)
            return NpcActionType.Neutral;

        NpcActionType bestAction = actions[0];
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < actions.Count; i++)
        {
            NpcActionType action = actions[i];
            float score = ScoreAction(definition, context, action);
            if (score > bestScore)
            {
                bestAction = action;
                bestScore = score;
            }
        }

        return bestAction;
    }

    private static float ScoreAction(
        NpcDefinition definition,
        NpcDecisionContext context,
        NpcActionType action)
    {
        return BaseScore(action);
    }

    private static float BaseScore(NpcActionType action)
    {
        switch (action)
        {
            case NpcActionType.Neutral:
                return 0.1f;
            default:
                return 0f;
        }
    }

    private static DialogueData ResolveFirstAvailableDialogue(
        NpcDefinition definition,
        NpcDecisionContext context)
    {
        IReadOnlyList<NpcActionType> actions = definition.AvailableActions;
        if (actions == null)
            return null;

        for (int i = 0; i < actions.Count; i++)
        {
            DialogueData fallback = definition.ResponseSet.ResolveDialogue(context, actions[i]);
            if (fallback != null)
                return fallback;
        }

        return null;
    }

    private static string BuildReason(
        NpcDefinition definition,
        NpcDecisionContext context,
        NpcActionType action,
        DialogueData dialogue)
    {
        string model = definition.PlannerConfig != null ? "planner" : "personality";
        return "model=" + model +
               " action=" + action +
               " dialogue=" + (dialogue != null ? dialogue.name : "NULL") +
               " context=" + context.ToDebugString();
    }

    private static void LogDecisionResult(
        NpcDecisionResult result,
        bool success,
        double durationMs)
    {
        string level = success ? "[SUCCESS]" : "[FAIL]";
        string message =
            "[ROAE][AI][NpcDecision]" + level +
            " npc=" + (result.definition != null ? result.definition.NpcId : "NULL") +
            " action=" + result.action +
            " dialogue=" + (result.dialogue != null ? result.dialogue.name : "NULL") +
            " reason=" + result.reason +
            " state={" + result.context.ToDebugString() + "}" +
            " durationMs=" + durationMs.ToString("0.00");

        if (success)
            Debug.Log(message);
        else
            Debug.LogWarning(message);
    }

    private static void LogDecisionInput(
        NpcDefinition definition,
        NpcDecisionContext context)
    {
        string planner = definition != null && definition.PlannerConfig != null
            ? definition.PlannerConfig.name
            : "None";
        string model = definition != null && definition.PlannerConfig != null
            ? "planner"
            : "personality";

        Debug.Log(
            "[ROAE][AI][NpcDecision][INPUT] npc=" + (definition != null ? definition.NpcId : "NULL") +
            " model=" + model +
            " planner=" + planner +
            " state={" + context.ToDebugString() + "}");
    }
}
