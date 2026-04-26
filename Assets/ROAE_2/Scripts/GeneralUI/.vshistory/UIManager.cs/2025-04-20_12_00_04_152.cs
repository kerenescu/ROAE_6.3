// === UIManager.cs ===
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject phoneUI;
    public GameObject journalUI;
    public GameObject statsUI;

    [Header("Open Buttons")]
    public GameObject phoneButtonOpen;
    public GameObject journalButtonOpen;
    public GameObject statsButtonOpen;

    [Header("Optional")]
    public GameObject uiBlocker; // panel opac sau transparent

    private bool isAnyUIOpen = false;

    private void Start()
    {
        CloseAllUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) TogglePhone();
        if (Input.GetKeyDown(KeyCode.J)) ToggleJournal();
        if (Input.GetKeyDown(KeyCode.S)) ToggleStats();
    }

    public void TogglePhone()
    {
        if (phoneUI.activeSelf) ClosePhone();
        else OpenPhone();
    }

    public void ToggleJournal()
    {
        if (journalUI.activeSelf) CloseJournal();
        else OpenJournal();
    }

    public void ToggleStats()
    {
        if (statsUI.activeSelf) CloseStats();
        else OpenStats();
    }

    public void OpenPhone()
    {
        CloseAllUI();
        phoneUI.SetActive(true);
        phoneButtonOpen.SetActive(false);
        UpdateState(true);
    }

    public void OpenJournal()
    {
        CloseAllUI();
        journalUI.SetActive(true);
        journalButtonOpen.SetActive(false);
        UpdateState(true);
    }

    public void OpenStats()
    {
        CloseAllUI();
        statsUI.SetActive(true);
        statsButtonOpen.SetActive(false);
        UpdateState(true);
    }

    public void ClosePhone()
    {
        phoneUI.SetActive(false);
        phoneButtonOpen.SetActive(true);
        UpdateState(false);
    }

    public void CloseJournal()
    {
        journalUI.SetActive(false);
        journalButtonOpen.SetActive(true);
        UpdateState(false);
    }

    public void CloseStats()
    {
        statsUI.SetActive(false);
        statsButtonOpen.SetActive(true);
        UpdateState(false);
    }

    private void CloseAllUI()
    {
        phoneUI.SetActive(false);
        journalUI.SetActive(false);
        statsUI.SetActive(false);

        phoneButtonOpen.SetActive(true);
        journalButtonOpen.SetActive(true);
        statsButtonOpen.SetActive(true);

        UpdateState(false);
    }

    private void UpdateState(bool uiOpened)
    {
        isAnyUIOpen = uiOpened;
        Time.timeScale = uiOpened ? 0f : 1f;
        if (uiBlocker != null)
            uiBlocker.SetActive(uiOpened);
    }
}
