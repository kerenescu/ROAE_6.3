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
        // curăță lista de butoane vechi
        for (int i = conversationListParent.childCount - 1; i >= 0; i--)
        {
            Destroy(conversationListParent.GetChild(i).gameObject);
        }

        // facem o copie a listei ca să evităm "Collection was modified"
        var conversationsCopy = new List<PhoneConversation>(conversations);

        foreach (var convo in conversationsCopy)
        {
            GameObject button = Instantiate(conversationButtonPrefab, conversationListParent);
            button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = convo.contactName;

            // 🧠 Captăm valoarea într-o variabilă locală, altfel closure-ul referă ultima valoare
            PhoneConversation currentConvo = convo;

            button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
                ShowConversationMessages(currentConvo);
            });
        }

        // activăm vizual lista de conversații
        conversationListParent.gameObject.SetActive(true);
        messageParent.gameObject.SetActive(false);
    }




    public void ShowConversationMessages(PhoneConversation convo)
    {
        // 🔄 curăță bulele vechi fără foreach
        for (int i = messageParent.childCount - 1; i >= 0; i--)
        {
            Destroy(messageParent.GetChild(i).gameObject);
        }

        // 🔒 folosește for în loc de foreach (SAFE!)
        for (int i = 0; i < convo.messages.Count; i++)
        {
            ReceiveMessage(convo.messages[i]);
        }

        // 👁‍🗨 Activăm zona de mesaje
        messageParent.gameObject.SetActive(true);
        conversationListParent.gameObject.SetActive(false);
    }


}
