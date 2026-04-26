using UnityEngine;
using TMPro;


public class PhoneManager : MonoBehaviour
{
    public GameObject phoneUI; // Obiectul PhoneUI (întregul widget)
    public Transform messageParent; // Content din Scroll View
    public GameObject messageBubblePrefab; // Prefab-ul cu bulă + text

    void Start()
    {
        phoneUI.SetActive(false);
    }

    // Functie pentru a deschide/ inchide telefonul
    public void TogglePhone()
    {
        bool isNowOpen = !phoneUI.activeSelf;
        phoneUI.SetActive(isNowOpen);

        // 🔒 Blochează controlul în Adventure Creator
        if (isNowOpen)
        {
            AC.KickStarter.playerInput.enabled = false; // dezactivează input
            AC.KickStarter.stateHandler.gameState = AC.GameState.Paused; // oprește AC (nu mai merge click, dialog etc)
        }
        else
        {
            AC.KickStarter.playerInput.enabled = true; // reactivăm inputul
            AC.KickStarter.stateHandler.gameState = AC.GameState.Normal; // jocul redevine activ
        }
    }



    public void ReceiveMessage(string messageText)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        bubble.GetComponentInChildren<TextMeshProUGUI>().text = messageText;

    }
}
