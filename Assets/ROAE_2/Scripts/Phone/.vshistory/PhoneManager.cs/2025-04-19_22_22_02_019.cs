using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public static PhoneManager Instance;

    [Header("UI")]
    public GameObject phoneUI;
    public MessageManager messageManager;

    private void Awake()
    {
        Debug.Log("🔥 Start PhoneManager")
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // evită duplicarea
        }
    }

    public void TogglePhoneUI()
    {
        Debug.Log("🔁 TogglePhoneUI called.");

        if (phoneUI == null)
        {
            Debug.LogError("❌ phoneUI is NULL!");
            return;
        }

        if (messageManager == null)
        {
            Debug.LogError("❌ messageManager is NULL!");
            return;
        }

        bool isActive = phoneUI.activeSelf;
        phoneUI.SetActive(!isActive);

        if (!isActive)
        {
            messageManager.ShowConversations(); // când îl deschizi → afișează lista
        }
    }


    public void ClosePhoneUI()
    {
        phoneUI.SetActive(false);
    }
}
