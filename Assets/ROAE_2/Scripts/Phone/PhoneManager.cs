using UnityEngine;

public class PhoneManager : MonoBehaviour
{
    public static PhoneManager Instance;

    [Header("UI References")]
    //public GameObject phoneClosedImage;
    public GameObject phoneOpenUI;
    public GameObject phoneButton_Open;
    public GameObject phoneButton_Close;
    public GameObject[] phonePages; // pagini ca în jurnal dacă ai mai multe secțiuni

    private int currentPage = 0;
    private bool isOpen = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ClosePhoneCompletely();
    }

    public void TogglePhone()
    {
        if (isOpen)
            ClosePhoneCompletely();
        else
            OpenPhone();
    }

    public void OpenPhone()
    {
        isOpen = true;
        //phoneClosedImage.SetActive(false);
        phoneOpenUI.SetActive(true);
        phoneButton_Open.SetActive(false);
        phoneButton_Close.SetActive(true);

        ShowPage(currentPage);

        Time.timeScale = 0f;
    }

    public void ClosePhoneCompletely()
    {
        isOpen = false;
        //phoneClosedImage.SetActive(true);
        phoneOpenUI.SetActive(false);
        phoneButton_Open.SetActive(true);
        phoneButton_Close.SetActive(false);

        Time.timeScale = 1f;
    }

    public void ShowPage(int index)
    {
        for (int i = 0; i < phonePages.Length; i++)
        {
            phonePages[i].SetActive(i == index);
        }
    }

    public void NextPage()
    {
        currentPage++;
        if (currentPage >= phonePages.Length)
            currentPage = 0;

        ShowPage(currentPage);
    }

    public void PrevPage()
    {
        currentPage--;
        if (currentPage < 0)
            currentPage = phonePages.Length - 1;

        ShowPage(currentPage);
    }
}
