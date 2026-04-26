using UnityEngine;
using TMPro;

public class PhoneManager : MonoBehaviour
{
    public GameObject phoneUI;                // Telefonul complet
    public Transform messageParent;           // Content-ul din Scroll View
    public GameObject messageBubblePrefab;    // Prefab-ul cu textul de mesaj
    public GameObject uiBlocker;              // Blocator transparent care interceptează toate click-urile

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

        // Oprește timpul de joc doar dacă vrei
        // Time.timeScale = isOpen ? 0f : 1f;
    }

    public void ReceiveMessage(string messageText)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        bubble.GetComponentInChildren<TextMeshProUGUI>().text = messageText;
    }
}
