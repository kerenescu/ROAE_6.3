using System;
using UnityEngine;
using static DialogueManager;

[Serializable]
public class DialogueChoice
{
    [SerializeField] private string choiceText;
    [SerializeField] private DialogueData nextDialogue;

    public string ChoiceText => choiceText;
    public DialogueData NextDialogue => nextDialogue;
}
