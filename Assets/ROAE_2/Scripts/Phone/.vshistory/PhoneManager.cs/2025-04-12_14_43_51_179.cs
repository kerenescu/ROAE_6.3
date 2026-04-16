using UnityEngine;
using TMPro;

public class PhoneManager : MonoBehaviour
{
    public GameObject phoneUI;                // Obiectul complet cu telefonul
    public Transform messageParent;           // Content din Scroll View
    public GameObject messageBubblePrefab;    // Prefab mesaj
    public GameObject uiBlocker;              // UI blocker transparent

    private bool isOpen = false;

    void Start()
    {
        phoneUI.SetActive(false);
        uiBlocker.SetActive(false);
    }

    public void TogglePhone()
    {
        isOpen = !isOpen;
        phoneUI.SetActive(isOpen);
        uiBlocker.SetActive(isOpen);
    }

    public void ReceiveMessage(string messageText)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        bubble.GetComponentInChildren<TextMeshProUGUI>().text = messageText;
    }
}
