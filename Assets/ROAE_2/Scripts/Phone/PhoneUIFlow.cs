using UnityEngine;
using System.Collections;

public class PhoneUIFlow : MonoBehaviour
{
    public static PhoneUIFlow Instance;

    [SerializeField] private GameObject interfataVizuala;
    [SerializeField] private GameObject phoneButton_Open;
    [SerializeField] private GameObject phoneButton_Close;
    [SerializeField] private MessageManager messageManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }
    }

    private void Start()
    {
        this.gameObject.SetActive(true);

        Debug.Log("📱 PhoneUIFlow START");
        Debug.Log("🧩 interfataVizuala: " + (interfataVizuala == null ? "NULL" : "OK"));
        Debug.Log("🧩 phoneButton_Open: " + (phoneButton_Open == null ? "NULL" : "OK"));
        Debug.Log("🧩 phoneButton_Close: " + (phoneButton_Close == null ? "NULL" : "OK"));
        Debug.Log("🧩 messageManager: " + (messageManager == null ? "NULL" : "OK"));

        StartCoroutine(InitialUIState());
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Alpha1))
            return;

        if (IsPhoneOpen())
        {
            OnCloseButtonPressed();
            return;
        }

        if (JournalUIFlow.Instance != null && JournalUIFlow.Instance.IsJournalOpen())
        {
            JournalUIFlow.Instance.CloseJournalCompletely();
        }

        OnPhoneButtonPressed();
    }

    private IEnumerator InitialUIState()
    {
        yield return null; // așteaptă un frame ca toate referințele și componentele să fie gata

        if (interfataVizuala != null) interfataVizuala.SetActive(false);
        if (phoneButton_Open != null) phoneButton_Open.SetActive(true);
        if (phoneButton_Close != null) phoneButton_Close.SetActive(false);
    }



    public GameObject GetInterfataVizuala()
    {
        return interfataVizuala;
    }

    public bool IsPhoneOpen()
    {
        return interfataVizuala != null && interfataVizuala.activeSelf;
    }


    public void OnPhoneButtonPressed()
    {
        if (JournalUIFlow.Instance != null && JournalUIFlow.Instance.IsJournalOpen())
        {
            JournalUIFlow.Instance.CloseJournalCompletely();
        }

        Time.timeScale = 0f; 

        interfataVizuala.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);
    }

    public void OnCloseButtonPressed()
    {
        Time.timeScale = 1f;

        interfataVizuala.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);
        messageManager.ShowConversations();  // Revine la inbox
        messageManager.HideDecisionPanel();

        if (CaptionUIManager.Instance != null)
        {
            CaptionUIManager.Instance.ForceHideCaption();  // Ascunde gândul instant
        }

    }
}
