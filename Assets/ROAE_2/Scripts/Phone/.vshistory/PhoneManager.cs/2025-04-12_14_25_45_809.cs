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
            // Blochează complet controlul jucătorului
            AC.KickStarter.playerInput.enabled = false;
            AC.KickStarter.playerInteraction.DisableHotspots();
            AC.KickStarter.playerMenus.ClearAllLocks(); // opțional
        }
        else
        {
            // Repornește controlul jucătorului
            AC.KickStarter.playerInput.enabled = true;
            AC.KickStarter.playerInteraction.EnableHotspots();
        }
    }






    public void ReceiveMessage(string messageText)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        bubble.GetComponentInChildren<TextMeshProUGUI>().text = messageText;

    }
}
