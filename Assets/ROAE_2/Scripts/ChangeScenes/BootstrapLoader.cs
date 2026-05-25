using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    private void Start()
    {
        if (!IsOwningSceneActive())
            return;

        Invoke("LoadGameScene", 1f);
    }

    void LoadGameScene()
    {
        if (!IsOwningSceneActive())
            return;

        SceneManager.LoadScene("Flower_Field");
    }

    private bool IsOwningSceneActive()
    {
        return gameObject.scene.IsValid() &&
               gameObject.scene == SceneManager.GetActiveScene();
    }
}
