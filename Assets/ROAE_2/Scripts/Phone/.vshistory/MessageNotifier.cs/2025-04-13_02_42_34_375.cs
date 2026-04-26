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
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
        }

        currentNotification = StartCoroutine(ShowRoutine(sender));
    }

    IEnumerator ShowRoutine(string sender)
    {
        notificationText.text = $"📨 Mesaj nou de la {sender}";
        notificationPanel.SetActive(true);

        yield return new WaitForSeconds(displayTime);

        notificationPanel.SetActive(false);
    }
}
