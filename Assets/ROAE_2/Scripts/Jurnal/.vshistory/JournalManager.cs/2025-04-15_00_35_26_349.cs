using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class JournalManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject journalUI;
    public GameObject thoughtsPage;
    public GameObject flowersPage;
    public GameObject tasksPage;
    public GameObject navigationButtons;
    public GameObject closedJournalImage; // adaugă la început
    public GameObject openJournalImage; // adaugă la început

    public TextMeshProUGUI thoughtsText;
    public Transform flowersGrid;
    public GameObject flowerPrefab;
    public TextMeshProUGUI tasksText;

    [Header("Data")]
    public List<JournalEntry> thoughtEntries;
    public List<PressedFlower> collectedFlowers;
    public List<string> taskNotes;
    public List<JournalPageData> pages;

    [Header("Audio")]
    public AudioSource pageFlipAudio;

    private int currentPageIndex = 0;
    private bool isOpen = false;

    private Collider2D[] allColliders;
    private MonoBehaviour[] inputScripts;

    private void Start()
    {
        journalUI.SetActive(false);
        if (navigationButtons != null)
            navigationButtons.SetActive(false);

        allColliders = FindObjectsOfType<Collider2D>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            inputScripts = player.GetComponents<MonoBehaviour>();
    }

    public void ToggleJournal()
    {
        if (closedJournalImage != null)
            closedJournalImage.SetActive(true);

        if (isOpen)
            CloseJournal();
        else
            OpenJournal();
    }

    public void OpenJournal()
    {
        if (closedJournalImage != null)
            closedJournalImage.SetActive(false); // 🔥 ASCUNDE JURNALUL ÎNCHIS

        journalUI.SetActive(true);
        if (navigationButtons != null)
            navigationButtons.SetActive(true);

        ShowPage(0);
        isOpen = true;

        ToggleGameplayInteraction(false);
        Time.timeScale = 0f;
    }


    public void CloseJournal()
    {
        journalUI.SetActive(false);
        if (navigationButtons != null)
            navigationButtons.SetActive(false);

        isOpen = false;

        ToggleGameplayInteraction(true);
        Time.timeScale = 1f;
    }

    private void ToggleGameplayInteraction(bool enable)
    {
        foreach (Collider2D col in allColliders)
            col.enabled = enable;

        if (inputScripts != null)
        {
            foreach (MonoBehaviour script in inputScripts)
            {
                if (script != this)
                    script.enabled = enable;
            }
        }
    }

    public void ShowPage(int index)
    {
        if (index < 0 || index >= pages.Count) return;

        currentPageIndex = index;
        JournalPageData page = pages[index];

        thoughtsPage.SetActive(true);
        flowersPage.SetActive(true);
        tasksPage.SetActive(true);

        thoughtsText.text = string.Join("\n", page.thoughts);
        tasksText.text = string.Join("\n", page.tasks);

        foreach (Transform child in flowersGrid)
            Destroy(child.gameObject);

        foreach (var sprite in page.collectibles)
        {
            GameObject go = Instantiate(flowerPrefab, flowersGrid);
            Image img = go.GetComponent<Image>();
            img.sprite = sprite;
        }

        if (pageFlipAudio != null)
            pageFlipAudio.Play();
    }

    public void NextPage() => ShowPage(currentPageIndex + 1);
    public void PreviousPage() => ShowPage(currentPageIndex - 1);
}
