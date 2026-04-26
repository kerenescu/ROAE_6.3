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
    public bool wasDecisionTaken;           // A fost luată decizia? (pentru a nu mai arăta opțiunile după ce ai ales)

    public List<DecisionChoice> choices;    // Opțiuni de răspuns (dacă e decizie)
}

[Serializable]
public class DecisionChoice
{
    public string responseText;             // Textul opțiunii (ex: "Da, vin!")
    public StatsEffect statsEffect;         // Efectul asupra stats (creativitate, empatie etc.)
    public string followUpContent;          // Mesajul follow-up (mai simplu pentru JsonUtility)
}
