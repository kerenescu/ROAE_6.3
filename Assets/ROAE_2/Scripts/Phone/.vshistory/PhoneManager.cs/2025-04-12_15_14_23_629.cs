using UnityEngine;
using TMPro;

public class PhoneManager : MonoBehaviour
{
    public GameObject phoneUI;
    public Transform messageParent;
    public GameObject messageBubblePrefab;

    private bool isOpen = false;
    private Collider2D[] allColliders;
    private MonoBehaviour[] inputScripts;

    void Start()
    {
        phoneUI.SetActive(false);

        // Găsește toate collider-ele din scenă (obiectele pe care le poți clickui)
        allColliders = FindObjectsOfType<Collider2D>();

        // Găsește toate scripturile de input dacă ai un tag special pe ele (opțional)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            inputScripts = player.GetComponents<MonoBehaviour>();
        }
    }

    public void TogglePhone()
    {
        isOpen = !isOpen;
        phoneUI.SetActive(isOpen);

        // 🧊 Dezactivează collider-ele
        foreach (Collider2D col in allColliders)
        {
            col.enabled = !isOpen;
        }

        // ⛔ Dezactivează toate scripturile de input din Player (opțional)
        if (inputScripts != null)
        {
            foreach (MonoBehaviour script in inputScripts)
            {
                // Dacă e gen PlayerMovement sau alt script scris de tine
                if (script != this) // Nu dezactiva PhoneManager 😅
                    script.enabled = !isOpen;
            }
        }

        // 🔄 Oprește/pornește timpul jocului (pentru animații, particule etc.)
        Time.timeScale = isOpen ? 0f : 1f;
    }

    public void ReceiveMessage(string messageText)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        bubble.GetComponentInChildren<TextMeshProUGUI>().text = messageText;
    }
}
