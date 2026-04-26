using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    [SerializeField] private string choiceText;
    [SerializeField] private DialogueData nextDialogue;
    [SerializeField] private StatsEffect statEffect;
    [SerializeField] private RelationshipEffect relationshipEffect;
    [SerializeField] private List<DialogueChoiceEffect> extraEffects = new List<DialogueChoiceEffect>();

    public string ChoiceText => choiceText;
    public DialogueData NextDialogue => nextDialogue;
    public StatsEffect StatEffect => statEffect;
    public RelationshipEffect RelationshipEffect => relationshipEffect;
    public IReadOnlyList<DialogueChoiceEffect> ExtraEffects => extraEffects;
}