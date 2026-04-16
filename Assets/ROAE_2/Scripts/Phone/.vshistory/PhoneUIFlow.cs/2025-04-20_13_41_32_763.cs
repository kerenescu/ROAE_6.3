using UnityEngine;

public class PhoneUIFlow : MonoBehaviour
{
    public GameObject phoneUI;
    public GameObject phoneButton_Open;
    public GameObject phoneButton_Close;

    public GameObject scrollViewMessages;
    public GameObject scrollViewConversations;

    private bool isPhoneOpen = false;

    private void Start()
    {
        phoneUI.SetActive(false);
        phoneButton_Close.SetActive(false);
        phoneButton_Open.SetActive(true);
    }

    public void OnPhoneButtonPressed()
    {
        phoneUI.SetActive(true);
        phoneButton_Close.SetActive(true);
        phoneButton_Open.SetActive(false);
        ShowInbox();
        isPhoneOpen = true;
    }

    public void OnCloseButtonPressed()
    {
        phoneUI.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
        isPhoneOpen = false;
    }

    public void ShowInbox()
    {
        scrollViewMessages.SetActive(false);
        scrollViewConversations.SetActive(true);
    }

    public void ShowMessages()
    {
        scrollViewMessages.SetActive(true);
        scrollViewConversations.SetActive(false);
    }
}
