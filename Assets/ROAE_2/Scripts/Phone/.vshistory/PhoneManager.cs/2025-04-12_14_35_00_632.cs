using UnityEngine;
using TMPro;


public class PhoneManager : MonoBehaviour
{
    public GameObject phoneUI; // Obiectul PhoneUI (întregul widget)
    public Transform messageParent; // Content din Scroll View
    public GameObject messageBubblePrefab; // Prefab-ul cu bulă + text
    public GameObject uiBlocker;

    void Start()
    {
        phoneUI.SetActive(false);
    }

    // Functie pentru a deschide/ inchide telefonul
    public void TogglePhone()
    {
        bool isNowOpen = !phoneUI.activeSelf;
        phoneUI.SetActive(isNowOpen);
        uiBlocker.SetActive(isNowOpen); // 🛡 blochează click-urile

        Time.timeScale = isNowOpen ? 0f : 1f;

        if (isNowOpen)
        {
            Time.timeScale = 0f;
            uiBlocker.SetActive(true);

            // Blochează meniurile și interacțiunile AC
            AC.KickStarter.playerMenus.SetGameplayBlocked(true);
        }
        else
        {
            Time.timeScale = 1f;
            uiBlocker.SetActive(false);

            // Deblochează interacțiunile AC
            AC.KickStarter.playerMenus.SetGameplayBlocked(false);
        }

    }










    public void ReceiveMessage(string messageText)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        bubble.GetComponentInChildren<TextMeshProUGUI>().text = messageText;

    }
}
