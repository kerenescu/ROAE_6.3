using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
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


[Serializable]
public class DecisionChoice
{
    public string responseText;             // Textul opțiunii (ex: "Da, vin!")
    public StatsEffect statsEffect;         // Efectul asupra stats (creativitate, empatie etc.)
    public string followUpContent;          // Mesajul follow-up (mai simplu pentru JsonUtility)
}
