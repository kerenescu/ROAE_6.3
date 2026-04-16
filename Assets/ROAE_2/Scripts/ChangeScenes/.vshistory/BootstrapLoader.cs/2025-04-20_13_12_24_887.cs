using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    public GameObject uiContainerPrefab;
    public string initialSceneName = "Flower_Field"; // sau altceva

    void Awake()
    {
        // Evită duplicarea UIContainer-ului
        GameObject existing = GameObject.Find("UIContainer");
        if (existing == null)
        {
            GameObject uiInstance = Instantiate(uiContainerPrefab);
            uiInstance.name = "UIContainer";
            DontDestroyOnLoad(uiInstance);

            // Activează toate componentele UI
            uiInstance.SetActive(true);

            // (opțional) setează un tag special, dacă l-ai creat în Unity
             uiInstance.tag = "UIContainer";
        }

        // Încarcă scena inițială
        SceneManager.LoadScene(initialSceneName);
    }
}
