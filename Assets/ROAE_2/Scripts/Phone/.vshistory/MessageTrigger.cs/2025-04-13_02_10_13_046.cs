using UnityEngine;

public class MessageTrigger : MonoBehaviour
{
    [Header("Mesaj de test")]
    public string senderName = "Elina";
    [TextArea]
    public string messageContent = "Hei, ai ajuns unde trebuia?";
    public bool isOld = false;

    private bool triggered = false;

    void Update()
    {
        // Apasă Space pentru a trimite un mesaj nou
        if (Input.GetKeyDown(KeyCode.Space) && !triggered)
        {
            SendTestMessage();
            triggered = true;
        }
    }

    void SendTestMessage()
    {
        var message = new PhoneMessage(senderName, messageContent, isOld);

        MessageManager mm = FindObjectOfType<MessageManager>();
        if (mm != null)
        {
            // 📨 Adăugăm conversația dacă nu există
            PhoneConversation convo = mm.conversations.Find(c => c.contactName == senderName);
            if (convo == null)
            {
                convo = new PhoneConversation(senderName);
                mm.conversations.Add(convo);
            }

            convo.messages.Add(message);

            // ✅ Dacă telefonul e deschis, actualizăm UI-ul direct
            if (mm.phoneUI.activeSelf)
            {
                // Dacă e în conversație cu acel contact → arată mesajul
                mm.ShowConversationMessages(convo);
            }

            Debug.Log($"[MessageTrigger] Mesaj trimis de la {senderName}");
        }
        else
        {
            Debug.LogWarning("[MessageTrigger] MessageManager nu a fost găsit în scenă!");
        }
    }
}
