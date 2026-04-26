using System.Collections.Generic;
using UnityEngine;

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

    private static readonly NpcActionType[] actions =
    {
        NpcActionType.Neutral,
        NpcActionType.Warm,
        NpcActionType.Guarded,
        NpcActionType.Hint,
        NpcActionType.WarmHint,
        NpcActionType.GuardedHint
    };

    public static NpcActionType GetBestAction(
        NpcPlannerConfig config,
        NpcDecisionState state,
        bool verboseLogs = false)
    {
        if (config == null)
            return NpcActionType.Neutral;

        NpcPolicySolution solution = GetOrBuildPolicy(config, verboseLogs);
        if (solution.policy.TryGetValue(state, out NpcActionType action))
            return action;

        if (verboseLogs)
            Debug.LogWarning("[ROAE][NpcPolicySolver] Missing policy entry for state=" + state + " fallback=" + solution.fallbackAction);

        return solution.fallbackAction;
    }

    public static NpcPolicySolution GetOrBuildPolicy(
        NpcPlannerConfig config,
        bool verboseLogs = false)
    {
        if (config == null)
            return BuildEmptySolution(NpcActionType.Neutral);

        CacheKey key = BuildKey(config);
        if (cache.TryGetValue(key, out NpcPolicySolution cached))
        {
            if (verboseLogs)
                Debug.Log("[ROAE][NpcPolicySolver] Cache hit config=" + config.name + " mode=" + cached.plannerMode);

            return cached;
        }

        NpcPolicySolution built = BuildSolution(config, verboseLogs);
        cache[key] = built;
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

    private static NpcPolicySolution BuildSolution(NpcPlannerConfig config, bool verboseLogs)
    {
        NpcPlannerSettings settings = config.ToSettings();

        if (config.RewardProfile == null)
        {
            if (verboseLogs)
                Debug.LogWarning("[ROAE][NpcPolicySolver] Missing reward profile for config=" + config.name + ". Using fallback policy.");

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
                return RunPolicyIteration(config, settings, verboseLogs);

            default:
                return RunValueIteration(config, settings);
        }
    }

    private static NpcPolicySolution RunValueIteration(
        NpcPlannerConfig config,
        NpcPlannerSettings settings)
    {
        NpcValueIterationSolution solution = ValueIterationSolver.SolveWithValues(
            NpcStateSpaceGenerator.GenerateAllStates(),
            actions,
            (state, action) => NpcRewardEvaluator.GetReward(config.RewardProfile, state, action),
            (state, action) => NpcTransitionEvaluator.GetTransitions(config.TransitionProfile, state, action),
            settings.gamma,
            settings.epsilon,
            settings.maxValueIterations);

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
        bool verboseLogs)
    {
        List<NpcDecisionState> states = NpcStateSpaceGenerator.GenerateAllStates();
        Dictionary<NpcDecisionState, float> values = new Dictionary<NpcDecisionState, float>(states.Count);
        Dictionary<NpcDecisionState, NpcActionType> policy = new Dictionary<NpcDecisionState, NpcActionType>(states.Count);

        for (int i = 0; i < states.Count; i++)
        {
            values[states[i]] = 0f;
            policy[states[i]] = config.FallbackAction;
        }

        for (int iteration = 0; iteration < settings.maxPolicyIterations; iteration++)
        {
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

                if (delta < settings.epsilon)
                    break;
            }

            bool stable = true;
            int changed = 0;

            for (int stateIndex = 0; stateIndex < states.Count; stateIndex++)
            {
                NpcDecisionState state = states[stateIndex];
                NpcActionType previousAction = policy[state];
                NpcActionType bestAction = SelectBestAction(config, state, values, settings);

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
                break;
        }

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
        NpcPlannerSettings settings)
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
}
