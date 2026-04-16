using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelHomeMenu;
    public GameObject panelMessages;
    public GameObject panelCamera;
    public GameObject panelGallery;
    public GameObject panelNotes;
    public GameObject panelGame;

    [Header("Manageri Secundari")]
    public MessageManager messageManager;

    void Start()
    {
        BackToMenu(); // Pornim cu meniul principal
    }

    public void OpenMessages()
    {
        CloseAll();
        panelMessages.SetActive(true);
        if (messageManager != null)
        {
            messageManager.OpenConversations(); // metode din MessageManager
        }
    }

    public void OpenCamera()
    {
        CloseAll();
        panelCamera.SetActive(true);
        // Poți adăuga aici activare cameră, efecte etc.
    }

    public void OpenGallery()
    {
        CloseAll();
        panelGallery.SetActive(true);
        // eventual refresh la conținutul galeriei
    }

    public void OpenNotes()
    {
        CloseAll();
        panelNotes.SetActive(true);
        // încarcă task-urile curente etc.
    }

    public void OpenGame()
    {
        CloseAll();
        panelGame.SetActive(true);
        // pornește mini-game-ul, dacă există logică
    }

    public void BackToMenu()
    {
        CloseAll();
        panelHomeMenu.SetActive(true);
    }

    private void CloseAll()
    {
        panelHomeMenu?.SetActive(false);
        panelMessages?.SetActive(false);
        panelCamera?.SetActive(false);
        panelGallery?.SetActive(false);
        panelNotes?.SetActive(false);
        panelGame?.SetActive(false);
    }
}
