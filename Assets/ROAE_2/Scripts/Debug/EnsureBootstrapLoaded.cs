using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnsureBootstrapLoaded : MonoBehaviour
{
    [SerializeField] private string bootstrapSceneName = "BootstrapScene";
    [SerializeField] private bool unloadBootstrapSceneAfterInit = true;
    [SerializeField] private bool persistentRuntimeGuard = false;
    [SerializeField] private float bootstrapReadyTimeoutSeconds = 1.5f;
    [SerializeField] private GameObject uiContainerPrefab;
    [SerializeField] private bool instantiateUiContainerWhenMissing = true;
    [SerializeField] private bool allowBootstrapSceneFallback = false;

    private bool isEnsuring;
    private const string UiContainerPrefabPath = "Assets/ROAE_2/Prefabs/UIContainer.prefab";

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private IEnumerator Start()
    {
        yield return EnsureLoadedIfNeeded();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isActiveAndEnabled)
            return;

        StartCoroutine(EnsureLoadedIfNeeded());
    }

    private IEnumerator EnsureLoadedIfNeeded()
    {
        if (isEnsuring)
            yield break;

        if (SceneManager.GetActiveScene().name == bootstrapSceneName)
            yield break;

        if (AreBootstrapSystemsAvailable())
            yield break;

        isEnsuring = true;

        if (instantiateUiContainerWhenMissing)
        {
            EnsureGameplayUiRoot();
            yield return null;
        }

        if (!AreBootstrapSystemsAvailable() && allowBootstrapSceneFallback)
            yield return LoadBootstrapSceneFallback();

        isEnsuring = false;
    }

    private IEnumerator LoadBootstrapSceneFallback()
    {
        Scene bootstrapScene = SceneManager.GetSceneByName(bootstrapSceneName);

        if (!bootstrapScene.isLoaded)
            yield return SceneManager.LoadSceneAsync(bootstrapSceneName, LoadSceneMode.Additive);

        bootstrapScene = SceneManager.GetSceneByName(bootstrapSceneName);
        PromoteBootstrapUiRoot(bootstrapScene);
        yield return WaitForBootstrapSystems();

        bootstrapScene = SceneManager.GetSceneByName(bootstrapSceneName);
        DisableRuntimeSceneDriversInScene(bootstrapScene);

        if (!unloadBootstrapSceneAfterInit || !AreBootstrapSystemsAvailable())
            yield break;

        bootstrapScene = SceneManager.GetSceneByName(bootstrapSceneName);
        if (bootstrapScene.isLoaded)
            yield return SceneManager.UnloadSceneAsync(bootstrapScene);
    }

    private void EnsureGameplayUiRoot()
    {
        if (HasGameplayUiRoot())
            return;

        if (TryInstantiateConfiguredUiContainer(out GameObject instance) ||
            TryInstantiateEditorFallback(out instance))
        {
            PrepareUiRoot(instance);
            DontDestroyOnLoad(instance);
            return;
        }

        if (uiContainerPrefab == null)
        {
            Debug.LogWarning("EnsureBootstrapLoaded is missing a UIContainer prefab reference.", this);
            return;
        }

        Debug.LogError("EnsureBootstrapLoaded could not instantiate a UIContainer GameObject.", this);
    }

    private bool TryInstantiateConfiguredUiContainer(out GameObject instance)
    {
        instance = null;

        if (uiContainerPrefab == null)
            return false;

        Object instanceObject = Instantiate((Object)uiContainerPrefab);
        instance = instanceObject as GameObject;
        if (instance == null && instanceObject is Component component)
            instance = component.gameObject;

        return instance != null;
    }

    private static bool TryInstantiateEditorFallback(out GameObject instance)
    {
        instance = null;

#if UNITY_EDITOR
        GameObject prefabAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UiContainerPrefabPath);
        if (prefabAsset == null)
            return false;

        Object prefabInstance = UnityEditor.PrefabUtility.InstantiatePrefab(prefabAsset);
        instance = prefabInstance as GameObject;
        if (instance == null && prefabInstance is Component component)
            instance = component.gameObject;

        return instance != null;
#else
        return false;
#endif
    }

    private static void PrepareUiRoot(GameObject uiRoot)
    {
        if (uiRoot == null)
            return;

        uiRoot.SetActive(true);
        uiRoot.transform.localScale = Vector3.one;

        RectTransform rectTransform = uiRoot.transform as RectTransform;
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        Canvas canvas = uiRoot.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
            canvas.planeDistance = 100f;
        }
    }

    private IEnumerator WaitForBootstrapSystems()
    {
        float timeoutAt = Time.unscaledTime + Mathf.Max(0.1f, bootstrapReadyTimeoutSeconds);

        while (!AreBootstrapSystemsAvailable() && Time.unscaledTime < timeoutAt)
            yield return null;
    }

    private static bool AreBootstrapSystemsAvailable()
    {
        return CreativeCore.Instance != null &&
               CreativeHUD.Instance != null &&
               MessageManager.Instance != null &&
               JournalUIFlow.Instance != null &&
               HasGameplayUiRoot();
    }

    private static bool HasGameplayUiRoot()
    {
        Transform uiRoot = null;

        if (PhoneUIFlow.Instance != null)
            uiRoot = PhoneUIFlow.Instance.transform.root;
        else if (JournalUIFlow.Instance != null)
            uiRoot = JournalUIFlow.Instance.transform.root;
        else if (CreativeHUD.Instance != null)
            uiRoot = CreativeHUD.Instance.transform.root;

        return uiRoot != null && uiRoot.gameObject.activeInHierarchy;
    }

    private static void PromoteBootstrapUiRoot(Scene scene)
    {
        GameObject uiRoot = FindBootstrapUiRoot(scene);
        if (uiRoot == null)
            return;

        PrepareUiRoot(uiRoot);
        DontDestroyOnLoad(uiRoot);
    }

    private static GameObject FindBootstrapUiRoot(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return null;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];
            if (root == null)
                continue;

            if (root.name == "UIContainer")
                return root;

            if (root.GetComponentInChildren<PhoneUIFlow>(true) != null ||
                root.GetComponentInChildren<JournalUIFlow>(true) != null ||
                root.GetComponentInChildren<CreativeHUD>(true) != null)
            {
                return root;
            }
        }

        return null;
    }

    private static void DisableRuntimeSceneDriversInScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            AudioListener[] listeners = roots[i].GetComponentsInChildren<AudioListener>(true);
            for (int j = 0; j < listeners.Length; j++)
                listeners[j].enabled = false;

            Camera[] cameras = roots[i].GetComponentsInChildren<Camera>(true);
            for (int j = 0; j < cameras.Length; j++)
                cameras[j].enabled = false;

            SpriteRenderer[] spriteRenderers = roots[i].GetComponentsInChildren<SpriteRenderer>(true);
            for (int j = 0; j < spriteRenderers.Length; j++)
                spriteRenderers[j].enabled = false;

            UnityEngine.EventSystems.EventSystem[] eventSystems =
                roots[i].GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true);
            for (int j = 0; j < eventSystems.Length; j++)
                eventSystems[j].enabled = false;

            BootstrapLoader[] bootstrapLoaders = roots[i].GetComponentsInChildren<BootstrapLoader>(true);
            for (int j = 0; j < bootstrapLoaders.Length; j++)
                bootstrapLoaders[j].enabled = false;

            LianaFallWorld[] lianaIntros = roots[i].GetComponentsInChildren<LianaFallWorld>(true);
            for (int j = 0; j < lianaIntros.Length; j++)
                lianaIntros[j].enabled = false;
        }
    }
}
