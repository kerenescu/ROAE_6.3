using UnityEngine;
using TMPro;

public class PhoneManager : MonoBehaviour
{
    public GameObject phoneUI; // Obiectul PhoneUI (întregul widget)
    public Transform messageParent; // Content din Scroll View
    public GameObject messageBubblePrefab; // Prefab-ul cu bulă + text
    public GameObject uiBlocker; // UI invizibil care blochează clickurile

    void Start()
    {
        phoneUI.SetActive(false);
        uiBlocker.SetActive(false);
    }

    public void TogglePhone()
    {
        bool isNowOpen = !phoneUI.activeSelf;

        phoneUI.SetActive(isNowOpen);
        uiBlocker.SetActive(isNowOpen); // 🛡 blochează click-urile
        Time.timeScale = isNowOpen ? 0f : 1f; // ⏸ oprește tot ce are de-a face cu timp
    }

    public void ReceiveMessage(string messageText)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        bubble.GetComponentInChildren<TextMeshProUGUI>().text = messageText;
    }
}
