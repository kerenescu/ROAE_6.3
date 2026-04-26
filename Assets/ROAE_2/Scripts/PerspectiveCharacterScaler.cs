using AC;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

[DisallowMultipleComponent]
public class PerspectiveCharacterScaler : MonoBehaviour
{
    [System.Serializable]
    private struct SceneShrinkOverride
    {
        public string sceneName;
        public float shrinkAmountMultiplier;
    }

    [SerializeField] private Transform scaledTransform;
    [SerializeField] private float scaleStartViewportY = 0.18f;
    [SerializeField] private float shrinkDistanceViewportY = 0.25f;
    [SerializeField] private float minScaleMultiplier = 0.1f;
    [SerializeField] private bool ignoreIfSortingMapControlsScale = true;
    [SerializeField] private SceneShrinkOverride[] sceneShrinkOverrides;

    private AC.Char character;
    private FollowSortingMap followSortingMap;
    private Vector3 baseLocalScale;

    private void Awake()
    {
        CacheReferences();
        CacheBaseScale();
    }

    private void OnEnable()
    {
        CacheReferences();
        CacheBaseScale();
        RestoreBaseScale();
    }

    private void LateUpdate()
    {
        CacheReferences();

        if (scaledTransform == null)
        {
            return;
        }

        if (ShouldDeferToSortingMap())
        {
            RestoreBaseScale();
            return;
        }

        Camera activeCamera = GetActiveCamera();
        if (activeCamera == null)
        {
            RestoreBaseScale();
            return;
        }

        Vector3 viewportPoint = activeCamera.WorldToViewportPoint(transform.position);
        if (viewportPoint.z < 0f)
        {
            RestoreBaseScale();
            return;
        }

        float scaleEndViewportY = scaleStartViewportY + Mathf.Max(shrinkDistanceViewportY, 0.01f);
        float normalized = Mathf.InverseLerp(scaleStartViewportY, scaleEndViewportY, viewportPoint.y);
        float multiplier = Mathf.Lerp(1f, Mathf.Max(minScaleMultiplier, 0.01f), normalized);
        multiplier = ApplySceneShrinkOverride(multiplier);
        ApplyScale(multiplier);
    }

    private void CacheReferences()
    {
        if (character == null)
        {
            character = GetComponent<AC.Char>();
        }

        if (scaledTransform == null && character != null && character.spriteChild != null)
        {
            scaledTransform = character.spriteChild;
        }

        if (scaledTransform == null)
        {
            SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            if (spriteRenderer != null)
            {
                scaledTransform = spriteRenderer.transform;
            }
        }

        if (followSortingMap == null && scaledTransform != null)
        {
            followSortingMap = scaledTransform.GetComponent<FollowSortingMap>();
        }
    }

    private void CacheBaseScale()
    {
        if (scaledTransform == null)
        {
            return;
        }

        if (baseLocalScale == Vector3.zero)
        {
            baseLocalScale = new Vector3(
                Mathf.Abs(scaledTransform.localScale.x),
                Mathf.Abs(scaledTransform.localScale.y),
                Mathf.Abs(scaledTransform.localScale.z));
        }
    }

    private bool ShouldDeferToSortingMap()
    {
        if (!ignoreIfSortingMapControlsScale || followSortingMap == null)
        {
            return false;
        }

        return !Mathf.Approximately(followSortingMap.GetLocalScale(), 0f);
    }

    private Camera GetActiveCamera()
    {
        if (KickStarter.mainCamera != null && KickStarter.mainCamera.Camera != null)
        {
            return KickStarter.mainCamera.Camera;
        }

        return Camera.main;
    }

    private float ApplySceneShrinkOverride(float multiplier)
    {
        float shrinkAmountMultiplier = GetSceneShrinkAmountMultiplier();
        float shrinkAmount = 1f - multiplier;
        return 1f - (shrinkAmount * shrinkAmountMultiplier);
    }

    private float GetSceneShrinkAmountMultiplier()
    {
        if (sceneShrinkOverrides == null || sceneShrinkOverrides.Length == 0)
        {
            return 1f;
        }

        string activeSceneName = UnitySceneManager.GetActiveScene().name;
        for (int i = 0; i < sceneShrinkOverrides.Length; i++)
        {
            if (string.Equals(sceneShrinkOverrides[i].sceneName, activeSceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return Mathf.Max(0f, sceneShrinkOverrides[i].shrinkAmountMultiplier);
            }
        }

        return 1f;
    }

    private void RestoreBaseScale()
    {
        ApplyScale(1f);
    }

    private void ApplyScale(float multiplier)
    {
        if (scaledTransform == null)
        {
            return;
        }

        if (baseLocalScale == Vector3.zero)
        {
            CacheBaseScale();
        }

        float xSign = Mathf.Sign(scaledTransform.localScale.x);
        if (Mathf.Approximately(xSign, 0f))
        {
            xSign = 1f;
        }

        scaledTransform.localScale = new Vector3(
            xSign * baseLocalScale.x * multiplier,
            baseLocalScale.y * multiplier,
            baseLocalScale.z * multiplier);
    }
}
