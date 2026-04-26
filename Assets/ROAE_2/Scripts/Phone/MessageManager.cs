using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using System.IO;

public class MessageManager : MonoBehaviour
{
    // --------------------------------------
    // UI
    public GameObject phoneUI;
    public Transform messageParent;
    public GameObject messageBubblePrefab;
    // --------------------------------------
    // Audio
    public AudioSource phoneOpenAudio; 
    //public AudioSource phoneCloseAudio;
    //private bool isInitialized = false;
    // --------------------------------------
    // UI pentru mesaje
    public GameObject scrollViewMessages;
    public GameObject scrollViewConversations;
    public GameObject conversationHeader; // HEADER == INBOX
    public GameObject backButton;
    // --------------------------------------
    // UI pentru conversații
    private PhoneConversation currentConversation;  // conversația activă
    public List<PhoneConversation> conversations = new List<PhoneConversation>();
    public Transform conversationListParent;
    public GameObject conversationButtonPrefab;
    // --------------------------------------
    // Notificare
    [HideInInspector] public bool isOpen = false;
    private Collider2D[] allColliders;
    [HideInInspector] public MonoBehaviour[] inputScripts;

    public List<global::DecisionChoice> choices;
    [SerializeField] private GameObject decisionPanel;
    [SerializeField] private GameObject responseButtonPrefab;



    public static List<PhoneConversation> persistentConversations = new List<PhoneConversation>();
    public static MessageManager Instance;



    [System.Serializable]
    public class PhoneData
    {
        public List<PhoneConversationData> conversations;
    }

    [System.Serializable]
    public class PhoneConversationData
    {
        public string contactName;
        public List<PhoneMessageData> messages;
    }

    [System.Serializable]
    public class PhoneMessageData
    {
        public string sender;
        public string text;
        public bool wasRead;
        public bool isDecision;
        public bool wasDecisionTaken;
        public List<global::DecisionChoice> choices;

    }



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            // dacă avem conversații salvate, restaurăm
            if (persistentConversations != null && persistentConversations.Count > 0)
            {
                conversations = new List<PhoneConversation>(persistentConversations);
            }
        }
        else
        {
            Destroy(gameObject); // evită duplicatul
        }
    }



    void Start()
    {
        if (persistentConversations != null && persistentConversations.Count > 0)
        {
            Debug.Log("✅ Conversații deja salvate, le restaurăm!");
            conversations = persistentConversations;
        }
        else
        {
            Debug.Log("📥 Nicio conversație în memorie, încărcăm din JSON...");
            LoadMessagesFromJSON();
            persistentConversations = conversations;
        }

        ShowConversations(); // ✅ doar UI intern
        StartCoroutine(SendDelayedIntroMessage1());
        StartCoroutine(SendDelayedIntroMessage2());
        StartCoroutine(SendDelayedIntroMessage3());

    }





    public void TogglePhone()
    {
        isOpen = !isOpen;
        phoneUI.SetActive(isOpen);

        Collider2D[] currentColliders = FindObjectsOfType<Collider2D>();
        foreach (Collider2D col in currentColliders)
        {
            if (col != null)
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

        if (isOpen)
        {
            ShowConversations();
            if (phoneOpenAudio != null)
            {
                phoneOpenAudio.Play();
            }
        }

        Time.timeScale = isOpen ? 0f : 1f;

        if (!isOpen)
        {
            ShowConversations(); // Reset UI când închidem
        }

    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            persistentConversations = conversations;
        }
    }



    void ConfigureBubble(GameObject bubble, PhoneMessage msg)
    {
        bool isFromRina = msg.sender.Trim().ToLower() == "rina";

        var texts = bubble.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 2)
        {
            texts[0].text = msg.sender;
            texts[1].text = msg.content;

            texts[0].alignment = isFromRina ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;
            texts[1].alignment = isFromRina ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;
        }


        // 🎨 Culoare mesaj
        if (isFromRina)
        {
            bubble.GetComponent<Image>().color = new Color(0.85f, 0.7f, 1f, 1f); // mov pastel (poți schimba valorile)
        }
        else if (msg.isOldConversation)
        {
            bubble.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f, 0.7f); // gri semi-transparent
        }
        else
        {
            bubble.GetComponent<Image>().color = Color.white; // mesaj nou, alb
        }

        // Aliniere pe stânga/dreapta
        RectTransform rect = bubble.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(isFromRina ? 1 : 0, 1);
        rect.pivot = new Vector2(isFromRina ? 1 : 0, 1);

        // mutare ușor în lateral, doar dacă e Rina (spre dreapta)
        rect.anchoredPosition = new Vector2(isFromRina ? -30 : 30, 0);


        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
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

        if (scrollViewConversations.activeSelf)
        {
            ShowConversations(); // reafișează lista dacă e deschisă
        }
        else if (!scrollViewMessages.activeSelf && !phoneUI.activeSelf)
        {
            // Dacă UI-ul nu e deschis, îl putem deschide automat sau trimite o notificare
            Debug.Log("Telefonul e închis, dar am primit un mesaj nou.");
        }


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

        if (conversationHeader != null)
            conversationHeader.SetActive(true); // 🔥 apare titlul
        // Vizibilitate
        scrollViewMessages.SetActive(false);
        scrollViewConversations.SetActive(true);
        backButton.SetActive(false);
        HideDecisionPanel();
    }

    public void ShowConversationMessages(PhoneConversation convo)
    {

        currentConversation = convo;  // ➕ setează conversația activă aici!
        for (int i = messageParent.childCount - 1; i >= 0; i--)
        {
            Destroy(messageParent.GetChild(i).gameObject);
        }

        var messagesCopy = new List<PhoneMessage>(convo.messages);

        foreach (var msg in messagesCopy)
        {
            GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
            ConfigureBubble(bubble, msg);
        }


        scrollViewConversations.SetActive(false);
        scrollViewMessages.SetActive(true);
        backButton.SetActive(true);

        // Replica modulară a Rinei, luată din CaptionThoughtDatabase
        if (CaptionThoughtDatabase.Instance != null && CaptionUIManager.Instance != null)
        {
            string thought = CaptionThoughtDatabase.Instance.GetThought(convo.contactName);
            if (!string.IsNullOrEmpty(thought))
            {
                CaptionUIManager.Instance.ShowCaption(thought);
            }
        }

        // 🔥 Detectăm dacă ultimul mesaj are decizii
        PhoneMessage lastMessage = convo.messages.LastOrDefault();
        if (lastMessage != null)
        {
            Debug.Log($"Ultimul mesaj: {lastMessage.content}");
            Debug.Log($"isDecision: {lastMessage.isDecision}");
            Debug.Log($"choices count: {lastMessage.choices?.Count ?? 0}");
            if (lastMessage.isDecision && !lastMessage.wasDecisionTaken && lastMessage.choices != null)

            {
                decisionPanel.SetActive(true); // Afișăm panelul cu butoane

                // Golesc butoanele anterioare ca sa nu se repete in caz ca nu iau o decizie la prima intrare in conversatie
                foreach (Transform child in decisionPanel.transform)
                {
                    Destroy(child.gameObject);
                }
                // Adaugă butoane pentru fiecare opțiune de răspuns
                foreach (var choice in lastMessage.choices)
                {
                    GameObject buttonGO = Instantiate(responseButtonPrefab, decisionPanel.transform);
                    buttonGO.GetComponentInChildren<TextMeshProUGUI>().text = choice.responseText;

                    buttonGO.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        choice.statsEffect.Apply();                           // Aplici efectul stats
                        AddPlayerResponse(choice.responseText);               // ➕ Adaugi răspunsul lui Rina
                        lastMessage.wasDecisionTaken = true;                  // ➕ Marchezi decizia ca luată
                        SaveMessages();                                       // ➕ Salvezi conversațiile
                        ClearDecisions();                                     // Ascunzi butoanele

                        if (!string.IsNullOrEmpty(choice.followUpContent))
                        {
                            PhoneMessage followUp = new PhoneMessage("Curator", choice.followUpContent);
                            ReceiveMessage(followUp);                         // Trimite follow-up dacă există
                        }
                    });


                }
            }
            else
            {
                decisionPanel.SetActive(false); // Dacă nu e decizie, ascundem panelul
            }
        }
    }

    private void AddPlayerResponse(string responseText)
    {
        // 1. Creează mesajul Rinei
        PhoneMessage playerResponse = new PhoneMessage("Rina", responseText);

        // 2. Adaugă mesajul în conversația activă
        currentConversation.messages.Add(playerResponse);  // ➕!!!!!!!!!!!!

        // 3. Afișează vizual bula mesajului
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        ConfigureBubble(bubble, playerResponse);
    }


    private void ClearDecisions()
    {
        foreach (Transform child in decisionPanel.transform)
        {
            Destroy(child.gameObject);
        }
        decisionPanel.SetActive(false);
    }
    private void LoadMessagesFromJSON()
    {
        string savedPath = Path.Combine(Application.persistentDataPath, "messages_saved.json");
        string defaultPath = Path.Combine(Application.streamingAssetsPath, "messages_start_default.json");

        string json;

        if (File.Exists(savedPath))
        {
            json = File.ReadAllText(savedPath);
            Debug.Log("📥 Încărcăm conversațiile din messages_saved.json");
        }
        else
        {
            json = File.ReadAllText(defaultPath);
            Debug.Log("📥 Încărcăm conversațiile din messages_start_default.json — fallback");
            File.WriteAllText(savedPath, json); // 🆕 copiază defaultul în salvare
            Debug.Log("💾 Copiat defaultul în messages_saved.json pentru sesiuni viitoare.");
        }

        PhoneData data = JsonUtility.FromJson<PhoneData>(json);
        conversations = new List<PhoneConversation>();

        foreach (var convoData in data.conversations)
        {
            PhoneConversation convo = new PhoneConversation(convoData.contactName);
            foreach (var msgData in convoData.messages)
            {
                PhoneMessage msg = new PhoneMessage(msgData.sender, msgData.text, msgData.wasRead);
                msg.isDecision = msgData.isDecision;
                msg.wasDecisionTaken = msgData.wasDecisionTaken;
                msg.choices = msgData.choices;

                convo.messages.Add(msg);
            }
            conversations.Add(convo);
        }
    }



    private void SaveMessages()
    {
        PhoneData data = new PhoneData();
        data.conversations = new List<PhoneConversationData>();

        foreach (var convo in conversations)
        {
            PhoneConversationData convoData = new PhoneConversationData();
            convoData.contactName = convo.contactName;
            convoData.messages = new List<PhoneMessageData>();

            foreach (var msg in convo.messages)
            {
                PhoneMessageData msgData = new PhoneMessageData();
                msgData.sender = msg.sender;
                msgData.text = msg.content;
                msgData.wasRead = msg.wasRead;
                msgData.isDecision = msg.isDecision;
                msgData.wasDecisionTaken = msg.wasDecisionTaken;
                msgData.choices = msg.choices; // ✔ Choices direct

                convoData.messages.Add(msgData);
            }

            data.conversations.Add(convoData);
        }

        string json = JsonUtility.ToJson(data, true);
        string savePath = System.IO.Path.Combine(Application.persistentDataPath, "messages_saved.json");
        System.IO.File.WriteAllText(savePath, json);
        Debug.Log($"💾 Conversațiile au fost salvate în {savePath}");
    }




    private IEnumerator SendDelayedIntroMessage1()
    {
        yield return new WaitForSeconds(7f); // 🕒 delay de 3 secunde

        string displayName = "Număr necunoscut (+40 XXX XXX XXX)";
        string senderReal = "+40 XXX XXX XXX";

        var message = new PhoneMessage(senderReal, "Rina, începe să te miști!");
        ReceiveMessage(message, displayName);

        var notifier = FindObjectOfType<MessageNotifier>();
        if (notifier != null)
        {
            notifier.ShowNotification(displayName);
        }
    }


    private IEnumerator SendDelayedIntroMessage2()
    {
        yield return new WaitForSeconds(15f); // 🕒 delay de 3 secunde

        string displayName = "Număr necunoscut (+40 XXX XXX XXX)";
        string senderReal = "+40 XXX XXX XXX";

        var message = new PhoneMessage(senderReal, "Hai să vedem dacă știi să faci ceva. Repară oglinda!");
        ReceiveMessage(message, displayName);

        var notifier = FindObjectOfType<MessageNotifier>();
        if (notifier != null)
        {
            notifier.ShowNotification(displayName);
        }
    }


    private IEnumerator SendDelayedIntroMessage3()
    {
        yield return new WaitForSeconds(60f); // 🕒 delay de 3 secunde

        string displayName = "Număr necunoscut (+40 XXX XXX XXX)";
        string senderReal = "+40 XXX XXX XXX";

        var message = new PhoneMessage(senderReal, "Apasă pe ciuperci... 3 de-un fel...");
        ReceiveMessage(message, displayName);

        var notifier = FindObjectOfType<MessageNotifier>();
        if (notifier != null)
        {
            notifier.ShowNotification(displayName);
        }
    }

    public void ReceiveMessage(PhoneMessage message, string displayNameOverride = null)
    {
        string contact = displayNameOverride ?? message.sender;

        // Găsește conversația
        var convo = conversations.FirstOrDefault(c => c.contactName == contact);
        if (convo == null)
        {
            convo = new PhoneConversation(contact);
            conversations.Add(convo);
        }

        convo.messages.Add(message);
    }

    public void HideDecisionPanel()
    {
        decisionPanel.SetActive(false);
    }

}
