using UnityEngine;

public class IntroMessageTrigger : MonoBehaviour
{
    public float delay = 3f;
    private bool messageSent = false;

    private float timer = 0f;
    private bool waitingToSend = false;

    void Start()
    {
        Debug.Log("📨 [IntroTrigger] Start() APELAT!");

        if (PlayerPrefs.GetInt("IntroMessageSent", 0) == 1)
        {
            Debug.Log("📨 [IntroTrigger] Mesajul fusese deja trimis. Obiectul se distruge.");
            Destroy(gameObject);
            return;
        }

        waitingToSend = true;
        Debug.Log("📨 [IntroTrigger] Timer pornit. Așteptăm " + delay + " secunde.");
    }

    void Update()
    {
        if (!waitingToSend) return;

        timer += Time.unscaledDeltaTime;
        Debug.Log("⏳ [IntroTrigger] Timer: " + timer.ToString("F2"));

        if (timer >= delay)
        {
            Debug.Log("✅ [IntroTrigger] Timer finalizat. Apelăm SendIntroMessage.");
            waitingToSend = false;
            SendIntroMessage();
        }
    }

    void SendIntroMessage()
    {
        Debug.Log("📨 [IntroTrigger] SendIntroMessage() APELAT!");

        if (PlayerPrefs.GetInt("IntroMessageSent", 0) == 1)
        {
            Debug.Log("📨 [IntroTrigger] PlayerPrefs spune că mesajul a fost deja trimis. Nu mai trimitem.");
            return;
        }

        PlayerPrefs.SetInt("IntroMessageSent", 1);
        PlayerPrefs.Save();

        if (messageSent)
        {
            Debug.Log("📨 [IntroTrigger] messageSent = true. Ieșim.");
            return;
        }

        messageSent = true;

        string contactName = "Număr necunoscut (+40 XXX XXX XXX)";
        string senderName = "+40 XXX XXX XXX";

        var message = new PhoneMessage(senderName, "Începe să te miști, Rina. Altfel o să te înghită și pe tine.", true);

        MessageManager mm = FindObjectOfType<MessageManager>();
        if (mm != null)
        {
            Debug.Log("📨 [IntroTrigger] Am găsit MessageManager.");
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
        else
        {
            Debug.LogWarning("⚠️ [IntroTrigger] NU am găsit MessageManager!");
        }

        var notifier = FindObjectOfType<MessageNotifier>();
        if (notifier != null)
        {
            Debug.Log("📨 [IntroTrigger] Trimitem notificarea!");
            notifier.ShowNotification(contactName);
        }
        else
        {
            Debug.LogWarning("⚠️ [IntroTrigger] NU am găsit MessageNotifier!");
        }
    }
}
