using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Replici secvențiale")]
    [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();

    [Header("Alegeri opționale (doar la final)")]
    [SerializeField] private List<DialogueChoice> choices = new List<DialogueChoice>();

    public IReadOnlyList<DialogueLine> DialogueLines => dialogueLines;
    public IReadOnlyList<DialogueChoice> Choices => choices;
}

[System.Serializable]
public class DialogueLine
{
    public string Speaker;

    [TextArea(2, 4)]
    public string Text;
}
