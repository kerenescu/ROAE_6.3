// 🎯 JournalUIFlow.cs – Control complet de jurnal: buton → imagine închisă → imagine deschisă + săgeți
using UnityEngine;
using UnityEngine.UI;

public class JournalUIFlow : MonoBehaviour
{
    [Header("References")]
    public GameObject journalButton;            // Butonul vizibil în gameplay
    public GameObject closedJournalImage;       // Imaginea cu jurnalul închis
    public GameObject openJournalImage;         // Imaginea cu jurnalul deschis
    public GameObject navigationButtons;        // Container cu butoanele ← →
    public GameObject closeButton;              // Butonul de închidere


    private bool isOpen = false;

    void Start()
    {
        // La start, doar butonul de jurnal este activ
        journalButton.SetActive(true);
        closedJournalImage.SetActive(false);
        openJournalImage.SetActive(false);
        navigationButtons.SetActive(false);
        if (closeButton != null)
            closeButton.SetActive(false);

    }

    // Apelat din butonul "Journal"
    public void OnJournalButtonPressed()
    {
        journalButton.SetActive(false);
        closedJournalImage.SetActive(true);
        closeButton.SetActive(true);
    }

    // Apelat din OnClick de pe imaginea jurnalului închis
    public void OnClosedJournalClicked()
    {
        closedJournalImage.SetActive(false);
        openJournalImage.SetActive(true);
        navigationButtons.SetActive(true);
        isOpen = true;
        if (closeButton == null)
            closeButton.SetActive(true);

    }

    // Poți adăuga și un buton de închidere dacă vrei
    public void CloseJournalCompletely()
    {
        openJournalImage.SetActive(false);
        navigationButtons.SetActive(false);
        journalButton.SetActive(true);
        isOpen = false;

        if (closeButton != null)
            closeButton.SetActive(false);

    }
}
