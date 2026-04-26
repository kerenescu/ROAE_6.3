using UnityEngine;

public class DestroyUIFromBootstrap : MonoBehaviour
{
    [Tooltip("Numele exact al GameObject-ului marcat DontDestroyOnLoad (ex: UIContainer)")]
    public string persistentObjectName = "UIContainer";

    void Start()
    {
        GameObject persistentUI = GameObject.Find(persistentObjectName);

        if (persistentUI != null)
        {
            Destroy(persistentUI);
            Debug.Log($"💣 Am distrus {persistentObjectName} din DontDestroyOnLoad pentru scena ByeBye.");
        }
        else
        {
            Debug.LogWarning($"⚠️ Nu am găsit {persistentObjectName} pentru distrugere.");
        }
    }
}
