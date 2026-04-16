// === BootstrapLoader.cs ===
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [Header("Setează prefab-ul complet al UIContainer")]
    [SerializeField] private GameObject uiContainerPrefab;

    [Header("Numele scenei inițiale")]
    [SerializeField] private string scenaInitiala = "Flower_Field";

    private void Awake()
    {
        // Caută dacă există deja un UIContainer în joc
        GameObject existingUI = GameObject.FindGameObjectWithTag("UIContainer");

        if (existingUI == null)
        {
            Debug.Log("[BootstrapLoader] Instanțiem UIContainer...");
            GameObject ui = Instantiate(uiContainerPrefab);
            ui.name = "UIContainer";
            ui.tag = "UIContainer";
            ui.SetActive(true); // ne asigurăm că e vizibil
            DontDestroyOnLoad(ui);
        }
        else
        {
            Debug.Log("[BootstrapLoader] UIContainer deja prezent. Nu instanțiem altul.");
        }

        Debug.Log("[BootstrapLoader] Încărcăm scena de start: " + scenaInitiala);
        SceneManager.LoadScene(scenaInitiala);
    }
}
