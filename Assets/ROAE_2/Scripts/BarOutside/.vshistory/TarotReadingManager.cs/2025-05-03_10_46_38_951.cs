using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TarotReadingManager : MonoBehaviour
{
    public static TarotReadingManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject tarotPanelUI;
    [SerializeField] private Button deckButton;
    [SerializeField] private GameObject[] cardPrefabs;
    private List<int> usedIndexes = new List<int>();

    [SerializeField] private Transform cardSpawnParent;
    [SerializeField] private TextMeshProUGUI madameText;

    [Header("Settings")]
    [SerializeField] private string introText = "Alege 3 cărți.";
    [SerializeField] private string finalText = "Hmm... interesant. Asta e tot ce-ți pot spune.";

    [Header("Spce hint")]
    [SerializeField] private GameObject spaceHintText;
    [SerializeField] private float hintDelaySeconds = 2f;

    [SerializeField] private List<Transform> revealPositions; /* pozitiile tinta ale cartilor */

    private float timeSinceFinalLine = 0f;
    private bool readyToClose = false;

    private List<GameObject> selectedCards = new List<GameObject>();
    private int revealedCardsCount = 0;
    private const int maxCards = 3;

    [SerializeField] private bool isRepeatReading = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        tarotPanelUI.SetActive(false);
    }

    public void SetMadameText(string text)
    {
        madameText.text = text;
    }


    public void StartReading()
    {
        tarotPanelUI.SetActive(true);
        madameText.text = introText;

        revealedCardsCount = 0;
        selectedCards.Clear();
        usedIndexes.Clear();

        if (isRepeatReading)
        {
            // Direct afișează 3 cărți cu fața în sus
            for (int i = 0; i < maxCards; i++)
            {
                int index;
                do
                {
                    index = Random.Range(0, cardPrefabs.Length);
                } while (usedIndexes.Contains(index));
                usedIndexes.Add(index);

                GameObject card = Instantiate(cardPrefabs[index], cardSpawnParent);
                selectedCards.Add(card);

                // Arată direct fața, ascunde spatele
                var tarotCard = card.GetComponent<TarotCard>();
                tarotCard.ShowFrontInstant();
            }

            StartCoroutine(ShowFinalLine()); // sau mesaj scurt
        }
        else
        {
            deckButton.gameObject.SetActive(true);
        }
    }


    public void OnDeckClick()
    {
        if (selectedCards.Count >= maxCards || usedIndexes.Count >= cardPrefabs.Length)
            return;

        int index;
        do
        {
            index = Random.Range(0, cardPrefabs.Length);
        } while (usedIndexes.Contains(index));

        usedIndexes.Add(index);

        GameObject card = Instantiate(cardPrefabs[index], cardSpawnParent);
        selectedCards.Add(card);

        // 🔥 Adaugăm listener pentru click pe spate
        Button backButton = card.transform.Find("Back")?.GetComponent<Button>();
        TarotCard tarotCard = card.GetComponent<TarotCard>();

        if (backButton != null && tarotCard != null)
            backButton.onClick.AddListener(tarotCard.OnClick);

        if (selectedCards.Count == maxCards)
        {
            deckButton.gameObject.SetActive(false);
            StartCoroutine(DelayAndAllowReveal());
        }
    }





    private IEnumerator DelayAndAllowReveal()
    {
        yield return new WaitForSeconds(0.5f);

        // Repoziționează cele 3 cărți
        for (int i = 0; i < selectedCards.Count && i < revealPositions.Count; i++)
        {
            selectedCards[i].transform.SetParent(revealPositions[i]);
            selectedCards[i].transform.localPosition = Vector3.zero;
            selectedCards[i].transform.localScale = Vector3.one;
        }

        madameText.text = "Apasă pe fiecare carte pentru a o întoarce.";
    }


    private void Update()
    {
        if (!readyToClose) return;

        timeSinceFinalLine += Time.unscaledDeltaTime;

        if (timeSinceFinalLine >= hintDelaySeconds && !spaceHintText.activeSelf)
        {
            spaceHintText.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            spaceHintText.SetActive(false);
            CloseReading();
        }
    }


    public void NotifyCardRevealed()
    {
        revealedCardsCount++;

        if (revealedCardsCount == maxCards)
        {
            StartCoroutine(ShowFinalLine());
        }
    }

    private IEnumerator ShowFinalLine()
    {
        yield return new WaitForSeconds(1.2f);
        madameText.text = finalText;
        readyToClose = true;
        timeSinceFinalLine = 0f;
        spaceHintText.SetActive(false);

    }

    private void CloseReading()
    {
        // Distrugem toate cărțile de pe ecran
        foreach (GameObject card in selectedCards)
        {
            Destroy(card);
        }
        selectedCards.Clear();

        tarotPanelUI.SetActive(false);
        spaceHintText.SetActive(false);
        Time.timeScale = 1f;
        readyToClose = false;
        Debug.Log("🔮 Tarot reading complete. Returning to gameplay.");
    }


    public void StartRepeatReading()
    {
        isRepeatReading = true;
        StartReading();
    }

}
