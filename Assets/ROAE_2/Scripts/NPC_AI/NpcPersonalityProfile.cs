using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNpcPersonalityProfile", menuName = "ROAE/NPC/Personality Profile")]
public class NpcPersonalityProfile : ScriptableObject
{
    [SerializeField] private List<NpcActionScoreRule> actionRules = new List<NpcActionScoreRule>();

    public IReadOnlyList<NpcActionScoreRule> ActionRules => actionRules;

    public float ScoreAction(NpcDecisionContext context, NpcActionType action)
    {
        float score = 0f;

        for (int i = 0; i < actionRules.Count; i++)
        {
            NpcActionScoreRule rule = actionRules[i];
            if (rule != null && rule.Matches(context, action))
                score += rule.ScoreDelta;
        }

        return score;
    }
}

[System.Serializable]
public class NpcActionScoreRule
{
    [SerializeField] private NpcActionType action = NpcActionType.Neutral;
    [SerializeField] private float scoreDelta = 1f;

    [Header("Optional gates")]
    [SerializeField] private bool useEmpathy;
    [SerializeField] private EmpathyBucket empathy;
    [SerializeField] private bool useCreativity;
    [SerializeField] private CreativityBucket creativity;
    [SerializeField] private bool useCorruption;
    [SerializeField] private CorruptionBucket corruption;
    [SerializeField] private bool useRelationship;
    [SerializeField] private RelationshipBucket relationship;
    [SerializeField] private List<string> requiredTrueFlags = new List<string>();
    [SerializeField] private List<string> requiredFalseFlags = new List<string>();

    public float ScoreDelta => scoreDelta;

    public bool Matches(NpcDecisionContext context, NpcActionType candidateAction)
    {
        if (candidateAction != action)
            return false;

        if (useEmpathy && context.EmpathyBucket != empathy)
            return false;

        if (useCreativity && context.CreativityBucket != creativity)
            return false;

        if (useCorruption && context.CorruptionBucket != corruption)
            return false;

        if (useRelationship && context.RelationshipBucket != relationship)
            return false;

        if (!NpcDecisionContextFlags.AllMatch(requiredTrueFlags, true))
            return false;

        if (!NpcDecisionContextFlags.AllMatch(requiredFalseFlags, false))
            return false;

        return true;
    }
}
