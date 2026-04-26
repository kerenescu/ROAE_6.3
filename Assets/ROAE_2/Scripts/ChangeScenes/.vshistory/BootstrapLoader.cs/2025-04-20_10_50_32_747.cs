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


// === PhoneBootstrapper.cs ===
using UnityEngine;

public class PhoneBootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject phoneSystemPrefab;
    private static bool isPhoneSystemInstantiated = false;

    void Awake()
    {
        if (!isPhoneSystemInstantiated)
        {
            GameObject phoneInstance = Instantiate(phoneSystemPrefab);
            DontDestroyOnLoad(phoneInstance);
            isPhoneSystemInstantiated = true;
            Debug.Log("✅ PhoneSystem instantiat si marcat ca DontDestroyOnLoad");
        }
        Destroy(this.gameObject);
    }
}


// === PhoneUIFlow.cs ===
using UnityEngine;

public class PhoneUIFlow : MonoBehaviour
{
    public GameObject phoneUI;
    public GameObject phoneButton_Open;
    public GameObject phoneButton_Close;

    private void Start()
    {
        ValidateRefs();
        SafeClosePhone();
    }

    public void OnPhoneButtonPressed() => SafeOpenPhone();
    public void OnCloseButtonPressed() => SafeClosePhone();

    private void SafeOpenPhone()
    {
        if (!ValidateRefs()) return;
        phoneUI.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);
        Time.timeScale = 0f;
    }

    private void SafeClosePhone()
    {
        if (!ValidateRefs()) return;
        phoneUI.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
        Time.timeScale = 1f;
    }

    private bool ValidateRefs()
    {
        if (phoneUI == null || phoneButton_Open == null || phoneButton_Close == null)
        {
            Debug.LogError("❌ PhoneUIFlow: Referinte lipsa la UI!");
            return false;
        }
        return true;
    }
}


// === MessageManager.cs ===
using UnityEngine;
using System.Collections.Generic;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance;
    public static List<PhoneConversation> persistentConversations = new();
    public List<PhoneConversation> conversations = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            if (persistentConversations != null && persistentConversations.Count > 0)
            {
                conversations = new List<PhoneConversation>(persistentConversations);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            persistentConversations = conversations;
        }
    }

    private void Start()
    {
        if (conversations == null || conversations.Count == 0)
        {
            LoadMessagesFromJSON();
        }

        ShowConversations();
    }

    public void LoadMessagesFromJSON()
    {
        // codul tau de incarcare JSON
    }

    public void ShowConversations()
    {
        // codul tau de afisare conversatii
    }
}


// === Setari proiect ===
// 1. Creezi scena "BootstrapScene".
// 2. In ea: 
//    - GameObject "PhoneBootstrapper" cu scriptul PhoneBootstrapper si setat prefab-ul PhoneSystem.
//    - GameObject "BootstrapLoader" cu scriptul BootstrapLoader si numele scenei initiale.
// 3. In Build Settings, setezi "BootstrapScene" ca prima scena (index 0).
