using UnityEngine;
using UnityEngine.SceneManagement;

public class PhoneUIFlow : MonoBehaviour
{
    [Header("References")]
    public GameObject phoneUI;
    public GameObject phoneButton_Open;
    public GameObject phoneButton_Close;

    [Header("Inbox/Convo Panels")]
    public GameObject scrollView_Messages;
    public GameObject scrollView_Conversations;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (phoneButton_Open == null)
            phoneButton_Open = GameObject.Find("PhoneButton_Open");

        if (phoneButton_Close == null)
            phoneButton_Close = GameObject.Find("PhoneButton_Close");

        if (phoneUI == null)
            phoneUI = GameObject.Find("PhoneUI");

        if (phoneUI != null && !phoneUI.activeSelf)
            phoneUI.SetActive(true); // activăm ca să o putem gestiona safe


        ValidateRefs();
        SafeClosePhone();
    }

    public void OnPhoneButtonPressed() => SafeOpenPhone();
    public void OnCloseButtonPressed() => SafeClosePhone();

    public void ShowInbox()
    {
        if (scrollView_Messages != null)
            scrollView_Messages.SetActive(true);

        if (scrollView_Conversations != null)
            scrollView_Conversations.SetActive(false);
    }

    private void SafeOpenPhone()
    {
        if (!ValidateRefs()) return;

        phoneUI.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);
        Time.timeScale = 0f;
    }

    private void SafeClosePhone()
    {
        if (!ValidateRefs()) return;

        phoneUI.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
        Time.timeScale = 1f;
    }

    private bool ValidateRefs()
    {
        if (phoneUI == null || phoneButton_Open == null || phoneButton_Close == null)
        {
            Debug.LogError("❌ PhoneUIFlow: Referințe lipsă la UI!");
            return false;
        }
        return true;
    }
}
