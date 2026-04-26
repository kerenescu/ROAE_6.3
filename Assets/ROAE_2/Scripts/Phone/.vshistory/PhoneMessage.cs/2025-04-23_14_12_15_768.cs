using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PhoneMessage
{
    public string sender;
    [TextArea] public string content;
    public bool isOldConversation;
    public bool isDecision;
    public bool wasRead;
    public bool wasDecisionTaken;

    public List<DecisionChoice> choices;

    public PhoneMessage() { }


    // Constructor simplu (pentru mesaje instant)
    public PhoneMessage(string sender, string content)
    {
        this.sender = sender;
        this.content = content;
        this.wasRead = false;
        this.isOldConversation = false;
        this.isDecision = false;
        this.choices = null;
    }


    // Constructor pentru mesaje cu decizie
    public PhoneMessage(string sender, string content, List<DecisionChoice> choices)
    {
        this.sender = sender;
        this.content = content;
        this.wasRead = false;
        this.isOldConversation = false;
        this.isDecision = true;
        this.choices = choices ?? new List<DecisionChoice>();
    }

}



[Serializable]
public class DecisionChoice
{
    public string responseText;             // Textul opțiunii (ex: "Da, vin!")
    public StatsEffect statsEffect;         // Efectul asupra stats (creativitate, empatie etc.)
    public string followUpContent;          // Mesajul follow-up (mai simplu pentru JsonUtility)
}
