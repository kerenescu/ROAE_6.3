using UnityEngine;

public class PhoneUIFlow : MonoBehaviour
{
    [SerializeField] private GameObject interfataVizuala;
    [SerializeField] private GameObject phoneButton_Open;
    [SerializeField] private GameObject phoneButton_Close;
    [SerializeField] private MessageManager messageManager;

    private void Start()
    {
        interfataVizuala.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
    }

    public void OnPhoneButtonPressed()
    {
        interfataVizuala.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);
    }

    public void OnCloseButtonPressed()
    {
        interfataVizuala.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
    }
}
