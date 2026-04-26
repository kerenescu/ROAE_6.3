using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnsureBootstrapLoaded : MonoBehaviour
{
    [SerializeField] private string bootstrapSceneName = "BootstrapScene";
    [SerializeField] private bool unloadBootstrapSceneAfterInit = true;

    private IEnumerator Start()
    {
        if (SceneManager.GetActiveScene().name == bootstrapSceneName)
            yield break;

        if (CreativeCore.Instance != null)
            yield break;

        Scene bootstrapScene = SceneManager.GetSceneByName(bootstrapSceneName);

        if (!bootstrapScene.isLoaded)
            yield return SceneManager.LoadSceneAsync(bootstrapSceneName, LoadSceneMode.Additive);

        bootstrapScene = SceneManager.GetSceneByName(bootstrapSceneName);
        DisableAudioListenersInScene(bootstrapScene);

        yield return null;

        if (unloadBootstrapSceneAfterInit)
        {
            bootstrapScene = SceneManager.GetSceneByName(bootstrapSceneName);
            if (bootstrapScene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(bootstrapScene);
        }
    }

    private static void DisableAudioListenersInScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            AudioListener[] listeners = roots[i].GetComponentsInChildren<AudioListener>(true);
            for (int j = 0; j < listeners.Length; j++)
                listeners[j].enabled = false;
        }
    }
}
