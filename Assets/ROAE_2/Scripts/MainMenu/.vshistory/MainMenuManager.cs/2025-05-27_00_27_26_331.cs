using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene names (set in Inspector)")]
    public string gameSceneName = "BootstrapScene";
    public string extrasSceneName = "ExtrasScene";

    public void PlayGame()
    {
        Debug.Log("Butonul Play a fost apasat.");

        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError($"Scena '{gameSceneName}' nu a fost adăugată în Build Settings! Mergi la File > Build Settings și adaug-o.");
            return;
        }

        if (SceneManager.GetActiveScene().name == gameSceneName)
        {
            Debug.LogWarning("Ești deja în scena de joc.");
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void OpenSettings()
    {
        Debug.Log("Settings panel to be shown here.");
        // Aici poți activa un panel sau deschide un nou UI
    }

    public void OpenExtras()
    {
        if (!Application.CanStreamedLevelBeLoaded(extrasSceneName))
        {
            Debug.LogError($"Scena '{extrasSceneName}' nu a fost adăugată în Build Settings!");
            return;
        }

        SceneManager.LoadScene(extrasSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("Iesire joc (în editor nu va funcționa)");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Doar pentru editor
#endif
    }


    public string mainMenuSceneName = "MainMenuScene";

    public void GoToMainMenu()
    {
        Debug.Log("Încerc să încarc meniul principal...");

        if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError($"Scena '{mainMenuSceneName}' nu a fost adăugată în Build Settings!");
            return;
        }

        if (SceneManager.GetActiveScene().name == mainMenuSceneName)
        {
            Debug.LogWarning("Ești deja în meniul principal.");
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

}
