// === PhoneUIFlow.cs ===
using UnityEngine;

public class PhoneUIFlow : MonoBehaviour
{
    public GameObject phoneUI;
    public GameObject phoneButton_Open;
    public GameObject phoneButton_Close;

    private void Start()
    {
        ValidateRefs();
        SafeClosePhone();
    }

    public void OnPhoneButtonPressed() => SafeOpenPhone();
    public void OnCloseButtonPressed() => SafeClosePhone();

    private void SafeOpenPhone()
    {
        if (!ValidateRefs()) return;
        phoneUI.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);
        Time.timeScale = 0f;
    }

    private void SafeClosePhone()
    {
        if (!ValidateRefs()) return;
        phoneUI.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
        Time.timeScale = 1f;
    }

    private bool ValidateRefs()
    {
        if (phoneUI == null || phoneButton_Open == null || phoneButton_Close == null)
        {
            Debug.LogError("❌ PhoneUIFlow: Referinte lipsa la UI!");
            return false;
        }
        return true;
    }
}
