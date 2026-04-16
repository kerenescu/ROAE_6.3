using UnityEngine;

public class JournalToggle : MonoBehaviour
{
    public GameObject closedJournal;
    public GameObject journalUI;

    public GameObject openJournalImage; // imaginea deschisă

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
        journalUI.SetActive(true);
        //FindObjectOfType<JournalManager>().ShowThoughts();

        ToggleGameplayInteraction(false); // încă suntem în jurnal
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
        foreach (Collider2D col in allColliders)
            col.enabled = enable;

        // Asigură-te că inputScripts nu e null
        if (inputScripts == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                inputScripts = player.GetComponents<MonoBehaviour>();
            }
        }

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
