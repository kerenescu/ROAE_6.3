using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("🚪 BootstrapLoader: Loading Flower_Field in 1 second...");
        Invoke("LoadGameScene", 1f);
    }

    void LoadGameScene()
    {
        SceneManager.LoadScene("Flower_Field");
    }
}
