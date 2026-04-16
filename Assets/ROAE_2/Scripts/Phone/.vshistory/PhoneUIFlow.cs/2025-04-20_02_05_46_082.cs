using UnityEngine;

public class PhoneUIFlow : MonoBehaviour
{
    [Header("References")]
    public GameObject phoneUI;
    public GameObject phoneButton_Open;
    public GameObject phoneButton_Close;

    private bool isPhoneOpen = false;

    private void Start()
    {
        Debug.Log("[PhoneUIFlow] Start() → Închidem telefonul inițial.");
        ClosePhone();
    }

    public void OnPhoneButtonPressed()
    {
        Debug.Log("[PhoneUIFlow] Butonul de DESCHIDERE telefon a fost apăsat.");
        OpenPhone();
    }

    public void OnCloseButtonPressed()
    {
        Debug.Log("[PhoneUIFlow] Butonul de ÎNCHIDERE telefon a fost apăsat.");
        ClosePhone();
    }

    private void OpenPhone()
    {
        Debug.Log("[PhoneUIFlow] Deschidem UI-ul telefonului...");
        phoneUI.SetActive(true);

        Debug.Log("[PhoneUIFlow] Ascundem butonul de deschidere.");
        phoneButton_Open.SetActive(false);

        Debug.Log("[PhoneUIFlow] Afișăm butonul de închidere.");
        phoneButton_Close.SetActive(true);

        isPhoneOpen = true;

        Debug.Log("[PhoneUIFlow] Time.timeScale = 0 (pauză joc).");
        Time.timeScale = 0f;
    }

    private void ClosePhone()
    {
        Debug.Log("[PhoneUIFlow] Închidem UI-ul telefonului...");
        phoneUI.SetActive(false);

        Debug.Log("[PhoneUIFlow] Afișăm butonul de deschidere.");
        phoneButton_Open.SetActive(true);

        Debug.Log("[PhoneUIFlow] Ascundem butonul de închidere.");
        phoneButton_Close.SetActive(false);

        isPhoneOpen = false;

        Debug.Log("[PhoneUIFlow] Time.timeScale = 1 (continuare joc).");
        Time.timeScale = 1f;
    }
}
