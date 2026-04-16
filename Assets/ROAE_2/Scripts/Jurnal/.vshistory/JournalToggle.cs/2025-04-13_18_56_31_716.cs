using UnityEngine;

public class JournalToggle : MonoBehaviour
{
    public GameObject closedJournal;
    public GameObject journalUI;

    public void ShowClosedJournal()
    {
        closedJournal.SetActive(true);
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
