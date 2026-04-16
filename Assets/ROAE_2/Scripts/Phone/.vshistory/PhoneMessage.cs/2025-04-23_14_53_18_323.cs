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
    public bool wasRead;                    // A fost citit mesajul?
    public bool wasDecisionTaken;           // A fost luată decizia? (pentru a ascunde opțiunile după ce alegi)

    public List<DecisionChoice> choices;    // Opțiuni de răspuns (dacă e decizie)

    // Constructor simplu (mesaj standard)
    public PhoneMessage(string sender, string content, bool wasRead = false, bool isOld = false)
    {
        this.sender = sender;
        this.content = content;
        this.wasRead = wasRead;
        this.isOldConversation = isOld;
        this.isDecision = false;
        this.wasDecisionTaken = false;
        this.choices = null;
    }

    // Constructor cu decizie
    public PhoneMessage(string sender, string content, List<DecisionChoice> choices)
    {
        this.sender = sender;
        this.content = content;
        this.wasRead = false;
        this.isOldConversation = false;
        this.isDecision = true;
        this.wasDecisionTaken = false;
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
