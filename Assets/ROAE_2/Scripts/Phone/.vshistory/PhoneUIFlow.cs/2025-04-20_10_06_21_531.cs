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
        SafeClosePhone(); // închide la pornire
    }

    public void OnPhoneButtonPressed()
    {
        SafeOpenPhone();
    }

    public void OnCloseButtonPressed()
    {
        SafeClosePhone();
    }

    private void SafeOpenPhone()
    {
        phoneUI.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);
        isPhoneOpen = true;
        Time.timeScale = 0f;
    }

    private void SafeClosePhone()
    {
        phoneUI.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
        isPhoneOpen = false;
        Time.timeScale = 1f;
    }
}
