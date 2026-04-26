using UnityEngine;

public class MessageTrigger : MonoBehaviour
{
    public string senderName = "Elina";
    public string messageContent = "Hei, ai ajuns unde trebuia?";
    public bool isOld = false;

    private bool triggered = false;

    void Update()
    {
        // ⚠️ Doar pentru test: trimite mesaj când apeși T
        if (Input.GetKeyDown(KeyCode.T) && !triggered)
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
            mm.ReceiveMessage(message);
            Debug.Log($"[MessageTrigger] Trimis mesaj de la {senderName}");
        }
        else
        {
            Debug.LogWarning("[MessageTrigger] MessageManager nu a fost găsit!");
        }
    }
}
