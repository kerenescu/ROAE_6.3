using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("BootstrapScene"); // Schimbă cu prima scenă reală
    }

    public void OpenSettings()
    {
        Debug.Log("Settings panel to be shown here.");
        // Activează un panel UI dacă ai
    }

    public void OpenExtras()
    {
        SceneManager.LoadScene("ExtrasScene"); // Sau deschide panel
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
