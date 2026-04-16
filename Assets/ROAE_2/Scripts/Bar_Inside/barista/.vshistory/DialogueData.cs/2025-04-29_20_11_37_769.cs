using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private string dialogueLine;
    [SerializeField] private List<DialogueChoice> choices = new List<DialogueChoice>();

    public string DialogueLine => dialogueLine;
    public IReadOnlyList<DialogueChoice> Choices => choices;
}
