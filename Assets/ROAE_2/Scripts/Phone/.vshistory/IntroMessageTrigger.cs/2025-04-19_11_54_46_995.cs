using UnityEngine;
using System.Collections;

public class IntroMessageTrigger : MonoBehaviour
{
    public float delay = 7f;
    private bool messageSent = false;

    void Start()
    {
        Debug.Log("📨 [IntroTrigger] Start() APELAT!");

        if (PlayerPrefs.GetInt("IntroMessageSent", 0) == 1)
        {
            Debug.Log("📨 [IntroTrigger] Mesajul fusese deja trimis. Obiectul se distruge.");
            Destroy(gameObject);
            return;
        }

        // Așteptăm un frame înainte să pornim corutina
        StartCoroutine(WaitOneFrameThenStart());
    }

    IEnumerator WaitOneFrameThenStart()
    {
        yield return null; // Așteaptă un frame
        yield return new WaitForSecondsRealtime(delay);
        Debug.Log("✅ [IntroTrigger] Delay terminat. Trimitem mesaj.");
        SendIntroMessage();
    }


    IEnumerator DelayedSend()
    {
        Debug.Log("⏳ [IntroTrigger] Începem așteptarea de " + delay + " secunde...");
        yield return new WaitForSecondsRealtime(delay); // NU afectat de Time.timeScale

        Debug.Log("✅ [IntroTrigger] Timpul a trecut. Trimitem mesajul.");
        SendIntroMessage();
    }

    void SendIntroMessage()
    {
        if (messageSent || PlayerPrefs.GetInt("IntroMessageSent", 0) == 1)
        {
            Debug.Log("📨 [IntroTrigger] Mesajul a fost deja trimis. Ieșim.");
            return;
        }

        messageSent = true;
        PlayerPrefs.SetInt("IntroMessageSent", 1);
        PlayerPrefs.Save();

        string contactName = "Număr necunoscut (+40 XXX XXX XXX)";
        string senderName = "+40 XXX XXX XXX";

        var message = new PhoneMessage(senderName, "Începe să te miști, Rina. Altfel o să te înghită și pe tine.", true);

        var mm = FindObjectOfType<MessageManager>();
        if (mm != null)
        {
            var convo = mm.conversations.Find(c => c.contactName == contactName);
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
            notifier.ShowNotification(contactName);
    }



    public bool devResetMessage = false;

    void Awake()
    {
        if (devResetMessage)
        {
            PlayerPrefs.DeleteKey("IntroMessageSent");
            PlayerPrefs.Save();
            Debug.Log("🧪 DEV: PlayerPrefs resetat pentru IntroMessage");
        }
    }

}
