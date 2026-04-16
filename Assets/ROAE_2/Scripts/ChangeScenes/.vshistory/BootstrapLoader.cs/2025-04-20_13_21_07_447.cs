using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    public GameObject uiContainerPrefab;

    private void Awake()
    {
        GameObject existing = GameObject.Find("UIContainer");
        if (existing == null)
        {
            GameObject ui = Instantiate(uiContainerPrefab);
            ui.name = "UIContainer";
            DontDestroyOnLoad(ui);
        }

        // Încarcă prima scenă reală (asigură-te că e adăugată în Build Settings)
        SceneManager.LoadScene("Flower_Field");
    }
}
