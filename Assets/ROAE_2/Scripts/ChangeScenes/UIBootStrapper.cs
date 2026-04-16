using UnityEngine;

public class UIBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("🛡 UIBootstrapper: Applying DontDestroyOnLoad to UIContainer");
        DontDestroyOnLoad(transform.root.gameObject);
        Debug.Log($"🛡 UIBootstrapper: DontDestroy applied to {transform.root.name}");

    }
}
