// JournalButtonTrigger.cs – deschide jurnalul închis când apeși butonul din UI sau o tastă
using UnityEngine;

public class JournalButtonTrigger : MonoBehaviour
{
    private JournalToggle journalToggle;

    void Start()
    {
        journalToggle = FindObjectOfType<JournalToggle>();
    }

    public void OnButtonClick()
    {
        Debug.Log("Butonul Jurnal a fost apăsat!");

        if (journalToggle != null)
        {
            journalToggle.ShowClosedJournal();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) // shortcut pentru tastatură
        {
            OnButtonClick();
        }
    }
}
