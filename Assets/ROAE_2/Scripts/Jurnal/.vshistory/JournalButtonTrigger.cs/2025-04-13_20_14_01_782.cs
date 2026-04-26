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

        bool isAnythingActive =
            journalToggle.closedJournal.activeSelf ||
            journalToggle.openJournalImage.activeSelf ||
            journalToggle.journalUI.activeSelf;

        if (isAnythingActive)
        {
            // Dacă oricare dintre ele e activ → închide tot
            journalToggle.CloseJournal();
        }
        else
        {
            // Altminteri → pornește cu jurnalul închis
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
