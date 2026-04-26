using UnityEngine;
using UnityEngine.SceneManagement;

public class PhoneUIFlow : MonoBehaviour
{
    public GameObject phoneUI;
    public GameObject phoneButton_Open;
    public GameObject phoneButton_Close;

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
        // Căutăm butoanele doar după ce intrăm într-o scenă reală
        if (phoneButton_Open == null)
            phoneButton_Open = GameObject.Find("PhoneButton_Open");

        if (phoneButton_Close == null)
            phoneButton_Close = GameObject.Find("PhoneButton_Close");

        if (phoneUI == null)
            phoneUI = GameObject.Find("PhoneUI");

        ValidateRefs();
        SafeClosePhone();
    }

    public void OnPhoneButtonPressed() => SafeOpenPhone();
    public void OnCloseButtonPressed() => SafeClosePhone();

    private void SafeOpenPhone()
    {
        if (!ValidateRefs()) return;
        phoneUI.SetActive(true);
        phoneButton_Open?.SetActive(false);
        phoneButton_Close?.SetActive(true);
        Time.timeScale = 0f;
    }

    private void SafeClosePhone()
    {
        if (!ValidateRefs()) return;
        phoneUI.SetActive(false);
        phoneButton_Open?.SetActive(true);
        phoneButton_Close?.SetActive(false);
        Time.timeScale = 1f;
    }

    private bool ValidateRefs()
    {
        if (phoneUI == null || phoneButton_Open == null || phoneButton_Close == null)
        {
            Debug.LogError("❌ PhoneUIFlow: Referinte lipsa la UI!");
            return false;
        }
        return true;
    }
}
