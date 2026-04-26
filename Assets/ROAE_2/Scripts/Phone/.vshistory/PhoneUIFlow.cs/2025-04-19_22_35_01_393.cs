using UnityEngine;
using UnityEngine.UI;

public class PhoneUIFlow : MonoBehaviour
{
    [SerializeField] private GameObject phoneButton;
    [SerializeField] private GameObject phoneUI;
    [SerializeField] private GameObject closeButton;

    void Start()
    {
        phoneButton.SetActive(true);
        phoneUI.SetActive(false);
        closeButton.SetActive(false);
    }

    public void OnPhoneButtonPressed()
    {
        Debug.Log("📱 Telefon deschis");
        phoneButton.SetActive(false);
        phoneUI.SetActive(true);
        closeButton.SetActive(true);
    }

    public void ClosePhoneCompletely()
    {
        Debug.Log("📴 Telefon închis");
        phoneUI.SetActive(false);
        phoneButton.SetActive(true);
        closeButton.SetActive(false);
    }
}
