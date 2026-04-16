using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhoneConversation
{
    public string contactName;
    public Sprite contactIcon; // poți adăuga poză în viitor
    public List<PhoneMessage> messages = new List<PhoneMessage>();

    public PhoneConversation(string name)
    {
        contactName = name;
    }
}
