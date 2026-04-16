using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    [SerializeField] private string choiceText;
    [SerializeField] private DialogueData nextDialogue;
    [SerializeField] private StatsEffect statEffect;
    [SerializeField] private RelationshipEffect relationshipEffect;

    public string ChoiceText => choiceText;
    public DialogueData NextDialogue => nextDialogue;
    public StatsEffect StatEffect => statEffect;
    public RelationshipEffect RelationshipEffect => relationshipEffect;
}