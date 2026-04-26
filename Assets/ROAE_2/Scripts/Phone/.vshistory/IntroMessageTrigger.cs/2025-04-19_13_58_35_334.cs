using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IntroMessageTrigger : MonoBehaviour
{
    public float delay = 7f;

    void Start()
    {
        Debug.Log("📨 [IntroTrigger] Start() APELAT!");

        PlayerPrefs.DeleteKey("IntroMessageSent"); // doar pt test!
        PlayerPrefs.Save();

        if (PlayerPrefs.GetInt("IntroMessageSent", 0) == 1)
        {
            Debug.Log("📨 [IntroTrigger] Mesajul fusese deja trimis. Obiectul se distruge.");
            Destroy(gameObject);
            return;
        }

        StartCoroutine(DelayedSend());
    }

    IEnumerator DelayedSend()
    {
        yield return new WaitForSecondsRealtime(delay);
        Debug.Log("✅ [IntroTrigger] Delay complet. Trimitem mesaj.");
        SendIntroMessage();
    }

    void SendIntroMessage()
    {
        if (PlayerPrefs.GetInt("IntroMessageSent", 0) == 1) return;

        string contactName = "Număr necunoscut (+40 XXX XXX XXX)";
        string senderName = "+40 XXX XXX XXX";
        var message = new PhoneMessage(senderName, "Începe să te miști, Rina. Altfel o să te înghită și pe tine.", false);

        MessageManager mm = FindObjectOfType<MessageManager>();
        if (mm == null)
        {
            Debug.LogError("❌ NU am găsit MessageManager!");
            return;
        }

        if (mm.conversations == null)
            mm.conversations = new List<PhoneConversation>();

        PhoneConversation convo = mm.conversations.Find(c => c.contactName == contactName);
        if (convo == null)
        {
            convo = new PhoneConversation(contactName);
            mm.conversations.Add(convo);
        }

        convo.messages.Add(message);
        Debug.Log("✅ Mesaj adăugat în conversație. Total: " + convo.messages.Count);

        mm.ShowConversations(); // Asigură afișarea în listă
        if (mm.phoneUI.activeSelf)
            mm.ShowConversationMessages(convo);

        var notifier = FindObjectOfType<MessageNotifier>();
        if (notifier != null)
        {
            notifier.ShowNotification(contactName);
        }

        PlayerPrefs.SetInt("IntroMessageSent", 1);
        PlayerPrefs.Save();
    }

    private static bool instanceExists = false;

    

}
