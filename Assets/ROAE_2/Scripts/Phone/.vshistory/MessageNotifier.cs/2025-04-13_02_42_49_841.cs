using UnityEngine;
using TMPro;
using System.Collections;

public class MessageNotifier : MonoBehaviour
{
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;
    public float displayTime = 3f;

    private Coroutine currentNotification;

    private string currentSender = "";

    public void ShowNotification(string sender)
    {
        currentSender = sender;

        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
        }

        currentNotification = StartCoroutine(ShowRoutine(sender));
    }
    public void OnNotificationClick()
    {
        var mm = FindObjectOfType<MessageManager>();
        if (mm != null)
        {
            var convo = mm.conversations.Find(c => c.contactName == currentSender);
            if (convo != null)
            {
                if (!mm.phoneUI.activeSelf)
                    mm.TogglePhone();

                mm.ShowConversationMessages(convo);
            }
        }

        // opțional: ascunde notificarea forțat
        notificationPanel.SetActive(false);
    }


    IEnumerator ShowRoutine(string sender)
    {
        notificationText.text = $"📨 Mesaj nou de la {sender}";
        notificationPanel.SetActive(true);

        yield return new WaitForSeconds(displayTime);

        notificationPanel.SetActive(false);
    }
}
