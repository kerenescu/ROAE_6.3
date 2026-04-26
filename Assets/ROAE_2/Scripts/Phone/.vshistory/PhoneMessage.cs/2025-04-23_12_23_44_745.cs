private void LoadMessagesFromJSON()
{
    string savedPath = System.IO.Path.Combine(Application.persistentDataPath, "messages_saved.json");
    string sourcePath = System.IO.Path.Combine(Application.streamingAssetsPath, "messages_bootstrap.json");

    if (!System.IO.File.Exists(savedPath))
    {
        Debug.Log("📁 Salvare inexistentă. Copiem bootstrap-ul.");
        System.IO.File.Copy(sourcePath, savedPath);
    }

    string json = System.IO.File.ReadAllText(savedPath);
    PhoneData data = JsonUtility.FromJson<PhoneData>(json);

    conversations = new List<PhoneConversation>();

    foreach (var convoData in data.conversations)
    {
        PhoneConversation convo = new PhoneConversation(convoData.contactName);

        foreach (var msg in convoData.messages)
        {
            PhoneMessage newMsg = new PhoneMessage();
            newMsg.sender = msg.sender;
            newMsg.content = msg.text;
            newMsg.wasRead = msg.wasRead;
            newMsg.isDecision = msg.isDecision;
            newMsg.wasDecisionTaken = msg.wasDecisionTaken;
            newMsg.choices = msg.choices;

            convo.messages.Add(newMsg);
        }

        conversations.Add(convo);
    }

    Debug.Log($"📲 {conversations.Count} conversații încărcate din JSON.");
}

private void SaveMessages()
{
    PhoneData data = new PhoneData();
    data.conversations = new List<PhoneConversationData>();

    foreach (var convo in conversations)
    {
        PhoneConversationData convoData = new PhoneConversationData();
        convoData.contactName = convo.contactName;
        convoData.messages = new List<PhoneMessageData>();

        foreach (var msg in convo.messages)
        {
            PhoneMessageData msgData = new PhoneMessageData();
            msgData.sender = msg.sender;
            msgData.text = msg.content;
            msgData.wasRead = msg.wasRead;
            msgData.isDecision = msg.isDecision;
            msgData.wasDecisionTaken = msg.wasDecisionTaken;
            msgData.choices = msg.choices;

            convoData.messages.Add(msgData);
        }

        data.conversations.Add(convoData);
    }

    string json = JsonUtility.ToJson(data, true);
    string savePath = System.IO.Path.Combine(Application.persistentDataPath, "messages_saved.json");
    System.IO.File.WriteAllText(savePath, json);
    Debug.Log($"💾 Conversațiile au fost salvate în {savePath}");
}
