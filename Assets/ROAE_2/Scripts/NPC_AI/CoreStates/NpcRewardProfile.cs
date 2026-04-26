using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNpcRewardProfile", menuName = "Dialogue System/NPC Reward Profile")]
public class NpcRewardProfile : ScriptableObject
{
    [SerializeField] private List<NpcBaseActionReward> baseRewards = new List<NpcBaseActionReward>();
    [SerializeField] private List<NpcRewardRule> rules = new List<NpcRewardRule>();

    public IReadOnlyList<NpcBaseActionReward> BaseRewards => baseRewards;
    public IReadOnlyList<NpcRewardRule> Rules => rules;
}
