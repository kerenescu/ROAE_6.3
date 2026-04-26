using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class PhoneMessage
{
    public string sender;                 // Nume expeditor
    [TextArea] public string content;     // Conținutul mesajului
    public bool isOldConversation;        // Este conversație veche?
    public bool hasChoices;               // Are opțiuni de răspuns?
    public List<string> responseOptions;  // Răspunsuri posibile

    // Constructor standard
    public PhoneMessage(string sender, string content, bool isOld = false)
    {
        this.sender = sender;
        this.content = content;
        this.isOldConversation = isOld;
        this.hasChoices = false;
        this.responseOptions = new List<string>();
    }

    // Constructor cu multiple choice
    public PhoneMessage(string sender, string content, List<string> responseOptions)
    {
        this.sender = sender;
        this.content = content;
        this.isOldConversation = false;
        this.hasChoices = true;
        this.responseOptions = responseOptions ?? new List<string>();
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

}
