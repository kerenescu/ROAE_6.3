using UnityEngine;

public class JournalToggle : MonoBehaviour
{
    public GameObject closedJournal;
    public GameObject journalUI;

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

    public void ShowClosedJournal()
    {
        Debug.Log("📖 ShowClosedJournal() called");

        if (closedJournal != null)
        {
            Debug.Log("✅ closedJournal is NOT null. Activating...");
            closedJournal.SetActive(true);
        }
        else
        {
            Debug.LogError("❌ closedJournal is NULL! NU e setat în Inspector.");
        }

        journalUI.SetActive(false);
    }


    public void OpenJournal()
    {
        closedJournal.SetActive(false);
        journalUI.SetActive(true);
        FindObjectOfType<JournalManager>().ShowThoughts(); // sau ShowFlowers/Tasks dacă vrei altă pagină
    }

    public void CloseJournal()
    {
        closedJournal.SetActive(false);
        journalUI.SetActive(false);
    }
}
