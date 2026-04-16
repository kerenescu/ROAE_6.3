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
            AC.KickStarter.playerInput.activeArrows = false;
            AC.KickStarter.playerInput.canClick = false;
            AC.KickStarter.playerInput.canMove = false;
            AC.KickStarter.playerInteraction.DisableHotspots();

            // Dezactivează și input general
            AC.KickStarter.playerInput.enabled = false;
        }
        else
        {
            AC.KickStarter.playerInput.activeArrows = true;
            AC.KickStarter.playerInput.canClick = true;
            AC.KickStarter.playerInput.canMove = true;
            AC.KickStarter.playerInteraction.EnableHotspots();

            // Reactivăm input
            AC.KickStarter.playerInput.enabled = true;
        }
    }





    public void ReceiveMessage(string messageText)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        bubble.GetComponentInChildren<TextMeshProUGUI>().text = messageText;

    }
}
