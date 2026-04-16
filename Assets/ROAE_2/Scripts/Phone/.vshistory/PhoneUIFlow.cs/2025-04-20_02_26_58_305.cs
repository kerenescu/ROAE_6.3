using UnityEngine;

public class PhoneUIFlow : MonoBehaviour
{
    [Header("References")]
    public GameObject phoneUI;
    public GameObject phoneButton_Open;
    public GameObject phoneButton_Close;

    private bool isPhoneOpen = false;
    private void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("PhoneSystem");
        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        Debug.Log("[PhoneUIFlow] Start() → Închidem telefonul inițial.");
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
        if (phoneUI == null || phoneButton_Open == null || phoneButton_Close == null)
        {
            Debug.LogError("❌ PhoneUIFlow: Referințe lipsă la UI!");
            return;
        }

        phoneUI.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);
        isPhoneOpen = true;

        Debug.Log("[PhoneUIFlow] ✅ Telefon deschis, Time.timeScale = 0.");
        Time.timeScale = 0f;
    }

    private void SafeClosePhone()
    {
        if (phoneUI == null || phoneButton_Open == null || phoneButton_Close == null)
        {
            Debug.LogError("❌ PhoneUIFlow: Referințe lipsă la UI!");
            return;
        }

        phoneUI.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
        isPhoneOpen = false;

        Debug.Log("[PhoneUIFlow] ✅ Telefon închis, Time.timeScale = 1.");
        Time.timeScale = 1f;
    }
}
