// === BootstrapLoader.cs ===
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string scenaInitiala = "Flower_Field";

    void Start()
    {
        Debug.Log("[BootstrapLoader] Incarcam scena initiala...");
        SceneManager.LoadScene(scenaInitiala);
    }
}


