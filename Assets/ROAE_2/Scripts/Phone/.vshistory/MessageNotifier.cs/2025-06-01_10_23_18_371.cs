using UnityEngine;
using TMPro;
using System.Collections;

public class MessageNotifier : MonoBehaviour
{
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;
    public float displayTime = 3f;
    public AudioSource notificationAudio; 

    private Coroutine currentNotification;

    private string currentSender = "";

    public void ShowNotification(string sender)
    {
        currentSender = sender;

        if (notificationPanel == null)
        {
            Debug.LogError("❌ [Notifier] NotificationPanel NU este setat!");
            return;
        }

        if (notificationText == null)
        {
            Debug.LogError("❌ [Notifier] NotificationText NU este setat!");
            return;
        }

        if (notificationAudio != null)
            notificationAudio.Play();

        if (currentNotification != null)
            StopCoroutine(currentNotification);

        currentNotification = StartCoroutine(ShowRoutine(sender));
    }

    public void OnNotificationClick()
    {
        var mm = MessageManager.Instance;
        if (mm != null)
        {
            // 🔓 Deschide telefonul dacă e închis
            if (!mm.phoneUI.activeSelf)
            {
                mm.TogglePhone(); // deschide UI și face Time.timeScale = 0
            }

            // 🗂 Găsește conversația
            var convo = mm.conversations.Find(c => c.contactName == currentSender);
            if (convo != null)
            {
                mm.ShowConversationMessages(convo); // arată conversația
            }
            else
            {
                Debug.LogWarning($"❗ Nu am găsit conversația cu {currentSender}");
            }
        }

        // ✅ Ascunde notificarea
        notificationPanel.SetActive(false);
    }



    IEnumerator ShowRoutine(string sender)
    {
        notificationText.text = $"Mesaj nou de la {sender}";
        notificationPanel.SetActive(true);

        yield return new WaitForSeconds(displayTime);

        notificationPanel.SetActive(false);
    }
}
