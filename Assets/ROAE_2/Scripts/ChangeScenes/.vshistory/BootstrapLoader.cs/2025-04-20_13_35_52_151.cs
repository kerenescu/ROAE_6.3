using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    public GameObject uiContainerPrefab;

    private void Awake()
    {
        // Dacă există deja un UIContainer în scenă, nu mai instanțiem altul
        GameObject existingUI = GameObject.Find("/UIContainer");
        if (existingUI == null)
        {
            GameObject ui = Instantiate(uiContainerPrefab);
            ui.name = "UIContainer";

            // Asigură-te că tot UI-ul este activ
            ActivateAllChildren(ui);

            // Îl facem persistent în toate scenele
            DontDestroyOnLoad(ui);
        }

        // Așteaptă un frame și apoi încarcă scena principală
        StartCoroutine(LoadMainSceneNextFrame());
    }

    private System.Collections.IEnumerator LoadMainSceneNextFrame()
    {
        yield return null; // așteaptă un frame
        SceneManager.LoadScene("Flower_Field");
    }

    // Activează toate GameObject-urile inactive dintr-un prefab
    private void ActivateAllChildren(GameObject root)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.SetActive(true);
        }
    }
}
