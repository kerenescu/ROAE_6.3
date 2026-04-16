using UnityEngine;

public class PhoneUIFlow : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject phoneUI;
    public GameObject phoneButtonOpen;
    public GameObject phoneButtonClose;

    private void Start()
    {
        // Setează starea inițială
        ClosePhone();
    }

    public void OnPhoneButtonPressed()
    {
        Debug.Log("[PhoneUIFlow] S-a apăsat butonul pentru a deschide telefonul.");
        OpenPhone();
    }

    public void OnCloseButtonPressed()
    {
        Debug.Log("[PhoneUIFlow] S-a apăsat butonul pentru a închide telefonul.");
        ClosePhone();
    }

    private void OpenPhone()
    {
        if (phoneUI != null) phoneUI.SetActive(true);
        if (phoneButtonOpen != null) phoneButtonOpen.SetActive(false);
        if (phoneButtonClose != null) phoneButtonClose.SetActive(true);
    }

    private void ClosePhone()
    {
        if (phoneUI != null) phoneUI.SetActive(false);
        if (phoneButtonOpen != null) phoneButtonOpen.SetActive(true);
        if (phoneButtonClose != null) phoneButtonClose.SetActive(false);
    }
}
