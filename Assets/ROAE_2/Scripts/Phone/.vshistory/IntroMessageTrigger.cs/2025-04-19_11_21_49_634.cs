using UnityEngine;
using System.Collections;


private float timer = 0f;
private bool waitingToSend = false;

void Start()
{
    if (PlayerPrefs.GetInt("IntroMessageSent", 0) == 1)
    {
        Destroy(gameObject);
        return;
    }

    waitingToSend = true;
    Debug.Log("📨 [IntroTrigger] Am pornit timerul pentru trimiterea mesajului");
}

void Update()
{
    if (!waitingToSend) return;

    timer += Time.unscaledDeltaTime;

    if (timer >= delay)
    {
        waitingToSend = false;
        SendIntroMessage();
    }
}



//IEnumerator DelayedSend()
//{
//    yield return new WaitForSecondsRealtime(delay);

//    if (PlayerPrefs.GetInt("IntroMessageSent", 0) == 1) yield break;

//    SendIntroMessage();
//}



void SendIntroMessage()
    {

        Debug.Log("📨 [IntroTrigger] SendIntroMessage() APELAT!");
        if (PlayerPrefs.GetInt("IntroMessageSent", 0) == 1) return;

        PlayerPrefs.SetInt("IntroMessageSent", 1); // salvăm că a fost trimis
        PlayerPrefs.Save(); // asigurăm persistarea
        if (messageSent) return;
        messageSent = true;

        string contactName = "Număr necunoscut (+40 XXX XXX XXX)";
        string senderName = "+40 XXX XXX XXX"; // ce apare în mesaj

        var message = new PhoneMessage(senderName, "Începe să te miști, Rina. Altfel o să te înghită și pe tine.", true);

        MessageManager mm = FindObjectOfType<MessageManager>();
        if (mm != null)
        {
            PhoneConversation convo = mm.conversations.Find(c => c.contactName == contactName);
            if (convo == null)
            {
                convo = new PhoneConversation(contactName);
                mm.conversations.Add(convo);
            }

            convo.messages.Add(message);

            if (mm.phoneUI.activeSelf)
                mm.ShowConversationMessages(convo);
        }

        var notifier = FindObjectOfType<MessageNotifier>();
        if (notifier != null)
        {
            notifier.ShowNotification(contactName);
        }
    }

}
