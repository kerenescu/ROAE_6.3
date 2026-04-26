using UnityEngine;
using TMPro;

public class PhoneManager : MonoBehaviour
{
    public GameObject phoneUI;                // Obiectul complet cu telefonul
    public Transform messageParent;           // Content din Scroll View
    public GameObject messageBubblePrefab;    // Prefab mesaj


    private bool isOpen = false;
    private GameObject player;

    void Start()
    {
        phoneUI.SetActive(false);

        player = GameObject.FindGameObjectWithTag("Player"); // caută playerul
    }

    public void TogglePhone()
    {
        isOpen = !isOpen;
        phoneUI.SetActive(isOpen);


        // 🧠 Dezactivează playerul (sau componenta de input)
        if (player != null)
        {
            player.SetActive(!isOpen); // opțional: dezactivează complet obiectul player
            // Sau: player.GetComponent<PlayerMovement>().enabled = !isOpen;  <- dacă ai un script separat de control
        }
    }

    public void ReceiveMessage(string messageText)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        bubble.GetComponentInChildren<TextMeshProUGUI>().text = messageText;
    }
}
