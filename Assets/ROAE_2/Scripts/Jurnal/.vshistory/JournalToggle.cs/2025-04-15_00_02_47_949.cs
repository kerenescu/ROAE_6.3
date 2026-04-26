using UnityEngine;

public class JournalToggle : MonoBehaviour
{
    [Header("References")]
    public GameObject journalUI;
    public GameObject closedJournal;
    public GameObject navigationButtons;

    [Header("Gameplay Control")]
    private Collider2D[] allColliders;
    private MonoBehaviour[] inputScripts;

    private bool isJournalOpen = false;

    void Start()
    {
        allColliders = FindObjectsOfType<Collider2D>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            inputScripts = player.GetComponents<MonoBehaviour>();

        // Închidem tot la început
        closedJournal.SetActive(false);
        journalUI.SetActive(false);
        if (navigationButtons != null)
            navigationButtons.SetActive(false);
    }

    public void ToggleJournal()
    {
        if (isJournalOpen)
            CloseJournal();
        else
            ShowClosedJournal();
    }

    public void ShowClosedJournal()
    {
        closedJournal.SetActive(true);
        journalUI.SetActive(false);
        if (navigationButtons != null)
            navigationButtons.SetActive(false);

        isJournalOpen = false;
        ToggleGameplayInteraction(false);
    }

    public void FlipOpen()
    {
        closedJournal.SetActive(false);
        journalUI.SetActive(true);
        if (navigationButtons != null)
            navigationButtons.SetActive(true);

        var jm = FindObjectOfType<JournalManager>();
        if (jm != null) jm.ShowPage(0);

        isJournalOpen = true;
        ToggleGameplayInteraction(false);
    }

    public void CloseJournal()
    {
        closedJournal.SetActive(false);
        journalUI.SetActive(false);
        if (navigationButtons != null)
            navigationButtons.SetActive(false);

        isJournalOpen = false;
        ToggleGameplayInteraction(true);
    }

    private void ToggleGameplayInteraction(bool enable)
    {
        if (allColliders == null || allColliders.Length == 0)
            allColliders = FindObjectsOfType<Collider2D>();

        if (inputScripts == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                inputScripts = player.GetComponents<MonoBehaviour>();
        }

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

        Time.timeScale = enable ? 1f : 0f;
    }
}
