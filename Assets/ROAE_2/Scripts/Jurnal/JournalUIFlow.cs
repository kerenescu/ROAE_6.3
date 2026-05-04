// 🎯 JournalUIFlow.cs – Control complet de jurnal: buton → imagine închisă → imagine deschisă + săgeți
using System.Collections.Generic;
using System.Linq;
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
    public AudioSource pageFlipAudioSource;
    public AudioClip pageFlipClip;

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

        // Debug.Log("Open Journal Button pressed");

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

        // Debug.Log("CLOSE JOURNAL TRIGGERED");

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
    public List<JournalPageData> journalPages = new List<JournalPageData>();

    private int currentPageIndex = 0;
    public static JournalUIFlow Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Alpha2))
            return;

        if (IsJournalOpen())
        {
            CloseJournalCompletely();
            return;
        }

        if (PhoneUIFlow.Instance != null && PhoneUIFlow.Instance.IsPhoneOpen())
        {
            PhoneUIFlow.Instance.OnCloseButtonPressed();
        }

        OpenJournal();
    }

    void PlayPageFlipSound()
    {
        if (pageFlipAudioSource != null && pageFlipClip != null)
        {
            pageFlipAudioSource.PlayOneShot(pageFlipClip);
        }
    }

    public bool IsJournalOpen()
    {
        return (closedJournalImage != null && closedJournalImage.activeSelf) ||
               (openJournalImage != null && openJournalImage.activeSelf);
    }

    public void OpenJournal()
    {
        OpenJournalInternal(openToContent: true);
    }

    public void OpenJournalToPage(JournalPageData page)
    {
        if (page != null)
        {
            int pageIndex = journalPages.IndexOf(page);
            if (pageIndex >= 0)
            {
                currentPageIndex = pageIndex;
                currentPage = journalPages[currentPageIndex];
            }
            else
            {
                currentPage = page;
            }
        }

        OpenJournalInternal(openToContent: true);
    }

    private void OpenJournalInternal(bool openToContent)
    {
        if (PhoneUIFlow.Instance != null && PhoneUIFlow.Instance.IsPhoneOpen())
        {
            PhoneUIFlow.Instance.OnCloseButtonPressed();
        }

        Time.timeScale = 0f;

        journalButton.SetActive(false);
        closedJournalImage.SetActive(!openToContent);
        openJournalImage.SetActive(openToContent);
        navigationButtons.SetActive(openToContent);
        closeButton.SetActive(openToContent);

        if (openToContent)
        {
            EnsureValidCurrentPage();
            DisplayJournalContent();
        }
    }

    public void NextPage()
    {
        if (journalPages.Count == 0) return;

        currentPageIndex = (currentPageIndex + 1) % journalPages.Count;
        currentPage = journalPages[currentPageIndex];
        DisplayJournalContent();

        PlayPageFlipSound();
    }

    public void PreviousPage()
    {
        if (journalPages.Count == 0) return;

        currentPageIndex--;
        if (currentPageIndex < 0)
            currentPageIndex = journalPages.Count - 1;

        currentPage = journalPages[currentPageIndex];
        DisplayJournalContent();

        PlayPageFlipSound();
    }


    // Nouă metodă de afișare a conținutului
    public void DisplayJournalContent()
    {
        EnsureValidCurrentPage();

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
    // Metodă de salvat starea task-ului
    public void OnTaskToggleChanged(int taskIndex, bool isCompleted)
    {
        if (currentPage != null && taskIndex < currentPage.tasks.Count)
        {
            currentPage.tasks[taskIndex].isCompleted = isCompleted;
        }
    }


    public void AddPageIfNotPresent(JournalPageData newPage)
    {
        if (!journalPages.Contains(newPage))
        {
            journalPages.Add(newPage);
            currentPageIndex = journalPages.Count - 1;
            currentPage = journalPages[currentPageIndex];
            DisplayJournalContent();

            if (JournalNotificationUI.Instance != null)
            {
                JournalNotificationUI.Instance.ShowPageNotification(newPage);
            }
        }
    }

    private void EnsureValidCurrentPage()
    {
        if (journalPages.Count == 0)
        {
            currentPage = null;
            currentPageIndex = 0;
            return;
        }

        if (currentPage == null || !journalPages.Contains(currentPage))
        {
            currentPageIndex = Mathf.Clamp(currentPageIndex, 0, journalPages.Count - 1);
            currentPage = journalPages[currentPageIndex];
            return;
        }

        currentPageIndex = journalPages.IndexOf(currentPage);
    }





}
