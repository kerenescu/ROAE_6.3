using System.Collections.Generic;

[System.Serializable]
public class PhoneData
{
    public List<PhoneConversationData> conversations;
}

[System.Serializable]
public class PhoneConversationData
{
    public string contactName;
    public List<PhoneMessageData> messages;
}

[System.Serializable]
public class PhoneMessageData
{
    public string sender;
    public string text;
    public bool wasRead;
    public bool isDecision;
    public bool wasDecisionTaken;
    public List<DecisionChoice> choices;
}

[System.Serializable]
public class DecisionChoice
{
    public string responseText;
    public StatsEffect statsEffect;
    public string followUpContent;
}
