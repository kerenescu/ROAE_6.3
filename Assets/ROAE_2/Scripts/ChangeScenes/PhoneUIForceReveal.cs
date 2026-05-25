using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhoneUIForceReveal : MonoBehaviour
{
    private static PhoneUIForceReveal instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
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

    private IEnumerator Start()
    {
        yield return ForceRevealNextFrame();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ForceRevealNextFrame());
    }

    private IEnumerator ForceRevealNextFrame()
    {
        yield return null;

        RepairNamedRoot("UIContainer");
        RepairNamedRoot("InGameUI");
        RepairNamedRoot("InventoryUI");

        PhoneUIFlow phone = FindObjectOfType<PhoneUIFlow>();
        if (phone != null)
        {
            GameObject phoneGO = phone.GetInterfataVizuala();
            if (phoneGO != null)
            {
                RepairRectTransform(phoneGO.GetComponent<RectTransform>());
                foreach (CanvasGroup cg in phoneGO.GetComponentsInChildren<CanvasGroup>(true))
                {
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }

                foreach (Image img in phoneGO.GetComponentsInChildren<Image>(true))
                {
                    Color color = img.color;
                    color.a = 1f;
                    img.color = color;
                }

                foreach (Button btn in phoneGO.GetComponentsInChildren<Button>(true))
                {
                    btn.interactable = true;
                    btn.gameObject.SetActive(true);
                }
            }
        }

        bool phoneOpen = phone != null && phone.IsPhoneOpen();
        SetNamedActive("PhoneButton_Open", !phoneOpen);
        SetNamedActive("PhoneButton_Close", phoneOpen);

        bool journalOpen = JournalUIFlow.Instance != null && JournalUIFlow.Instance.IsJournalOpen();
        SetNamedActive("JurnalButton_Open", !journalOpen);
        SetNamedActive("JurnalButton_Close", journalOpen);
        SetNamedActive("StatsUI", true);
    }

    private static void RepairNamedRoot(string objectName)
    {
        GameObject root = GameObject.Find(objectName);
        if (root == null)
            return;

        root.SetActive(true);
        root.transform.localScale = Vector3.one;
        RectTransform rectTransform = root.GetComponent<RectTransform>();
        RepairRectTransform(rectTransform);
    }

    private static void SetNamedActive(string objectName, bool isActive)
    {
        GameObject go = GameObject.Find(objectName);
        if (go == null)
            return;

        go.transform.localScale = Vector3.one;
        go.SetActive(isActive);
    }

    private static void RepairRectTransform(RectTransform rt)
    {
        if (rt == null)
            return;

        rt.anchoredPosition = Vector2.zero;
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one;
    }
}
