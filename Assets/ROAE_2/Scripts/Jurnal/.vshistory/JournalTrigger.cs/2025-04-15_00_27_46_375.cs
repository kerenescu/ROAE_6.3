// 🎯 JournalImageTrigger.cs – când userul apasă pe imaginea cu jurnalul închis, se deschide jurnalul complet
using UnityEngine;
using UnityEngine.UI;

public class JournalImageTrigger : MonoBehaviour
{
    public GameObject closedJournalImage;
    public JournalManager journalManager;

    void Start()
    {
        // Se asigură că imaginea închisă are buton
        if (closedJournalImage != null && closedJournalImage.GetComponent<Button>() == null)
        {
            closedJournalImage.AddComponent<Button>().onClick.AddListener(TriggerOpenJournal);
        }
    }

    public void TriggerOpenJournal()
    {
        if (closedJournalImage != null)
            closedJournalImage.SetActive(false);

        if (journalManager != null)
            journalManager.OpenJournal();
        else
            Debug.LogError("JournalManager not set!");
    }
}
