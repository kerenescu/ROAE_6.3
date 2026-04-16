using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public static PhoneManager Instance;

    [Header("Panels")]
    public GameObject panelHomeMenu;
    public GameObject panelCamera;
    public GameObject panelGallery;
    public GameObject panelNotes;
    public GameObject panelGame;

    [Header("Managers")]
    public MessageManager messageManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // evită duplicarea
        }
    }

    void Start()
    {
        BackToMenu(); // Pornește din meniul principal
    }

    // restul codului rămâne neschimbat


    public void OpenMessages()
    {
        CloseAllWithoutTouchingMessages();

        if (messageManager != null && !messageManager.phoneUI.activeSelf)
        {
            messageManager.TogglePhone();
            messageManager.ShowConversations();
        }

    }

    public void OpenCamera()
    {
        CloseAll();
        panelCamera?.SetActive(true);
    }

    public void OpenGallery()
    {
        CloseAll();
        panelGallery?.SetActive(true);
    }

    public void OpenNotes()
    {
        CloseAll();
        panelNotes?.SetActive(true);
    }

    public void OpenGame()
    {
        CloseAll();
        panelGame?.SetActive(true);
    }

    public void BackToMenu()
    {
        CloseAll();
        panelHomeMenu?.SetActive(true);
    }

    private void CloseAll()
    {
        panelHomeMenu?.SetActive(false);
        panelCamera?.SetActive(false);
        panelGallery?.SetActive(false);
        panelNotes?.SetActive(false);
        panelGame?.SetActive(false);

        // Dacă MessageManager e activ, îl închidem
        if (messageManager != null && messageManager.phoneUI.activeSelf)
        {
            messageManager.TogglePhone();
        }
    }

    private void CloseAllWithoutTouchingMessages()
    {
        panelHomeMenu?.SetActive(false);
        panelCamera?.SetActive(false);
        panelGallery?.SetActive(false);
        panelNotes?.SetActive(false);
        panelGame?.SetActive(false);
        // nu închidem phoneUI aici!
    }

}
