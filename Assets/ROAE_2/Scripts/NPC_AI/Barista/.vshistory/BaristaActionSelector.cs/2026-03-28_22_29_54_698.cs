using UnityEngine;

public class BaristaActionSelector : MonoBehaviour
{
    [SerializeField] private NpcRelationshipState relationshipState;
    [SerializeField] private NpcActionType fallbackAction = NpcActionType.Neutral;

    [SerializeField] private int highCreativityThreshold = 70;
    [SerializeField] private int highEmpathyThreshold = 3;
    [SerializeField] private int lowEmpathyThreshold = -2;
    [SerializeField] private int highCorruptionThreshold = 7;

    [SerializeField] private int warmRelationshipThreshold = 2;
    [SerializeField] private int guardedRelationshipThreshold = -2;

    public NpcActionType GetAction()
    {
        if (CreativeCore.Instance == null)
            return fallbackAction;

        int creativity = CreativeCore.Instance.creativity;
        int empathy = CreativeCore.Instance.empathy;
        int corruption = CreativeCore.Instance.plantCorruption;
        int relationship = relationshipState != null ? relationshipState.RelationshipScore : 0;

        if (relationship <= guardedRelationshipThreshold)
            return NpcActionType.Guarded;

        if (corruption >= highCorruptionThreshold)
            return NpcActionType.Guarded;

        if (empathy <= lowEmpathyThreshold)
            return NpcActionType.Guarded;

        if (relationship >= warmRelationshipThreshold && empathy >= highEmpathyThreshold)
            return NpcActionType.Warm;

        if (relationship >= warmRelationshipThreshold)
            return NpcActionType.Warm;

        if (empathy >= highEmpathyThreshold && creativity >= highCreativityThreshold)
            return NpcActionType.Warm;

        if (creativity >= highCreativityThreshold)
            return NpcActionType.Hint;

        if (empathy >= highEmpathyThreshold)
            return NpcActionType.Warm;

        return fallbackAction;
    }
}