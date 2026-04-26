using UnityEngine;

public class PhoneUIFlow : MonoBehaviour
{
    [SerializeField] private GameObject interfataVizuala;
    [SerializeField] private GameObject phoneButton_Open;
    [SerializeField] private GameObject phoneButton_Close;

    private void Start()
    {
        Debug.Log("📱 PhoneUIFlow START");

        interfataVizuala.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);

        Debug.Log("✅ Telefonul e ascuns, dar butonul de deschidere e vizibil.");
    }

    public void OnPhoneButtonPressed()
    {
        interfataVizuala.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);

        Debug.Log("📞 Telefon DESCHIS");
    }

    public void OnCloseButtonPressed()
    {
        interfataVizuala.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);

        Debug.Log("📞 Telefon ÎNCHIS");
    }
}
