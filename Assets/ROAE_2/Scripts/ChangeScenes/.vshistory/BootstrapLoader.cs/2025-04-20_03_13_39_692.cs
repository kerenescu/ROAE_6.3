using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [Header("Scena de start")]
    public string scenaInitiala = "Flower_Field";

    void Start()
    {
        Debug.Log("[BootstrapLoader] Încărcăm scena de start...");
        SceneManager.LoadScene(scenaInitiala);
    }
}
