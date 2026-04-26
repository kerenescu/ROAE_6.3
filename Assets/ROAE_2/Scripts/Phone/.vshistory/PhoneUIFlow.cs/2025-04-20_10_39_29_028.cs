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
        // Dacă nu sunt setate din Inspector, le căutăm
        if (phoneButton_Open == null)
            phoneButton_Open = GameObject.Find("PhoneButton_Open");

        if (phoneButton_Close == null)
            phoneButton_Close = GameObject.Find("PhoneButton_Close");

        if (phoneUI == null)
            phoneUI = GameObject.Find("PhoneUI");

        ValidateRefs();
    }

    private void Start()
    {
        Debug.Log("[PhoneUIFlow] Start() → Închidem telefonul inițial.");
        SafeClosePhone();
    }

    public void OnPhoneButtonPressed()
    {
        Debug.Log("[PhoneUIFlow] 🔓 Deschidem telefonul.");
        SafeOpenPhone();
    }

    public void OnCloseButtonPressed()
    {
        Debug.Log("[PhoneUIFlow] 🔒 Închidem telefonul.");
        SafeClosePhone();
    }

    private void SafeOpenPhone()
    {
        if (!ValidateRefs()) return;

        phoneUI.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);
        isPhoneOpen = true;
        Time.timeScale = 0f;
    }

    private void SafeClosePhone()
    {
        if (!ValidateRefs()) return;

        phoneUI.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
        isPhoneOpen = false;
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
