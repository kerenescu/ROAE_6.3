using UnityEngine;
using System.Collections;

public class IntroMessageTrigger : MonoBehaviour
{
    [Header("Config")]
    public float delay = 7f;
    public bool devResetMessage = false;

    private bool messageSent = false;

    void Awake()
    {
        if (devResetMessage)
        {
            PlayerPrefs.DeleteKey("IntroMessageSent");
            PlayerPrefs.Save();
            Debug.Log("🧪 DEV: PlayerPrefs resetat pentru IntroMessage");
        }
    }

    void Start()
    {
        Debug.Log("📨 [IntroTrigger] Start() APELAT!");

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
        Debug.Log("⏳ [IntroTrigger] Aștept " + delay + " secunde...");
        yield return new WaitForSecondsRealtime(delay);
        Debug.Log("✅ [IntroTrigger] Delay terminat. Trimitem mesajul...");
        SendIntroMessage();
    }

    void SendIntroMessage()
    {
        if (messageSent || PlayerPrefs.GetInt("IntroMessageSent", 0) == 1)
        {
            Debug.Log("📨 [IntroTrigger] Mesajul deja trimis. Ieșim.");
            return;
        }

        messageSent = true;

        // Asigurăm că MessageManager e pregătit
        var mm = FindObjectOfType<MessageManager>();
        if (mm == null)
        {
            Debug.LogWarning("❌ [IntroTrigger] NU am găsit MessageManager.");
            return;
        }

        if (mm.conversations == null)
            mm.conversations = new System.Collections.Generic.List<PhoneConversation>();

        string contactName = "Număr necunoscut (+40 XXX XXX XXX)";
        string senderName = "+40 XXX XXX XXX";

        var message = new PhoneMessage(senderName, "Începe să te miști, Rina. Altfel o să te înghită și pe tine.", true);

        var convo = mm.conversations.Find(c => c.contactName == contactName);
        if (convo == null)
        {
            convo = new PhoneConversation(contactName);
            mm.conversations.Add(convo);
        }

        convo.messages.Add(message);
        convo.messages.Add(message);

        Debug.Log("📥 [IntroTrigger] Mesaj adăugat în conversație.");

        if (mm.phoneUI.activeSelf)
        {
            mm.ShowConversationMessages(convo);
        }

        var notifier = FindObjectOfType<MessageNotifier>();
        if (notifier != null)
        {
            notifier.ShowNotification(contactName);
            Debug.Log("🔔 [IntroTrigger] Notificare trimisă.");
        }
        else
        {
            Debug.LogWarning("❌ [IntroTrigger] NU am găsit MessageNotifier.");
        }

        PlayerPrefs.SetInt("IntroMessageSent", 1);
        PlayerPrefs.Save();
        Debug.Log("✅ [IntroTrigger] PlayerPrefs marcat cu IntroMessageSent = 1");
    }
}
