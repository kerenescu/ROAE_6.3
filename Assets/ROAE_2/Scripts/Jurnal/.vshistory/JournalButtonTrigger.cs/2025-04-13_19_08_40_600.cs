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
