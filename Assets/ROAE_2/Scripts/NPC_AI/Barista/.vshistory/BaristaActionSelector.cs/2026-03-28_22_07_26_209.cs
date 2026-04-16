using UnityEngine;

public class BaristaActionSelector : MonoBehaviour
{
    [SerializeField] private NpcActionType fallbackAction = NpcActionType.Neutral;

    [SerializeField] private int highCreativityThreshold = 70;
    [SerializeField] private int highEmpathyThreshold = 3;
    [SerializeField] private int lowEmpathyThreshold = -2;
    [SerializeField] private int highCorruptionThreshold = 7;

    public NpcActionType GetAction()
    {
        if (CreativeCore.Instance == null)
            return fallbackAction;

        int creativity = CreativeCore.Instance.creativity;
        int empathy = CreativeCore.Instance.empathy;
        int corruption = CreativeCore.Instance.plantCorruption;

        if (corruption >= highCorruptionThreshold)
            return NpcActionType.Guarded;

        if (empathy <= lowEmpathyThreshold)
            return NpcActionType.Guarded;

        if (empathy >= highEmpathyThreshold && creativity >= highCreativityThreshold)
            return NpcActionType.Warm;

        if (creativity >= highCreativityThreshold)
            return NpcActionType.Hint;

        if (empathy >= highEmpathyThreshold)
            return NpcActionType.Warm;

        return fallbackAction;
    }
}