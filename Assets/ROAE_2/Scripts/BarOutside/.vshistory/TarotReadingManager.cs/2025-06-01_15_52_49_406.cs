using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AC;
using Button = UnityEngine.UI.Button;

public class TarotReadingManager : MonoBehaviour
{
    public static TarotReadingManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject tarotPanelUI;
    [SerializeField] private Button deckButton;
    [SerializeField] private GameObject[] cardPrefabs;
    [SerializeField] private Transform cardSpawnParent;
    [SerializeField] private TextMeshProUGUI madameText;
    [SerializeField] private GameObject spaceHintText;
    [SerializeField] private List<Transform> revealPositions;

    [Header("Text Settings")]
    [SerializeField] private string introText = "Alege 3 cărți.";
    [SerializeField] private string finalText = "Hmm... interesant. Asta e tot ce-ți pot spune.";
    [SerializeField] private GameObject interpretationCanvas;
    [SerializeField] private TextMeshProUGUI interpretationText; // dacă folosești TMP


    [Header("Options")]
    [SerializeField] private float hintDelaySeconds = 2f;
    [SerializeField] private bool isRepeatReading = false;

    private List<GameObject> selectedCards = new();
    private List<int> usedIndexes = new();
    private int revealedCardsCount = 0;
    private const int maxCards = 3;
    private float timeSinceFinalLine = 0f;
    private bool readyToClose = false;
    private bool isWaitingForDismiss = false;

    public static bool IsReadingFrozen { get; set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        tarotPanelUI.SetActive(false);
        spaceHintText.SetActive(false);
    }

    public void SetMadameText(string text)
    {
        madameText.text = text;
    }

    public void StartReading()
    {
        Time.timeScale = 0f;
        // dezactivează toate hotspoturile din scenă
        foreach (AC.Hotspot h in FindObjectsOfType<AC.Hotspot>())
        {
            h.enabled = false; 

        }


        Debug.Log("🔮 Tarot reading started.");
        tarotPanelUI.SetActive(true);
        deckButton.gameObject.SetActive(!isRepeatReading);
        madameText.text = introText;

        selectedCards.Clear();
        usedIndexes.Clear();
        revealedCardsCount = 0;

        if (isRepeatReading)
        {
            for (int i = 0; i < maxCards; i++)
            {
                int index = GetUniqueCardIndex();
                GameObject card = Instantiate(cardPrefabs[index], cardSpawnParent);
                card.GetComponent<TarotCard>().ShowFrontInstant();
                selectedCards.Add(card);
            }

            StartCoroutine(ShowFinalLine());
        }
    }

    public void OnDeckClick()
    {
        if (selectedCards.Count >= maxCards) return;

        int index = GetUniqueCardIndex();
        GameObject card = Instantiate(cardPrefabs[index], cardSpawnParent);
        selectedCards.Add(card);

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

    private int GetUniqueCardIndex()
    {
        int index;
        do index = Random.Range(0, cardPrefabs.Length);
        while (usedIndexes.Contains(index));
        usedIndexes.Add(index);
        return index;
    }

    private IEnumerator DelayAndAllowReveal()
    {
        yield return new WaitForSecondsRealtime(0.7f);


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
        // 🔮 Etapa: așteptăm să închidă interpretarea ultimei cărți
        if (isWaitingForDismiss && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIElement(interpretationCanvas))
            {
                HideInterpretation();
                isWaitingForDismiss = false;
                StartCoroutine(ShowFinalLine());
            }
        }

        // 🔮 Etapa: după replica finală
        if (readyToClose)
        {
            if (!spaceHintText.activeSelf)
                spaceHintText.SetActive(true);

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                CloseReading();
            }
        }
    }


    private bool IsPointerOverUIElement(GameObject targetUI)
    {
        var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            if (result.gameObject == targetUI || result.gameObject.transform.IsChildOf(targetUI.transform))
                return true;
        }

        return false;
    }

    private IEnumerator WaitForLastInterpretationThenFinalLine()
    {
        // Așteaptă până când canvasul cu interpretarea nu mai e activ
        while (interpretationCanvas.activeSelf)
        {
            yield return null; // așteaptă un frame
        }

        // Acum poți arăta replica finală
        yield return new WaitForSecondsRealtime(0.3f); // mic delay extra dacă vrei

        StartCoroutine(ShowFinalLine());
    }


    private IEnumerator ShowFinalLine()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        interpretationCanvas.SetActive(true);
        madameText.text = finalText;

        readyToClose = true;
        spaceHintText.SetActive(false);
        Time.timeScale = 1f;
    }






    private void CloseReading()
    {
        foreach (GameObject card in selectedCards)
            Destroy(card);

        selectedCards.Clear();
        tarotPanelUI.SetActive(false);
        spaceHintText.SetActive(false);
        Time.timeScale = 1f;
        // reactivează toate hotspoturile
        foreach (AC.Hotspot h in FindObjectsOfType<AC.Hotspot>())
        {
            h.enabled = true;

        }

        readyToClose = false;

        Debug.Log("🔮 Tarot reading complete. Returning to gameplay.");
    }
 
    public void StartRepeatReading()
    {
        isRepeatReading = true;
        StartReading();
    }
    public void ShowInterpretation(string text)
    {
        interpretationCanvas.SetActive(true);
        interpretationText.text = text;
    }

    public void HideInterpretation()
    {
        interpretationCanvas.SetActive(false);
    }



}
