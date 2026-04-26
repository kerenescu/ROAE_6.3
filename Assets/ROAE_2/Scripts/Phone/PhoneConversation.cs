using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhoneConversation
{
    public string contactName;                  // Numele contactului (ex: "AI", "Curatorul")
    public List<PhoneMessage> messages;         // Toate mesajele din conversația cu acel contact

    public PhoneConversation(string name)
    {
        contactName = name;
        messages = new List<PhoneMessage>();
    }
}
