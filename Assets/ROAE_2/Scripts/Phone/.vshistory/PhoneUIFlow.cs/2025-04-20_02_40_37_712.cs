using UnityEngine;
using UnityEngine.SceneManagement;

public class PhoneUIFlow : MonoBehaviour
{
    public static PhoneUIFlow Instance { get; private set; }

    [Header("References")]
    public GameObject phoneUI;
    public GameObject phoneButton_Open;
    public GameObject phoneButton_Close;

    private bool isPhoneOpen = false;

    private void Awake()
    {
        if (FindObjectsOfType<PhoneUIFlow>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void Start()
    {
        Debug.Log("[PhoneUIFlow] Start() → Închidem telefonul inițial.");
        SafeClosePhone();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[PhoneUIFlow] OnSceneLoaded → Reset telefon pentru scena {scene.name}");
        SafeClosePhone();
    }

    public void OnPhoneButtonPressed()
    {
        Debug.Log("[PhoneUIFlow] 🔓 Deschidere telefon.");
        SafeOpenPhone();
    }

    public void OnCloseButtonPressed()
    {
        Debug.Log("[PhoneUIFlow] 🔒 Închidere telefon.");
        SafeClosePhone();
    }

    private void SafeOpenPhone()
    {
        if (!ValidateRefs()) return;

        phoneUI.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);
        isPhoneOpen = true;

        Debug.Log("[PhoneUIFlow] ✅ Telefon deschis. Time.timeScale = 0.");
        Time.timeScale = 0f;
    }

    private void SafeClosePhone()
    {
        if (!ValidateRefs()) return;

        phoneUI.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
        isPhoneOpen = false;

        Debug.Log("[PhoneUIFlow] ✅ Telefon închis. Time.timeScale = 1.");
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
