using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PhoneMessage
{
    public string sender;                 // Nume expeditor
    [TextArea] public string content;     // Conținutul mesajului
    public bool isOldConversation;        // Este conversație veche?
    public bool hasChoices;               // Are opțiuni de răspuns?
    public List<string> responseOptions;  // Răspunsuri posibile

    // Constructor standard
    public PhoneMessage(string sender, string content, bool isOld = false)
    {
        this.sender = sender;
        this.content = content;
        this.isOldConversation = isOld;
        this.hasChoices = false;
        this.responseOptions = new List<string>();
    }

    public bool isChoice = false;
    public List<string> responseOptions;

    public PhoneMessage(string sender, string content, bool isOld, bool isChoice = false, List<string> responseOptions = null)
    {
        this.sender = sender;
        this.content = content;
        this.isOldConversation = isOld;
        this.isChoice = isChoice;
        this.responseOptions = responseOptions ?? new List<string>();
    }


}
