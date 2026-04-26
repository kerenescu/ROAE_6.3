using UnityEngine.SceneManagement;
using UnityEngine;

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
        SceneManager.sceneLoaded += TryReassignReferences;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= TryReassignReferences;
    }

    private void TryReassignReferences(Scene scene, LoadSceneMode mode)
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
}
