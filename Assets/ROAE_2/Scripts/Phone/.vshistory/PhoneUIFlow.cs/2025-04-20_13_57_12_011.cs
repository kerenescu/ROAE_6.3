using UnityEngine;
using UnityEngine.UI;

public class PhoneUIFlow : MonoBehaviour
{
    [SerializeField] private GameObject phoneButtonOpen;
    [SerializeField] private GameObject phoneButtonClose;
    [SerializeField] private GameObject phoneUI;
    [SerializeField] private GameObject inbox;
    [SerializeField] private GameObject conversationPanel;

    void Start()
    {
        if (phoneUI == null)
        {
            Debug.LogError("📵 PhoneUI reference is NULL in PhoneUIFlow!");
        }
        else
        {
            Debug.Log("📱 PhoneUI is assigned in PhoneUIFlow.");

            if (!phoneUI.activeInHierarchy)
            {
                Debug.LogWarning("🚫 PhoneUI exists but is INACTIVE in hierarchy!");
                phoneUI.SetActive(true); // activăm forțat
            }
            else
            {
                Debug.Log("✅ PhoneUI is active in hierarchy.");
            }
        }

        phoneButtonOpen?.SetActive(true);
        phoneButtonClose?.SetActive(false);
        inbox?.SetActive(false);
        conversationPanel?.SetActive(false);
    }



    public void OnPhoneButtonPressed()
    {
        Debug.Log("📞 Phone open button pressed");
        phoneButtonOpen.SetActive(false);
        phoneButtonClose.SetActive(true);
        phoneUI.SetActive(true);
        inbox.SetActive(true);
        conversationPanel.SetActive(false); // opțional
    }

    public void OnCloseButtonPressed()
    {
        Debug.Log("📞 Phone close button pressed");
        phoneButtonOpen.SetActive(true);
        phoneButtonClose.SetActive(false);
        phoneUI.SetActive(false);
        inbox.SetActive(false);
        conversationPanel.SetActive(false); // opțional
    }

    public void ShowInbox()
    {
        inbox.SetActive(true);
        conversationPanel.SetActive(false);
    }

    public void ShowConversation()
    {
        inbox.SetActive(false);
        conversationPanel.SetActive(true);
    }
}
