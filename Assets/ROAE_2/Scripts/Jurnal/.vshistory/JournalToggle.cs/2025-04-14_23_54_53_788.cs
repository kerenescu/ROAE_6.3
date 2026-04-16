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
        Debug.Log("navigationButtons = " + navigationButtons);
        if (navigationButtons != null)
            navigationButtons.SetActive(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            inputScripts = player.GetComponents<MonoBehaviour>();
        }
    }

    public void FlipOpen()
    {
        closedJournal.SetActive(false);          // dispare jurnalul închis
        openJournalImage.SetActive(false);       // imaginea deschisă NU e necesară
        journalUI.SetActive(true);               // afișăm UI complet
        navigationButtons.SetActive(true);       // afișăm săgețile

        FindObjectOfType<JournalManager>().ShowPage(0); // pagina 1
        ToggleGameplayInteraction(false);               // oprește jocul
    }




    public void ShowClosedJournal()
    {
        closedJournal.SetActive(true);           // ✅ doar jurnalul închis
        openJournalImage.SetActive(false);       // ❌ imaginea deschisă NU apare aici
        journalUI.SetActive(false);              // asigură-te că UI-ul e oprit
        navigationButtons.SetActive(false);      // săgețile NU apar

        ToggleGameplayInteraction(false);
    }



    public void OpenJournal()
    {
        closedJournal.SetActive(false);
        openJournalImage.SetActive(false);
        journalUI.SetActive(true);

        if (navigationButtons != null)
            navigationButtons.SetActive(true);

        FindObjectOfType<JournalManager>().ShowPage(0);
        ToggleGameplayInteraction(false);
    }



    public void CloseJournal()
    {
        closedJournal.SetActive(false);
        openJournalImage.SetActive(false);
        journalUI.SetActive(false);

        if (navigationButtons != null)
            navigationButtons.SetActive(false);

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
