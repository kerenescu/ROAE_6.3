using UnityEngine;

public class PhoneUIFlow : MonoBehaviour
{
    public GameObject phoneUI;
    public GameObject phoneButton_Open;
    public GameObject phoneButton_Close;

    void Start()
    {
        Debug.Log("📱 PhoneUIFlow START");

        Debug.Log($"phoneUI = {(phoneUI != null)}");
        Debug.Log($"phoneButton_Open = {(phoneButton_Open != null)}");
        Debug.Log($"phoneButton_Close = {(phoneButton_Close != null)}");

        phoneUI.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);

        Debug.Log("✅ PhoneUIFlow initializat corect. Telefonul este închis, dar deschizătorul e vizibil.");
    }


    public void OnPhoneButtonPressed()
    {
        phoneUI.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);

        Debug.Log("📞 Phone open button pressed");
    }

    public void OnCloseButtonPressed()
    {
        phoneUI.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);

        Debug.Log("📞 Phone close button pressed");
    }
}
