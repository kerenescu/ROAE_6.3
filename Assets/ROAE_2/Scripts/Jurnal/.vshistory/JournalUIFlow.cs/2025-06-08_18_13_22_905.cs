// 🎯 JournalUIFlow.cs – Control complet de jurnal: buton → imagine închisă → imagine deschisă + săgeți
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JournalUIFlow : MonoBehaviour
{
    
    [SerializeField]
    private GameObject closedJournalImage;       // Imaginea cu jurnalul închis
    [SerializeField]
    private GameObject openJournalImage;         // Imaginea cu jurnalul deschis
    [SerializeField]
    private GameObject navigationButtons;        // Container cu butoanele ← →
    [SerializeField]
    private GameObject journalButton;            // Butonul vizibil în gameplay
    [SerializeField]
    private GameObject closeButton;              // Butonul de închidere

    [Header("Journal Content References")]
    public JournalPageData currentPage;

    public TextMeshProUGUI thoughtsText;
    public Toggle[] taskToggles;
    public TextMeshProUGUI[] taskDescriptions;
    public Image[] collectibleImages;

    void Start()
    {
        // La start, doar butonul de jurnal este activ
        journalButton.SetActive(true);
        closedJournalImage.SetActive(false);
        openJournalImage.SetActive(false);
        navigationButtons.SetActive(false);
        closeButton.SetActive(false);

    }

    // Apelat din butonul "Journal"
    public void OnJournalButtonPressed()
    {
        Time.timeScale = 0f;

        Debug.Log("Open Journal Button pressed");

        journalButton.SetActive(false);
        closedJournalImage.SetActive(true);
        closeButton.SetActive(false);
        navigationButtons.SetActive(false);
    }

    // Apelat din OnClick de pe imaginea jurnalului închis
    public void OnClosedJournalClicked()
    {

       closedJournalImage.SetActive(false);
        openJournalImage.SetActive(true);
        navigationButtons.SetActive(true);
        closeButton.SetActive(true);
        DisplayJournalContent(); 
    }

    // Inchid de tot jurnalu
    public void CloseJournalCompletely()
    {
        Time.timeScale = 1f;

        Debug.Log("CLOSE JOURNAL TRIGGERED");

        closedJournalImage.SetActive(false);
        openJournalImage.SetActive(false);
        navigationButtons.SetActive(false);
        journalButton.SetActive(true);
        closeButton.SetActive(false);

        if (CaptionUIManager.Instance != null)
        {
            CaptionUIManager.Instance.ForceHideCaption();  // Ascunde gândul instant
        }

    }

    // Nouă metodă de afișare a conținutului
    public void DisplayJournalContent()
    {
        if (currentPage == null)
        {
            Debug.LogWarning("Lipsește JournalPageData!");
            return;
        }

        // Thoughts
        thoughtsText.text = string.Join("\n\n", currentPage.thoughts);

        // Tasks
        for (int i = 0; i < taskToggles.Length; i++)
        {
            if (i < currentPage.tasks.Count)
            {
                taskDescriptions[i].text = currentPage.tasks[i].taskDescription;
                taskToggles[i].isOn = currentPage.tasks[i].isCompleted;
                taskToggles[i].gameObject.SetActive(true);
            }
            else
            {
                taskToggles[i].gameObject.SetActive(false);
            }
        }

        // Collectibles
        for (int i = 0; i < collectibleImages.Length; i++)
        {
            if (i < currentPage.collectibles.Count && currentPage.collectibles[i] != null)
            {
                collectibleImages[i].sprite = currentPage.collectibles[i];
                collectibleImages[i].color = Color.white; // găsit
            }
            else
            {
                collectibleImages[i].color = Color.gray; // negăsit
            }
        }
    }
    DisplayJournalContent(); // ← ADĂUGĂ ASTA AICI
}
