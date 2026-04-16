using UnityEngine;

public class BaristaActionSelector : MonoBehaviour
{
    [SerializeField] private string npcId = "barista";
    [SerializeField] private NpcActionType fallbackAction = NpcActionType.Neutral;

    public NpcActionType GetAction()
    {
        NpcDecisionState state = NpcStateDiscretizer.Build(npcId);

        if (state.relationship == RelationshipBucket.Bad)
            return NpcActionType.Guarded;

        if (state.corruption == CorruptionBucket.High)
            return NpcActionType.Guarded;

        if (state.empathy == EmpathyBucket.Low)
            return NpcActionType.Guarded;

        if (state.relationship == RelationshipBucket.Good)
            return NpcActionType.Warm;

        if (state.empathy == EmpathyBucket.High && state.creativity == CreativityBucket.High)
            return NpcActionType.Warm;

        if (state.creativity == CreativityBucket.High)
            return NpcActionType.Hint;

        if (state.empathy == EmpathyBucket.High)
            return NpcActionType.Warm;

        return fallbackAction;
    }
}