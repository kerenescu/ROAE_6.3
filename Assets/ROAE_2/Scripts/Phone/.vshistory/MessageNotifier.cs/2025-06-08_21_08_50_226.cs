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
    private bool firstNotificationShown = false;
    private Coroutine pulseRoutine;

    public void ShowNotification(string sender)
    {
        currentSender = sender;

        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogError("❌ NotificationPanel/Text NU sunt setate!");
            return;
        }

        if (notificationAudio != null)
            notificationAudio.Play();

        if (currentNotification != null)
            StopCoroutine(currentNotification);

        currentNotification = StartCoroutine(ShowRoutine(sender));

        // 👉 Prima notificare: activează efectul!
        
            pulseRoutine = StartCoroutine(PulseEffect());
            firstNotificationShown = true;
        
    }


    public void OnNotificationClick()
    {
        var mm = MessageManager.Instance;
        if (mm != null)
        {
            // Activează vizual telefonul corect
            var flow = mm.phoneUI.GetComponent<PhoneUIFlow>();
            if (flow != null)
            {
                flow.OnPhoneButtonPressed(); // activează corect tot UI-ul
            }

            // Caută conversația și o deschide
            var convo = mm.conversations.Find(c => c.contactName == currentSender);
            if (convo != null)
            {
                mm.ShowConversationMessages(convo);
            }
            else
            {
                Debug.LogWarning($"❗ Conversația cu {currentSender} nu a fost găsită.");
            }


            if (pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
                pulseRoutine = null;
                notificationPanel.transform.localScale = Vector3.one;
            }

        }

        // Ascunde notificarea
        notificationPanel.SetActive(false);
    }





    IEnumerator ShowRoutine(string sender)
    {
        notificationText.text = $"Mesaj nou de la {sender}";
        notificationPanel.SetActive(true);

        yield return new WaitForSeconds(displayTime);

        notificationPanel.SetActive(false);
    }


    IEnumerator PulseEffect()
    {
        float pulseSpeed = 2f;
        float scaleAmount = 1.05f;
        Vector3 originalScale = notificationPanel.transform.localScale;

        while (true)
        {
            float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) / 2f;
            float scale = Mathf.Lerp(1f, scaleAmount, t);
            notificationPanel.transform.localScale = originalScale * scale;
            yield return null;
        }
    }

}
