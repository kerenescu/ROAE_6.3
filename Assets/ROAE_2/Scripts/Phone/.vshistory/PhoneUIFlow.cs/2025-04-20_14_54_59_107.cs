using UnityEngine;
using UnityEngine.UI;

public class PhoneUIFlow : MonoBehaviour
{
    [Header("UI References")]
    public GameObject phoneUI;
    public GameObject buttonOpen;
    public GameObject buttonClose;

    private void Start()
    {
        Debug.Log("📱 PhoneUIFlow START");

        phoneUI.SetActive(false);       // UI-ul complet de telefon e ascuns
        buttonOpen.SetActive(true);     // Apare doar butonul de deschidere
        buttonClose.SetActive(false);   // Butonul de închidere e ascuns
    }

    public void OnPhoneButtonPressed()
    {
        Debug.Log("📞 Phone open button pressed");

        phoneUI.SetActive(true);
        buttonOpen.SetActive(false);
        buttonClose.SetActive(true);
    }

    public void OnCloseButtonPressed()
    {
        Debug.Log("📞 Phone close button pressed");

        phoneUI.SetActive(false);
        buttonOpen.SetActive(true);
        buttonClose.SetActive(false);
    }
}
