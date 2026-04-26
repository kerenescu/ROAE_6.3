using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhoneMessage
{
    public string sender;           // Nume expeditor
    public string content;          // Conținutul mesajului
    public bool isOldConversation;  // E conversație veche?
    public bool hasChoices;         // Are opțiuni de răspuns?
    public List<string> responseOptions; // Lista de răspunsuri posibile

    public PhoneMessage(string sender, string content, bool isOld = false, bool hasChoices = false, List<string> options = null)
    {
        this.sender = sender;
        this.content = content;
        this.isOldConversation = isOld;
        this.hasChoices = hasChoices;
        this.responseOptions = options ?? new List<string>();
    }
}
