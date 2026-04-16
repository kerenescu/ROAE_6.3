using UnityEngine;
using TMPro;

public class PhoneManager : MonoBehaviour
{
    public GameObject phoneUI;
    public GameObject uiBlocker; // panel care acoperă tot ecranul și captează clickuri
    public Transform messageParent;
    public GameObject messageBubblePrefab;

    private bool isOpen = false;

    void Start()
    {
        phoneUI.SetActive(false);
        uiBlocker.SetActive(false);
        Time.timeScale = 1f;
    }

    public void TogglePhone()
    {
        isOpen = !isOpen;
        phoneUI.SetActive(isOpen);
        uiBlocker.SetActive(isOpen);

        // Freeze everything except UI
        if (isOpen)
        {
            Time.timeScale = 0f; // ⏸ oprește mișcarea, anim, timing
        }
        else
        {
            Time.timeScale = 1f; // ▶️ resume everything
        }
    }

    public void ReceiveMessage(string messageText)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        bubble.GetComponentInChildren<TextMeshProUGUI>().text = messageText;
    }
}
