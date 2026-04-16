using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PhoneManager : MonoBehaviour
{
    public GameObject phoneUI;
    public Transform messageParent;
    public GameObject messageBubblePrefab;

    public List<PhoneConversation> conversations = new List<PhoneConversation>();
    public Transform conversationListParent; // un container nou în UI pentru butoane
    public GameObject conversationButtonPrefab; // buton per conversație

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

    public void ReceiveMessage(PhoneMessage message)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);

        TextMeshProUGUI[] texts = bubble.GetComponentsInChildren<TextMeshProUGUI>();

        if (texts.Length >= 2)
        {
            texts[0].text = message.sender;
            texts[1].text = message.content;

            // ✨ Stilizează conversațiile vechi
            if (message.isOldConversation)
            {
                texts[0].fontStyle = FontStyles.Italic;
                texts[1].fontStyle = FontStyles.Italic;

                bubble.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f, 0.7f); // gri pal
            }
            else
            {
                texts[0].fontStyle = FontStyles.Bold;
                texts[1].fontStyle = FontStyles.Normal;

                bubble.GetComponent<Image>().color = Color.white;
            }
        }
    }

    public void ShowConversations()
    {
        // curăță lista
        foreach (Transform child in conversationListParent)
            Destroy(child.gameObject);

        foreach (var convo in conversations)
        {
            GameObject button = Instantiate(conversationButtonPrefab, conversationListParent);
            button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = convo.contactName;

            button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
                ShowConversationMessages(convo);
            });
        }
    }
    public void ShowConversationMessages(PhoneConversation convo)
    {
        // curăță bulele vechi
        foreach (Transform child in messageParent)
            Destroy(child.gameObject);

        foreach (var msg in convo.messages)
            ReceiveMessage(msg); // deja ai metoda asta
    }



}
