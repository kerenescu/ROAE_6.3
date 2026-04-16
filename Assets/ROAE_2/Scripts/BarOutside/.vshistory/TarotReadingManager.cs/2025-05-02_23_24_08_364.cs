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

    private float timeSinceFinalLine = 0f;
    private bool readyToClose = false;

    private List<GameObject> selectedCards = new List<GameObject>();
    private int revealedCardsCount = 0;
    private const int maxCards = 3;

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
        deckButton.gameObject.SetActive(true);
        madameText.text = introText;
        revealedCardsCount = 0;
        selectedCards.Clear();
    }

    private List<int> usedIndexes = new List<int>();




    private IEnumerator DelayAndAllowReveal()
    {
        yield return new WaitForSeconds(0.5f);
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
        tarotPanelUI.SetActive(false);
        Time.timeScale = 1f; // dacă ai blocat jocul în timpul citirii
        Debug.Log("🔮 Tarot reading complete. Returning to gameplay.");
    }
}
