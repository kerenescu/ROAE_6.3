using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNpcTransitionProfile", menuName = "Dialogue System/NPC Transition Profile")]
public class NpcTransitionProfile : ScriptableObject
{
    [SerializeField] private List<NpcActionTransitionEntry> transitions = new List<NpcActionTransitionEntry>();

    public IReadOnlyList<NpcActionTransitionEntry> Transitions => transitions;
}
