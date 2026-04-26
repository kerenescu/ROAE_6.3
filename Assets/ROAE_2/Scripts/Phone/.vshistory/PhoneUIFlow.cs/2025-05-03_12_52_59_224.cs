using UnityEngine;

public class PhoneUIFlow : MonoBehaviour
{
    [SerializeField] private GameObject interfataVizuala;
    [SerializeField] private GameObject phoneButton_Open;
    [SerializeField] private GameObject phoneButton_Close;
    [SerializeField] private MessageManager messageManager;

    private void Start()
    {

        Debug.Log("📱 PhoneUIFlow START");
        Debug.Log("🧩 interfataVizuala: " + (interfataVizuala == null ? "NULL" : "OK"));
        Debug.Log("🧩 phoneButton_Open: " + (phoneButton_Open == null ? "NULL" : "OK"));
        Debug.Log("🧩 phoneButton_Close: " + (phoneButton_Close == null ? "NULL" : "OK"));

        Debug.Log("🧩 messageManager: " + (messageManager == null ? "NULL" : "OK"));
        interfataVizuala.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
    }

    public void OnPhoneButtonPressed()
    {
        Time.timeScale = 0f; 

        interfataVizuala.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);
    }

    public void OnCloseButtonPressed()
    {
        Time.timeScale = 1f;

        interfataVizuala.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
        messageManager.ShowConversations();  // Revine la inbox
        messageManager.HideDecisionPanel();

        if (CaptionUIManager.Instance != null)
        {
            CaptionUIManager.Instance.ForceHideCaption();  // Ascunde gândul instant
        }

    }
}
