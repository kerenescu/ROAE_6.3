using UnityEngine;

public class IntroMessageTrigger : MonoBehaviour
{
    public float delay = 3f;
    private bool messageSent = false;

    void Start()
    {
        Invoke("SendIntroMessage", delay);
    }

    void SendIntroMessage()
    {
        if (messageSent) return;
        messageSent = true;

        string contactName = "Număr necunoscut (+40 XXX XXX XXX)";
        string senderName = "+40 XXX XXX XXX"; // ce apare în mesaj

        var message = new PhoneMessage(senderName, "Începe să te miști, Rina. Altfel o să te înghită și pe tine.", false);

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
