// 🎯 JournalUIFlow.cs – Control complet de jurnal: buton → imagine închisă → imagine deschisă + săgeți
using UnityEngine;
using UnityEngine.UI;

public class JournalUIFlow : MonoBehaviour
{
    
    [SerializeField]
    private GameObject closedJournalImage;       // Imaginea cu jurnalul închis
    [SerializeField]
    private GameObject openJournalImage;         // Imaginea cu jurnalul deschis
    [SerializeField]
    private GameObject navigationButtons;        // Container cu butoanele ← →
    [SerializeField]
    private GameObject journalButton;            // Butonul vizibil în gameplay
    [SerializeField]
    private GameObject closeButton;              // Butonul de închidere



    void Start()
    {
        // La start, doar butonul de jurnal este activ
        journalButton.SetActive(true);
        closedJournalImage.SetActive(false);
        openJournalImage.SetActive(false);
        navigationButtons.SetActive(false);
        closeButton.SetActive(false);

    }

    // Apelat din butonul "Journal"
    public void OnJournalButtonPressed()
    {
        Debug.Log("Open Journal Button pressed");

        journalButton.SetActive(false);
        closedJournalImage.SetActive(true);
        closeButton.SetActive(true);
        navigationButtons.SetActive(false);
    }

    // Apelat din OnClick de pe imaginea jurnalului închis
    public void OnClosedJournalClicked()
    {
        closedJournalImage.SetActive(false);
        openJournalImage.SetActive(true);
        navigationButtons.SetActive(true);
        closeButton.SetActive(true);

    }

    // Inchid de tot jurnalu
    public void CloseJournalCompletely()
    {
        closedJournalImage.SetActive(false);
        openJournalImage.SetActive(false);
        navigationButtons.SetActive(false);
        journalButton.SetActive(true);
        closeButton.SetActive(false);

    }
}
