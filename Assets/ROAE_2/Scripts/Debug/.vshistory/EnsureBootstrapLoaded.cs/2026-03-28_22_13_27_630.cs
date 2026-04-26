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

        yield return null;

        if (unloadBootstrapSceneAfterInit)
        {
            bootstrapScene = SceneManager.GetSceneByName(bootstrapSceneName);
            if (bootstrapScene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(bootstrapScene);
        }
    }
}