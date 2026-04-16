using UnityEngine;

public class PhoneUIFlow : MonoBehaviour
{
    public GameObject phoneUI;
    public GameObject phoneButtonOpen;
    public GameObject phoneButtonClose;

    public void OnPhoneButtonPressed()
    {
        phoneUI.SetActive(true);
        phoneButtonOpen.SetActive(false);
        phoneButtonClose.SetActive(true);
    }

    public void OnPhoneCloseButtonPressed()
    {
        phoneUI.SetActive(false);
        phoneButtonOpen.SetActive(true);
        phoneButtonClose.SetActive(false);
    }
}
