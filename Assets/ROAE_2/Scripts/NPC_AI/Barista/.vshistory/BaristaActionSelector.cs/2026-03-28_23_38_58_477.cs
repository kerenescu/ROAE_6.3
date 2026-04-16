using UnityEngine;

public class BaristaActionSelector : MonoBehaviour
{
    [SerializeField] private string npcId = "barista";
    [SerializeField] private NpcActionType fallbackAction = NpcActionType.Neutral;

    public NpcActionType GetAction()
    {
        NpcDecisionState state = NpcStateDiscretizer.Build(npcId);

        if (state.relationship == RelationshipBucket.Bad)
        {
            if (state.creativity == CreativityBucket.High)
                return NpcActionType.GuardedHint;

            return NpcActionType.Guarded;
        }

        if (state.corruption == CorruptionBucket.High)
        {
            if (state.creativity == CreativityBucket.High)
                return NpcActionType.GuardedHint;

            return NpcActionType.Guarded;
        }

        if (state.empathy == EmpathyBucket.Low)
        {
            if (state.creativity == CreativityBucket.High)
                return NpcActionType.GuardedHint;

            return NpcActionType.Guarded;
        }

        if (state.relationship == RelationshipBucket.Good)
        {
            if (state.creativity == CreativityBucket.High)
                return NpcActionType.WarmHint;

            return NpcActionType.Warm;
        }

        if (state.empathy == EmpathyBucket.High && state.creativity == CreativityBucket.High)
            return NpcActionType.WarmHint;

        if (state.creativity == CreativityBucket.High)
            return NpcActionType.Hint;

        if (state.empathy == EmpathyBucket.High)
            return NpcActionType.Warm;

        return fallbackAction;
    }
}
