using UnityEngine;
using UnityEngine.SceneManagement;

public class PhoneUIFlow : MonoBehaviour
{
    [Header("References")]
    public GameObject phoneUI;
    public GameObject phoneButton_Open;
    public GameObject phoneButton_Close;

    private bool isPhoneOpen = false;

    private void Awake()
    {
        // Singleton logic
        GameObject[] objs = GameObject.FindGameObjectsWithTag("PhoneSystem");
        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        Debug.Log("[PhoneUIFlow] Start() → Închidem telefonul inițial.");
        SafeClosePhone();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[PhoneUIFlow] OnSceneLoaded → Resetăm UI-ul telefonului pentru scena: {scene.name}");
        SafeClosePhone();
    }

    public void OnPhoneButtonPressed()
    {
        Debug.Log("[PhoneUIFlow] 🔓 Butonul de DESCHIDERE telefon a fost apăsat.");
        SafeOpenPhone();
    }

    public void OnCloseButtonPressed()
    {
        Debug.Log("[PhoneUIFlow] 🔒 Butonul de ÎNCHIDERE telefon a fost apăsat.");
        SafeClosePhone();
    }

    private void SafeOpenPhone()
    {
        if (!ValidateRefs()) return;

        phoneUI.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);
        isPhoneOpen = true;

        Debug.Log("[PhoneUIFlow] ✅ Telefon deschis, Time.timeScale = 0.");
        Time.timeScale = 0f;
    }

    private void SafeClosePhone()
    {
        if (!ValidateRefs()) return;

        phoneUI.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
        isPhoneOpen = false;

        Debug.Log("[PhoneUIFlow] ✅ Telefon închis, Time.timeScale = 1.");
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
