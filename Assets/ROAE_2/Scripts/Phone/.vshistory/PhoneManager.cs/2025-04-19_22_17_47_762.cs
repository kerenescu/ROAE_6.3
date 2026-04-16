using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public static PhoneManager Instance;

    [Header("UI")]
    public GameObject phoneUI;
    public MessageManager messageManager;

    private void Awake()
    {
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
