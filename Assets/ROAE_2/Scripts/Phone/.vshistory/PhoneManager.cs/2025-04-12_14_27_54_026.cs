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

        if (isNowOpen)
        {
            // Blochează total gameplay-ul și inputul
            AC.KickStarter.stateHandler.SetInGameplay(false);
            AC.KickStarter.playerInput.enabled = false;
        }
        else
        {
            // Repornește controlul
            AC.KickStarter.stateHandler.SetInGameplay(true);
            AC.KickStarter.playerInput.enabled = true;
        }
    }








    public void ReceiveMessage(string messageText)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        bubble.GetComponentInChildren<TextMeshProUGUI>().text = messageText;

    }
}
