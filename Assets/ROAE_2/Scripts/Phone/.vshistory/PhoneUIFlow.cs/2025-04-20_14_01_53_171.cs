using UnityEngine;

public class PhoneUIFlow : MonoBehaviour
{
    [SerializeField] private GameObject phoneUI;
    [SerializeField] private GameObject phoneButton_Open;
    [SerializeField] private GameObject phoneButton_Close;
    [SerializeField] private GameObject scrollView_Messages;
    [SerializeField] private GameObject scrollView_Conversations;

    void Start()
    {
        Debug.Log("📱 PhoneUIFlow START");

        if (phoneUI == null || phoneButton_Open == null || phoneButton_Close == null)
        {
            Debug.LogError("❌ Referințe lipsă în PhoneUIFlow!");
            return;
        }

        // Activăm tot ca să știm sigur că e vizibil
        phoneUI.SetActive(true);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);

        scrollView_Messages?.SetActive(false);
        scrollView_Conversations?.SetActive(true);

        Debug.Log("✅ PhoneUIFlow initializat corect. Telefonul este închis, dar deschizătorul e vizibil.");
    }


    public void OnPhoneButtonPressed()
    {
        phoneUI.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);
        scrollView_Messages.SetActive(false);
        scrollView_Conversations.SetActive(true);

        Debug.Log("📞 Phone open button pressed");
    }

    public void OnCloseButtonPressed()
    {
        phoneUI.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);

        Debug.Log("📞 Phone close button pressed");
    }

    public void ShowInbox()
    {
        scrollView_Messages.SetActive(false);
        scrollView_Conversations.SetActive(true);
    }

    public void ShowMessages()
    {
        scrollView_Messages.SetActive(true);
        scrollView_Conversations.SetActive(false);
    }
}
