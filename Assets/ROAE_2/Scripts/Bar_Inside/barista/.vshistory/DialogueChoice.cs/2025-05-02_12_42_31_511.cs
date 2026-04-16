using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    [SerializeField] private string choiceText;
    [SerializeField] private DialogueData nextDialogue;
    [SerializeField] private StatsEffect statEffect;

    public string ChoiceText => choiceText;
    public DialogueData NextDialogue => nextDialogue;
    public StatsEffect StatEffect => statEffect;
}
