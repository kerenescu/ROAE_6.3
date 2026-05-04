using System.Collections.Generic;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public sealed class NpcPolicySolution
{
    public readonly Dictionary<NpcDecisionState, NpcActionType> policy;
    public readonly Dictionary<NpcDecisionState, float> values;
    public readonly NpcPlannerMode plannerMode;
    public readonly NpcPlannerSettings settings;
    public readonly NpcActionType fallbackAction;

    public NpcPolicySolution(
        Dictionary<NpcDecisionState, NpcActionType> policy,
        Dictionary<NpcDecisionState, float> values,
        NpcPlannerMode plannerMode,
        NpcPlannerSettings settings,
        NpcActionType fallbackAction)
    {
        this.policy = policy ?? new Dictionary<NpcDecisionState, NpcActionType>();
        this.values = values ?? new Dictionary<NpcDecisionState, float>();
        this.plannerMode = plannerMode;
        this.settings = settings;
        this.fallbackAction = fallbackAction;
    }
}

internal struct NpcSolverStats
{
    public bool success;
    public string status;
    public int stateCount;
    public int actionCount;
    public int iterations;
    public int evaluationSweeps;
    public float finalDelta;
    public bool converged;

    public static NpcSolverStats Create(
        bool success,
        string status,
        int stateCount,
        int actionCount,
        int iterations,
        int evaluationSweeps,
        float finalDelta,
        bool converged)
    {
        return new NpcSolverStats
        {
            success = success,
            status = status ?? string.Empty,
            stateCount = stateCount,
            actionCount = actionCount,
            iterations = iterations,
            evaluationSweeps = evaluationSweeps,
            finalDelta = finalDelta,
            converged = converged
        };
    }
}

public static class NpcPolicySolver
{
    private struct CacheKey : System.IEquatable<CacheKey>
    {
        public int configId;
        public int rewardProfileId;
        public int transitionProfileId;
        public int settingsHash;
        public int fallbackAction;

        public bool Equals(CacheKey other)
        {
            return configId == other.configId &&
                   rewardProfileId == other.rewardProfileId &&
                   transitionProfileId == other.transitionProfileId &&
                   settingsHash == other.settingsHash &&
                   fallbackAction == other.fallbackAction;
        }

        public override bool Equals(object obj)
        {
            return obj is CacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = configId;
                hash = (hash * 397) ^ rewardProfileId;
                hash = (hash * 397) ^ transitionProfileId;
                hash = (hash * 397) ^ settingsHash;
                hash = (hash * 397) ^ fallbackAction;
                return hash;
            }
        }
    }

    private static readonly Dictionary<CacheKey, NpcPolicySolution> cache =
        new Dictionary<CacheKey, NpcPolicySolution>();

    public static NpcActionType GetBestAction(
        NpcPlannerConfig config,
        NpcDecisionState state,
        bool verboseLogs = false,
        bool auditLogs = false)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        if (config == null)
        {
            stopwatch.Stop();
            if (auditLogs || verboseLogs)
            {
                Debug.LogWarning(
                    "[ROAE][AI][Decision][FAIL] reason=missing_planner_config" +
                    " state=" + state +
                    " fallback=" + NpcActionType.Neutral +
                    " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
            }

            return NpcActionType.Neutral;
        }

        NpcPolicySolution solution = GetOrBuildPolicy(config, verboseLogs, auditLogs);
        if (solution.policy.TryGetValue(state, out NpcActionType action))
        {
            stopwatch.Stop();
            if (auditLogs || verboseLogs)
            {
                Debug.Log(
                    "[ROAE][AI][Decision][SUCCESS] config=" + config.name +
                    " mode=" + solution.plannerMode +
                    " state=" + state +
                    " action=" + action +
                    " policyEntries=" + solution.policy.Count +
                    " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
            }

            return action;
        }

        stopwatch.Stop();

        if (verboseLogs)
            Debug.LogWarning("[ROAE][NpcPolicySolver] Missing policy entry for state=" + state + " fallback=" + solution.fallbackAction);

        if (auditLogs || verboseLogs)
        {
            Debug.LogWarning(
                "[ROAE][AI][Decision][FAIL] config=" + config.name +
                " mode=" + solution.plannerMode +
                " reason=missing_policy_entry" +
                " state=" + state +
                " fallback=" + solution.fallbackAction +
                " policyEntries=" + solution.policy.Count +
                " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
        }

        return solution.fallbackAction;
    }

    public static NpcPolicySolution GetOrBuildPolicy(
        NpcPlannerConfig config,
        bool verboseLogs = false,
        bool auditLogs = false)
    {
        if (config == null)
            return BuildEmptySolution(NpcActionType.Neutral);

        Stopwatch stopwatch = Stopwatch.StartNew();
        CacheKey key = BuildKey(config);
        if (cache.TryGetValue(key, out NpcPolicySolution cached))
        {
            stopwatch.Stop();
            if (verboseLogs)
                Debug.Log("[ROAE][NpcPolicySolver] Cache hit config=" + config.name + " mode=" + cached.plannerMode);

            if (auditLogs || verboseLogs)
            {
                Debug.Log(
                    "[ROAE][AI][PlannerCache][SUCCESS] config=" + config.name +
                    " mode=" + cached.plannerMode +
                    " cache=hit" +
                    " states=" + cached.policy.Count +
                    " policyEntries=" + cached.policy.Count +
                    " durationMs=" + stopwatch.Elapsed.TotalMilliseconds.ToString("0.00"));
            }

            return cached;
        }

        NpcSolverStats stats;
        NpcPolicySolution built = BuildSolution(config, verboseLogs, out stats);
        cache[key] = built;
        stopwatch.Stop();

        if (auditLogs || verboseLogs)
            LogPolicyBuild(config, built, stats, stopwatch.Elapsed.TotalMilliseconds);

        return built;
    }

    public static void Invalidate(NpcPlannerConfig config)
    {
        if (config == null || cache.Count == 0)
            return;

        int configId = config.GetInstanceID();
        List<CacheKey> keysToRemove = new List<CacheKey>();

        foreach (KeyValuePair<CacheKey, NpcPolicySolution> pair in cache)
        {
            if (pair.Key.configId == configId)
                keysToRemove.Add(pair.Key);
        }

        for (int i = 0; i < keysToRemove.Count; i++)
            cache.Remove(keysToRemove[i]);
    }

    public static void ClearCache()
    {
        cache.Clear();
    }

    private static CacheKey BuildKey(NpcPlannerConfig config)
    {
        return new CacheKey
        {
            configId = config.GetInstanceID(),
            rewardProfileId = config.RewardProfile != null ? config.RewardProfile.GetInstanceID() : 0,
            transitionProfileId = config.TransitionProfile != null ? config.TransitionProfile.GetInstanceID() : 0,
            settingsHash = config.ToSettings().GetHashCode(),
            fallbackAction = (int) config.FallbackAction
        };
    }

    private static NpcPolicySolution BuildSolution(
        NpcPlannerConfig config,
        bool verboseLogs,
        out NpcSolverStats stats)
    {
        NpcPlannerSettings settings = config.ToSettings();
        int stateCount = NpcStateSpaceGenerator.GetAllStates().Count;

        if (config.RewardProfile == null)
        {
            if (verboseLogs)
                Debug.LogWarning("[ROAE][NpcPolicySolver] Missing reward profile for config=" + config.name + ". Using fallback policy.");

            stats = NpcSolverStats.Create(
                false,
                "missing_reward_profile",
                stateCount,
                0,
                0,
                0,
                0f,
                false);

            return BuildFallbackSolution(config.FallbackAction, settings);
        }

        if (verboseLogs)
        {
            Debug.Log(
                "[ROAE][NpcPolicySolver] Build policy config=" + config.name +
                " mode=" + settings.plannerMode +
                " states=" + NpcStateSpaceGenerator.GetAllStates().Count +
                " rewardProfile=" + config.RewardProfile.name +
                " transitionProfile=" + (config.TransitionProfile != null ? config.TransitionProfile.name : "NULL"));
        }

        switch (settings.plannerMode)
        {
            case NpcPlannerMode.PolicyIteration:
                return RunPolicyIteration(config, settings, verboseLogs, out stats);

            default:
                return RunValueIteration(config, settings, out stats);
        }
    }

    private static NpcPolicySolution RunValueIteration(
        NpcPlannerConfig config,
        NpcPlannerSettings settings,
        out NpcSolverStats stats)
    {
        NpcActionType[] actions = GetPlannerActions(config);
        List<NpcDecisionState> states = NpcStateSpaceGenerator.GenerateAllStates();
        NpcValueIterationSolution solution = ValueIterationSolver.SolveWithValues(
            states,
            actions,
            (state, action) => NpcRewardEvaluator.GetReward(config.RewardProfile, state, action),
            (state, action) => NpcTransitionEvaluator.GetTransitions(config.TransitionProfile, state, action),
            settings.gamma,
            settings.epsilon,
            settings.maxValueIterations);

        NpcValueIterationRunStats runStats = ValueIterationSolver.LastRunStats;
        stats = NpcSolverStats.Create(
            true,
            runStats.converged ? "converged" : "max_iterations_reached",
            states.Count,
            actions.Length,
            runStats.iterations,
            0,
            runStats.finalDelta,
            runStats.converged);

        return new NpcPolicySolution(
            solution.policy,
            solution.values,
            settings.plannerMode,
            settings,
            config.FallbackAction);
    }

    private static NpcPolicySolution RunPolicyIteration(
        NpcPlannerConfig config,
        NpcPlannerSettings settings,
        bool verboseLogs,
        out NpcSolverStats stats)
    {
        NpcActionType[] actions = GetPlannerActions(config);
        List<NpcDecisionState> states = NpcStateSpaceGenerator.GenerateAllStates();
        Dictionary<NpcDecisionState, float> values = new Dictionary<NpcDecisionState, float>(states.Count);
        Dictionary<NpcDecisionState, NpcActionType> policy = new Dictionary<NpcDecisionState, NpcActionType>(states.Count);

        for (int i = 0; i < states.Count; i++)
        {
            values[states[i]] = 0f;
            policy[states[i]] = config.FallbackAction;
        }

        bool stablePolicy = false;
        int iterationsRun = 0;
        int evaluationSweeps = 0;
        float finalDelta = 0f;

        for (int iteration = 0; iteration < settings.maxPolicyIterations; iteration++)
        {
            iterationsRun = iteration + 1;

            for (int sweep = 0; sweep < settings.maxPolicyEvaluationSweeps; sweep++)
            {
                float delta = 0f;

                for (int stateIndex = 0; stateIndex < states.Count; stateIndex++)
                {
                    NpcDecisionState state = states[stateIndex];
                    float nextValue = ComputeActionValue(config, state, policy[state], values, settings);
                    delta = Mathf.Max(delta, Mathf.Abs(nextValue - values[state]));
                    values[state] = nextValue;
                }

                if (verboseLogs)
                    Debug.Log("[ROAE][NpcPolicySolver][PI][Eval] config=" + config.name + " iter=" + iteration + " sweep=" + sweep + " delta=" + delta.ToString("0.00000"));

                evaluationSweeps++;
                finalDelta = delta;

                if (delta < settings.epsilon)
                    break;
            }

            bool stable = true;
            int changed = 0;

            for (int stateIndex = 0; stateIndex < states.Count; stateIndex++)
            {
                NpcDecisionState state = states[stateIndex];
                NpcActionType previousAction = policy[state];
                NpcActionType bestAction = SelectBestAction(config, state, values, settings, actions);

                if (bestAction != previousAction)
                {
                    policy[state] = bestAction;
                    stable = false;
                    changed++;
                }
            }

            if (verboseLogs)
                Debug.Log("[ROAE][NpcPolicySolver][PI][Improve] config=" + config.name + " iter=" + iteration + " changed=" + changed + " stable=" + stable);

            if (stable)
            {
                stablePolicy = true;
                break;
            }
        }

        stats = NpcSolverStats.Create(
            true,
            stablePolicy ? "stable_policy" : "max_policy_iterations_reached",
            states.Count,
            actions.Length,
            iterationsRun,
            evaluationSweeps,
            finalDelta,
            stablePolicy);

        return new NpcPolicySolution(
            policy,
            values,
            settings.plannerMode,
            settings,
            config.FallbackAction);
    }

    private static NpcActionType SelectBestAction(
        NpcPlannerConfig config,
        NpcDecisionState state,
        Dictionary<NpcDecisionState, float> values,
        NpcPlannerSettings settings,
        NpcActionType[] actions)
    {
        NpcActionType bestAction = config.FallbackAction;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < actions.Length; i++)
        {
            NpcActionType action = actions[i];
            float score = ComputeActionValue(config, state, action, values, settings);
            if (score > bestScore)
            {
                bestScore = score;
                bestAction = action;
            }
        }

        return bestAction;
    }

    private static float ComputeActionValue(
        NpcPlannerConfig config,
        NpcDecisionState state,
        NpcActionType action,
        Dictionary<NpcDecisionState, float> values,
        NpcPlannerSettings settings)
    {
        float score = NpcRewardEvaluator.GetReward(config.RewardProfile, state, action);
        List<StateTransition> transitions = NpcTransitionEvaluator.GetTransitions(config.TransitionProfile, state, action);

        for (int i = 0; i < transitions.Count; i++)
        {
            StateTransition transition = transitions[i];
            if (values.TryGetValue(transition.nextState, out float nextValue))
                score += settings.gamma * transition.probability * nextValue;
        }

        return score;
    }

    private static NpcPolicySolution BuildFallbackSolution(
        NpcActionType fallbackAction,
        NpcPlannerSettings settings)
    {
        Dictionary<NpcDecisionState, NpcActionType> policy = new Dictionary<NpcDecisionState, NpcActionType>();
        Dictionary<NpcDecisionState, float> values = new Dictionary<NpcDecisionState, float>();
        IReadOnlyList<NpcDecisionState> states = NpcStateSpaceGenerator.GetAllStates();

        for (int i = 0; i < states.Count; i++)
        {
            policy[states[i]] = fallbackAction;
            values[states[i]] = 0f;
        }

        return new NpcPolicySolution(
            policy,
            values,
            settings.plannerMode,
            settings,
            fallbackAction);
    }

    private static NpcPolicySolution BuildEmptySolution(NpcActionType fallbackAction)
    {
        return new NpcPolicySolution(
            new Dictionary<NpcDecisionState, NpcActionType>(),
            new Dictionary<NpcDecisionState, float>(),
            NpcPlannerMode.ValueIteration,
            new NpcPlannerSettings(NpcPlannerMode.ValueIteration, 0.85f, 0.0001f, 1, 1, 1),
            fallbackAction);
    }

    private static NpcActionType[] GetPlannerActions(NpcPlannerConfig config)
    {
        HashSet<NpcActionType> actionSet = new HashSet<NpcActionType>();

        if (config != null)
            actionSet.Add(config.FallbackAction);

        if (config != null && config.RewardProfile != null)
        {
            var baseRewards = config.RewardProfile.BaseRewards;
            for (int i = 0; i < baseRewards.Count; i++)
                actionSet.Add(baseRewards[i].action);

            var rules = config.RewardProfile.Rules;
            for (int i = 0; i < rules.Count; i++)
                actionSet.Add(rules[i].action);
        }

        if (config != null && config.TransitionProfile != null)
        {
            var transitions = config.TransitionProfile.Transitions;
            for (int i = 0; i < transitions.Count; i++)
                actionSet.Add(transitions[i].action);
        }

        if (actionSet.Count == 0)
            AddDefaultActions(actionSet);

        NpcActionType[] actions = new NpcActionType[actionSet.Count];
        actionSet.CopyTo(actions);
        return actions;
    }

    private static void AddDefaultActions(HashSet<NpcActionType> actionSet)
    {
        actionSet.Add(NpcActionType.Neutral);
        actionSet.Add(NpcActionType.Warm);
        actionSet.Add(NpcActionType.Guarded);
        actionSet.Add(NpcActionType.Hint);
        actionSet.Add(NpcActionType.WarmHint);
        actionSet.Add(NpcActionType.GuardedHint);
    }

    private static void LogPolicyBuild(
        NpcPlannerConfig config,
        NpcPolicySolution solution,
        NpcSolverStats stats,
        double durationMs)
    {
        string level = stats.success ? "[SUCCESS]" : "[FAIL]";
        string message =
            "[ROAE][AI][PlannerBuild]" + level +
            " config=" + config.name +
            " mode=" + solution.plannerMode +
            " cache=miss" +
            " status=" + stats.status +
            " states=" + stats.stateCount +
            " actions=" + stats.actionCount +
            " policyEntries=" + solution.policy.Count +
            " iterations=" + stats.iterations +
            " evalSweeps=" + stats.evaluationSweeps +
            " finalDelta=" + stats.finalDelta.ToString("0.00000") +
            " converged=" + stats.converged +
            " fallback=" + solution.fallbackAction +
            " durationMs=" + durationMs.ToString("0.00");

        if (stats.success)
            Debug.Log(message);
        else
            Debug.LogWarning(message);
    }
}
