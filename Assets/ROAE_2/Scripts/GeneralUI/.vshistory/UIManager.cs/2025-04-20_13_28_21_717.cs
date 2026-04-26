using UnityEngine;
using UnityEngine.SceneManagement;

public class UIContainerManager : MonoBehaviour
{
    public static UIContainerManager Instance;

    [Header("UI Elements")]
    public GameObject phoneUI;
    public GameObject journalUI;
    public GameObject statsUI;

    [Header("Toggle Buttons")]
    public GameObject phoneButton_Open;
    public GameObject phoneButton_Close;
    public GameObject journalButton_Open;
    public GameObject journalButton_Close;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        TryReassignReferences();

        //if (scene.name != "BootstrapScene") // ⛔ evită în Bootstrap
        //    CloseAllUI();
    }


    private void TryReassignReferences()
    {
        if (phoneButton_Open == null)
            phoneButton_Open = GameObject.Find("PhoneButton_Open");

        if (phoneButton_Close == null)
            phoneButton_Close = GameObject.Find("PhoneButton_Close");

        if (journalButton_Open == null)
            journalButton_Open = GameObject.Find("JurnalButton_Open");

        if (journalButton_Close == null)
            journalButton_Close = GameObject.Find("JurnalButton_Close");
    }

    public void CloseAllUI()
    {
        if (phoneUI != null) phoneUI.SetActive(false);
        if (journalUI != null) journalUI.SetActive(false);
        if (statsUI != null) statsUI.SetActive(false);
    }

    public void TogglePhone(bool state)
    {
        if (phoneUI != null) phoneUI.SetActive(state);
        if (phoneButton_Open != null) phoneButton_Open.SetActive(!state);
        if (phoneButton_Close != null) phoneButton_Close.SetActive(state);
    }

    public void ToggleJournal(bool state)
    {
        if (journalUI != null) journalUI.SetActive(state);
        if (journalButton_Open != null) journalButton_Open.SetActive(!state);
        if (journalButton_Close != null) journalButton_Close.SetActive(state);
    }

    public void ToggleStats(bool state)
    {
        if (statsUI != null) statsUI.SetActive(state);
    }
}
