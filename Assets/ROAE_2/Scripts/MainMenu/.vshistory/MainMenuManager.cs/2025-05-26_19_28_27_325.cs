using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("Butonul Play a fost apasat");
        if ("BootstrapScene" != SceneManager.GetActiveScene().name)
            Debug.LogWarning("Nu ești în scena BootstrapScene! Asigură-te că ai încărcat scena corectă înainte de a începe jocul.");
        SceneManager.LoadScene("BootstrapScene"); // sau orice altă scenă
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
