using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PhoneManager : MonoBehaviour
{
    public GameObject phoneUI;
    public Transform messageParent;
    public GameObject messageBubblePrefab;

    public GameObject scrollViewMessages;
    public GameObject scrollViewConversations;
    public GameObject backButton;

    public List<PhoneConversation> conversations = new List<PhoneConversation>();
    public Transform conversationListParent;
    public GameObject conversationButtonPrefab;

    private bool isOpen = false;
    private Collider2D[] allColliders;
    private MonoBehaviour[] inputScripts;

    void Start()
    {
        phoneUI.SetActive(false);

        allColliders = FindObjectsOfType<Collider2D>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            inputScripts = player.GetComponents<MonoBehaviour>();
        }

        ShowConversations();

        // 👈 Adăugăm eveniment pentru butonul de întoarcere
        if (backButton != null)
        {
            backButton.GetComponent<Button>().onClick.AddListener(() => {
                ShowConversations();
            });
        }
    }

    public void TogglePhone()
    {
        isOpen = !isOpen;
        phoneUI.SetActive(isOpen);

        foreach (Collider2D col in allColliders)
        {
            col.enabled = !isOpen;
        }

        if (inputScripts != null)
        {
            foreach (MonoBehaviour script in inputScripts)
            {
                if (script != this)
                    script.enabled = !isOpen;
            }
        }

        Time.timeScale = isOpen ? 0f : 1f;
    }

    void ConfigureBubble(GameObject bubble, PhoneMessage msg)
    {
        bool isFromRina = msg.sender == "Rina";

        // 💬 Setează textul
        var texts = bubble.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 2)
        {
            texts[0].text = msg.sender;
            texts[1].text = msg.content;
            texts[0].alignment = isFromRina ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;
            texts[1].alignment = isFromRina ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;
        }

        // 🎨 Stil mesaj
        bubble.GetComponent<Image>().color = msg.isOldConversation ?
            new Color(0.9f, 0.9f, 0.9f, 0.7f) : Color.white;

        // ↔️ Aliniere stânga/dreapta
        RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
        bubbleRect.anchorMin = bubbleRect.anchorMax = new Vector2(isFromRina ? 1 : 0, 1);
        bubbleRect.pivot = new Vector2(isFromRina ? 1 : 0, 1);
        bubbleRect.anchoredPosition = Vector2.zero;
    }



    public void ReceiveMessage(PhoneMessage message)
    {
        PhoneConversation convo = conversations.Find(c => c.contactName == message.sender);
        if (convo == null)
        {
            convo = new PhoneConversation(message.sender);
            conversations.Add(convo);
        }
        convo.messages.Add(message);

        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        ConfigureBubble(bubble, message); // ✅ configurare unică
    }

    public void ShowConversations()
    {
        for (int i = conversationListParent.childCount - 1; i >= 0; i--)
        {
            Destroy(conversationListParent.GetChild(i).gameObject);
        }

        // Folosește o copie pentru a evita erori de modificare în timp ce iterezi
        var conversationsCopy = new List<PhoneConversation>(conversations);

        foreach (var convo in conversationsCopy)
        {
            GameObject button = Instantiate(conversationButtonPrefab, conversationListParent);
            button.GetComponentInChildren<TextMeshProUGUI>().text = convo.contactName;

            PhoneConversation currentConvo = convo;
            button.GetComponent<Button>().onClick.AddListener(() => {
                ShowConversationMessages(currentConvo);
            });
        }

        // Vizibilitate
        scrollViewMessages.SetActive(false);
        scrollViewConversations.SetActive(true);
        backButton.SetActive(false);
    }

    public void ShowConversationMessages(PhoneConversation convo)
    {
        for (int i = messageParent.childCount - 1; i >= 0; i--)
        {
            Destroy(messageParent.GetChild(i).gameObject);
        }

        var messagesCopy = new List<PhoneMessage>(convo.messages);

        foreach (var msg in messagesCopy)
        {
            GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
            ConfigureBubble(bubble, msg); // ✅ la fel și aici
        }

        scrollViewConversations.SetActive(false);
        scrollViewMessages.SetActive(true);
        backButton.SetActive(true);
    }

}
