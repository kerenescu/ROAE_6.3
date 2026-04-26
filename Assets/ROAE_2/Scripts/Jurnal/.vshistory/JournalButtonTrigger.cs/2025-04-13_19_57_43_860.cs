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
        Debug.Log("📎 Butonul Jurnal a fost apăsat!");

        if (journalToggle == null)
            journalToggle = FindObjectOfType<JournalToggle>();

        if (journalToggle.closedJournal.activeSelf || journalToggle.journalUI.activeSelf)
        {
            // dacă jurnalul (în orice formă) e activ → îl închidem
            journalToggle.CloseJournal();
        }
        else
        {
            // altfel îl deschidem (închis vizual)
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
