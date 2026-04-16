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

    // Constructor cu multiple choice
    public PhoneMessage(string sender, string content, List<string> responseOptions)
    {
        this.sender = sender;
        this.content = content;
        this.isOldConversation = false;
        this.hasChoices = true;
        this.responseOptions = responseOptions ?? new List<string>();
    }


}
