using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNpcDefinition", menuName = "ROAE/NPC/Definition")]
public class NpcDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string npcId = "npc";
    [SerializeField] private string displayName = "NPC";

    [Header("Decision Model")]
    [SerializeField] private NpcPlannerConfig plannerConfig;
    [SerializeField] private NpcPersonalityProfile personalityProfile;
    [SerializeField] private List<NpcActionType> availableActions = new List<NpcActionType>
    {
        NpcActionType.Neutral,
        NpcActionType.Warm,
        NpcActionType.Guarded,
        NpcActionType.Hint,
        NpcActionType.WarmHint,
        NpcActionType.GuardedHint
    };

    [Header("Responses")]
    [SerializeField] private NpcResponseSet responseSet;

    public string NpcId => string.IsNullOrWhiteSpace(npcId) ? name : npcId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? NpcId : displayName;
    public NpcPlannerConfig PlannerConfig => plannerConfig;
    public NpcPersonalityProfile PersonalityProfile => personalityProfile;
    public NpcResponseSet ResponseSet => responseSet;
    public IReadOnlyList<NpcActionType> AvailableActions => availableActions;

    public bool HasAction(NpcActionType action)
    {
        return availableActions == null || availableActions.Count == 0 || availableActions.Contains(action);
    }
}
