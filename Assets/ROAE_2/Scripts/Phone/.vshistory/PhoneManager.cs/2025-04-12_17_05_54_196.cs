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

        ShowConversations();

    }

    public void TogglePhone()
    {
        isOpen = !isOpen;
        phoneUI.SetActive(isOpen);

        // 🧲 Dezactivează collider-ele
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
        // Adaugă mesajul într-o conversație existentă sau nouă
        PhoneConversation convo = conversations.Find(c => c.contactName == message.sender);
        if (convo == null)
        {
            convo = new PhoneConversation(message.sender);
            conversations.Add(convo);
        }
        convo.messages.Add(message);

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
        // 🧹 Fă o copie a elementelor din container și distruge-le fără să modifici direct în timp ce iterezi
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in conversationListParent)
            toDestroy.Add(child.gameObject);
        foreach (var go in toDestroy)
            Destroy(go);

        // 🧩 Adaugă fiecare conversație ca buton
        foreach (var convo in conversations)
        {
            GameObject button = Instantiate(conversationButtonPrefab, conversationListParent);
            button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = convo.contactName;

            // 🔐 Captură corectă — fără referințe dubioase care dau crash
            PhoneConversation capturedConvo = convo;
            button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                ShowConversationMessages(capturedConvo);

                // 🔄 Schimbă vizual între ecranul cu conversații și cel cu mesaje
                conversationListParent.gameObject.SetActive(false);
                messageParent.gameObject.SetActive(true);
            });
        }

        // 🟢 Activează lista de conversații și ascunde mesajele
        conversationListParent.gameObject.SetActive(true);
        messageParent.gameObject.SetActive(false);
    }



    public void ShowConversationMessages(PhoneConversation convo)
    {
        // 🔄 curăță bulele de mesaje
        for (int i = messageParent.childCount - 1; i >= 0; i--)
            Destroy(messageParent.GetChild(i).gameObject);

        // 📨 adaugă toate mesajele din conversație
        foreach (var msg in convo.messages)
            ReceiveMessage(msg);
    }

}
