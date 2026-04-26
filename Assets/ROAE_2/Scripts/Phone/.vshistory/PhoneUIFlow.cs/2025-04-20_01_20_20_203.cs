using UnityEngine;

public class PhoneUIFlow : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject phoneUI;            // Telefonul în sine (cu conversații și mesaje)
    public GameObject phoneButtonOpen;    // Butonul vizibil când telefonul e închis
    public GameObject phoneButtonClose;   // Butonul vizibil când telefonul e deschis

    private void Start()
    {
        ClosePhone();  // Se asigură că telefonul e închis la start
    }

    public void OnPhoneButtonPressed()
    {
        Debug.Log("[PhoneUIFlow] Buton OPEN apăsat.");
        OpenPhone();
    }

    public void OnCloseButtonPressed()
    {
        Debug.Log("[PhoneUIFlow] Buton CLOSE apăsat.");
        ClosePhone();
    }

    private void OpenPhone()
    {
        if (phoneUI != null) phoneUI.SetActive(true);
        if (phoneButtonOpen != null) phoneButtonOpen.SetActive(false); // dispare butonul de deschidere
        if (phoneButtonClose != null) phoneButtonClose.SetActive(true); // apare cel de închidere
    }

    private void ClosePhone()
    {
        if (phoneUI != null) phoneUI.SetActive(false);
        if (phoneButtonOpen != null) phoneButtonOpen.SetActive(true); // reapare butonul de deschidere
        if (phoneButtonClose != null) phoneButtonClose.SetActive(false); // dispare cel de închidere
    }
}
