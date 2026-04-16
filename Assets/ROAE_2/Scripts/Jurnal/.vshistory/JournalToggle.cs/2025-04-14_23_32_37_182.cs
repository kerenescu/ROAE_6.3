using UnityEngine;

public class JournalToggle : MonoBehaviour
{
    
    public GameObject journalUI; 
    
    public GameObject closedJournal; // jurnalul închis
    public GameObject openJournalImage; // imaginea deschisă

    public GameObject navigationButtons; // butoanele de navigare

    private Collider2D[] allColliders;
    private MonoBehaviour[] inputScripts;

    void Start()
    {
        allColliders = FindObjectsOfType<Collider2D>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            inputScripts = player.GetComponents<MonoBehaviour>();
        }
    }

    public void FlipOpen()
    {
        closedJournal.SetActive(false);
        openJournalImage.SetActive(true);
    }


    public void ShowClosedJournal()
    {
        closedJournal.SetActive(true);
        journalUI.SetActive(false);

        ToggleGameplayInteraction(false);
    }

    public void OpenJournal()
    {
        closedJournal.SetActive(false);
        openJournalImage.SetActive(false);
        journalUI.SetActive(true);

        if (navigationButtons != null)
            navigationButtons.SetActive(true);

        FindObjectOfType<JournalManager>().ShowThoughts(); // sau ShowPage(0)
        ToggleGameplayInteraction(false);
    }


    public void CloseJournal()
    {
        if (closedJournal != null) closedJournal.SetActive(false);
        if (openJournalImage != null) openJournalImage.SetActive(false);
        if (journalUI != null) journalUI.SetActive(false);

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
            {
                inputScripts = player.GetComponents<MonoBehaviour>();
            }
            else
            {
                Debug.LogWarning("⚠️ Nu am găsit obiect cu tag Player! Sari blocarea inputului.");
            }
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
