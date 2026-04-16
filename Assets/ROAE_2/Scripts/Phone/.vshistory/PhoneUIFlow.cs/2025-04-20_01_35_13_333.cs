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
        ClosePhone();
    }

    public void OnPhoneButtonPressed()
    {
        OpenPhone();
    }

    public void OnCloseButtonPressed()
    {
        ClosePhone();
    }

    private void OpenPhone()
    {
        phoneUI.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);
        isPhoneOpen = true;

        Time.timeScale = 0f; // Oprește jocul
    }

    private void ClosePhone()
    {
        phoneUI.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
        isPhoneOpen = false;

        Time.timeScale = 1f; // Reia jocul
    }
}
