// JournalManager.cs – gestionează tot jurnalul (gânduri, flori, taskuri)
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

    public TextMeshProUGUI thoughtsText;
    public Transform flowersGrid;
    public GameObject flowerPrefab;
    public TextMeshProUGUI tasksText;

    [Header("Data")]
    public List<JournalEntry> thoughtEntries;
    public List<PressedFlower> collectedFlowers;
    public List<string> taskNotes;

    [Header("Pages")]
    public List<JournalPageData> pages;

    private int currentPageIndex = 0;

    public AudioSource pageFlipAudio;

    private void Start()
    {
        CloseJournal();
    }

    public void OpenJournal()
    {
        journalUI.SetActive(true);
        ShowThoughts();
    }

    public void CloseJournal()
    {
        journalUI.SetActive(false);
    }

    public void ShowThoughts()
    {
        thoughtsPage.SetActive(true);
        flowersPage.SetActive(false);
        tasksPage.SetActive(false);

        thoughtsText.text = "";
        foreach (var entry in thoughtEntries)
        {
            if (entry.unlocked)
                thoughtsText.text += $"<b>{entry.title}</b>\n{entry.body}\n\n";
        }
    }

    public void ShowFlowers()
    {
        thoughtsPage.SetActive(false);
        flowersPage.SetActive(true);
        tasksPage.SetActive(false);

        foreach (Transform child in flowersGrid)
            Destroy(child.gameObject);

        foreach (var flower in collectedFlowers)
        {
            GameObject go = Instantiate(flowerPrefab, flowersGrid);
            Image img = go.GetComponent<Image>();
            img.sprite = flower.sprite;
            img.color = flower.collected ? Color.white : new Color(1, 1, 1, 0.2f);
        }
    }

    public void ShowTasks()
    {
        thoughtsPage.SetActive(false);
        flowersPage.SetActive(false);
        tasksPage.SetActive(true);

        tasksText.text = "";
        foreach (var t in taskNotes)
            tasksText.text += $"• {t}\n";
    }

    [System.Serializable]
    public class JournalPage
    {
        public List<string> thoughts;
        public List<string> tasks;
        public List<Sprite> collectibles;
    }

    public void ShowPage(int index)
    {
        if (index < 0 || index >= pages.Count) return;

        currentPageIndex = index;
        JournalPage page = pages[index];

        // Gânduri
        thoughtsText.text = string.Join("\n", page.thoughts);

        // Taskuri
        tasksText.text = string.Join("\n", page.tasks);

        // Florile/frunzele
        foreach (Transform child in flowersGrid)
            Destroy(child.gameObject);

        foreach (var sprite in page.collectibles)
        {
            GameObject go = Instantiate(flowerPrefab, flowersGrid);
            Image img = go.GetComponent<Image>();
            img.sprite = sprite;
        }

        // Sunet
        if (pageFlipAudio != null)
            pageFlipAudio.Play();
    }

    public void NextPage() => ShowPage(currentPageIndex + 1);
    public void PreviousPage() => ShowPage(currentPageIndex - 1);

}
