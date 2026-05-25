using UnityEngine;

public class CompanionBootstrapper : MonoBehaviour
{
    [Header("Authoring")]
    [SerializeField] private CompanionProfile profile;
    [SerializeField] private CompanionDialogueLibrary dialogueLibrary;
    [SerializeField] private CompanionManifestationController manifestationPrefab;

    [Header("Runtime")]
    [SerializeField] private bool keepAcrossScenes = true;
    [SerializeField] private bool dismissOnSceneChange = false;
    [SerializeField] private bool debugLogs;

    private void Awake()
    {
        CompanionSystem.EnsureBootstrapHost(
            profile,
            dialogueLibrary,
            manifestationPrefab,
            keepAcrossScenes,
            dismissOnSceneChange,
            debugLogs);

        Destroy(gameObject);
    }
}
