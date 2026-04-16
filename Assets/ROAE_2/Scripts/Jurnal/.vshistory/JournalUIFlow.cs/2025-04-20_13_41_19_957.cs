using UnityEngine;

public class JournalUIFlow : MonoBehaviour
{
    public GameObject closedJournalImage;
    public GameObject openJournalImage;
    public GameObject thoughtsPage;
    public GameObject tasksPage;
    public GameObject flowersPage;

    private bool isOpen = false;

    public void OnJournalImageClicked()
    {
        closedJournalImage.SetActive(false);
        openJournalImage.SetActive(true);
        isOpen = true;

        // Arătăm pagina default, ex: gânduri
        ShowThoughtsPage();
    }

    public void OnCloseButtonPressed()
    {
        openJournalImage.SetActive(false);
        closedJournalImage.SetActive(true);
        isOpen = false;
    }

    public void ShowThoughtsPage()
    {
        thoughtsPage.SetActive(true);
        tasksPage.SetActive(false);
        flowersPage.SetActive(false);
    }

    public void ShowTasksPage()
    {
        thoughtsPage.SetActive(false);
        tasksPage.SetActive(true);
        flowersPage.SetActive(false);
    }

    public void ShowFlowersPage()
    {
        thoughtsPage.SetActive(false);
        tasksPage.SetActive(false);
        flowersPage.SetActive(true);
    }
}
