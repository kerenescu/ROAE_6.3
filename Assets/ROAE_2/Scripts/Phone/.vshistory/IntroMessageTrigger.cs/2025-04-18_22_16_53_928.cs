using UnityEngine;
using System.Collections;


public class IntroMessageTrigger : MonoBehaviour
{
    public float delay = 3f;
    private bool messageSent = false;

    void Start()
    {
        Debug.Log("📨 [IntroTrigger] Start() APELAT!");
        if (PlayerPrefs.GetInt("IntroMessageSent", 0) == 1)
        {
            Destroy(gameObject);
            return;
        }

        Invoke(nameof(SendIntroMessage), delay);
        Debug.Log("📨 [IntroTrigger] Invocare trimisă cu delay " + delay + " secunde.");
    }


    IEnumerator DelayedSend()
    {
        yield return new WaitForSecondsRealtime(delay);

        if (PlayerPrefs.GetInt("IntroMessageSent", 0) == 1) yield break;

        SendIntroMessage();
    }



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
