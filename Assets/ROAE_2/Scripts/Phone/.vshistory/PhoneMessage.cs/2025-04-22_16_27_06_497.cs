using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PhoneMessage
{
    public string sender;                   // Cine trimite mesajul
    [TextArea] public string content;       // Textul mesajului
    public bool isOldConversation;          // Este un mesaj vechi (de background)?
    public bool isDecision;                 // Este un mesaj cu decizie?

    public List<DecisionChoice> choices;    // Opțiuni de răspuns (dacă e decizie)

    // Constructor simplu (mesaj standard)
    public PhoneMessage(string sender, string content, bool isOld = false)
    {
        this.sender = sender;
        this.content = content;
        this.isOldConversation = isOld;
        this.isDecision = false;
        this.choices = null;
    }

    // Constructor cu decizii
    public PhoneMessage(string sender, string content, List<DecisionChoice> choices)
    {
        this.sender = sender;
        this.content = content;
        this.isOldConversation = false;
        this.isDecision = true;
        this.choices = choices ?? new List<DecisionChoice>();
    }
}

[Serializable]
public class DecisionChoice
{
    public string responseText;       // Textul opțiunii (ex: "Da, vin!")
    public StatsEffect statsEffect;   // Efectul asupra stats (creativitate, empatie etc.)
    public PhoneMessage followUp;     // Mesajul care vine DUPĂ alegere (de la altcineva)
    public DecisionChoice(string responseText, StatsEffect statsEffect)
    {
        this.responseText = responseText;
        this.statsEffect = statsEffect;
    }
}
