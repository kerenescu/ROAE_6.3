using UnityEngine;
using UnityEngine.SceneManagement;

public class UIBootstrapper : MonoBehaviour
{
    private Canvas rootCanvas;
    private RectTransform rootRect;

    private void Awake()
    {
        rootCanvas = transform.root.GetComponent<Canvas>();
        rootRect = transform.root as RectTransform;

        NormalizeForPersistentUse();
        DontDestroyOnLoad(transform.root.gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        NormalizeForPersistentUse();
    }

    private void NormalizeForPersistentUse()
    {
        Transform root = transform.root;
        root.gameObject.SetActive(true);
        root.localScale = Vector3.one;

        if (rootRect != null)
        {
            rootRect.localScale = Vector3.one;
            rootRect.localPosition = Vector3.zero;
            rootRect.anchoredPosition = Vector2.zero;
        }

        if (rootCanvas != null)
        {
            // This UI survives BootstrapScene unloading, so it cannot keep a
            // dependency on BootstrapScene's camera when scenes are started directly.
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.worldCamera = null;
            rootCanvas.planeDistance = 100f;
        }
    }
}
