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
            // 1️⃣ Activează direct UI-ul dacă e închis
            if (!mm.phoneUI.activeSelf)
            {
                mm.phoneUI.SetActive(true); // deschide UI-ul (vizual)
                mm.isOpen = true;

                // dezactivează coliziuni / gameplay
                Collider2D[] currentColliders = FindObjectsOfType<Collider2D>();
                foreach (Collider2D col in currentColliders)
                {
                    if (col != null)
                        col.enabled = false;
                }

                // dezactivează inputurile (dacă e cazul)
                if (mm.inputScripts != null)
                {
                    foreach (MonoBehaviour script in mm.inputScripts)
                    {
                        if (script != mm)
                            script.enabled = false;
                    }
                }

                Time.timeScale = 0f;

                // play audio
                if (mm.phoneOpenAudio != null)
                    mm.phoneOpenAudio.Play();
            }

            // 2️⃣ Caută conversația și deschide-o
            var convo = mm.conversations.Find(c => c.contactName == currentSender);
            if (convo != null)
            {
                // 2️⃣ Caută conversația și deschide-o
                var convo = mm.conversations.Find(c => c.contactName == currentSender);
                if (convo != null)
                {
                    mm.ShowConversationMessages(convo);

                    // forțează activarea UI-ului conversației
                    mm.scrollViewMessages.SetActive(true);
                    mm.scrollViewConversations.SetActive(false);
                    mm.backButton.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"❗ Conversația cu {currentSender} nu a fost găsită.");
                }

            }
            else
            {
                Debug.LogWarning($"❗ Conversația cu {currentSender} nu a fost găsită.");
            }
        }

        // 3️⃣ Ascunde notificarea
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
