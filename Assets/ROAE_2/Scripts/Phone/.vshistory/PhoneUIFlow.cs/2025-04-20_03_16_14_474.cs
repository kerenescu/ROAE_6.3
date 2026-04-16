using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhoneUIFlow : MonoBehaviour
{
    [Header("UI References")]
    public GameObject phoneUI;
    public Button phoneButton_Open;
    public Button phoneButton_Close;

    private bool isPhoneOpen = false;

    private void Awake()
    {
        if (FindObjectsOfType<PhoneUIFlow>().Length > 1)
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[PhoneUIFlow] Scene loaded: {scene.name} → încercăm să legăm butoanele...");

        // Caută butoanele în scenă
        phoneButton_Open = GameObject.Find("PhoneButton_Open")?.GetComponent<Button>();
        phoneButton_Close = GameObject.Find("PhoneButton_Close")?.GetComponent<Button>();

        // Leagă evenimentele
        if (phoneButton_Open != null)
        {
            phoneButton_Open.onClick.RemoveAllListeners();
            phoneButton_Open.onClick.AddListener(OnPhoneButtonPressed);
            Debug.Log("✅ PhoneButton_Open legat.");
        }
        else Debug.LogWarning("⚠️ PhoneButton_Open nu a fost găsit!");

        if (phoneButton_Close != null)
        {
            phoneButton_Close.onClick.RemoveAllListeners();
            phoneButton_Close.onClick.AddListener(OnCloseButtonPressed);
            Debug.Log("✅ PhoneButton_Close legat.");
        }
        else Debug.LogWarning("⚠️ PhoneButton_Close nu a fost găsit!");

        SafeClosePhone(); // închidem telefonul la fiecare scenă nouă
    }

    public void OnPhoneButtonPressed()
    {
        Debug.Log("[PhoneUIFlow] 🔓 Telefon deschis.");
        SafeOpenPhone();
    }

    public void OnCloseButtonPressed()
    {
        Debug.Log("[PhoneUIFlow] 🔒 Telefon închis.");
        SafeClosePhone();
    }

    private void SafeOpenPhone()
    {
        if (phoneUI == null) return;

        phoneUI.SetActive(true);
        phoneButton_Open?.gameObject.SetActive(false);
        phoneButton_Close?.gameObject.SetActive(true);
        isPhoneOpen = true;
        Time.timeScale = 0f;
    }

    private void SafeClosePhone()
    {
        if (phoneUI == null) return;

        phoneUI.SetActive(false);
        phoneButton_Open?.gameObject.SetActive(true);
        phoneButton_Close?.gameObject.SetActive(false);
        isPhoneOpen = false;
        Time.timeScale = 1f;
    }
}
