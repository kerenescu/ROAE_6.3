using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;



public class CreativeHUD : MonoBehaviour
{
    public Image creativityBar;
    public Image empathyIcon;
    public Image corruptionIcon;

    [Header("Empathy Colors")]
    public Color empathyNegative = Color.red;
    public Color empathyNeutral = Color.gray;
    public Color empathyPositive = new Color(1f, 0.6f, 0.9f); // roz cald

    [Header("Corruption Gradient")]
    public Color corruptionLow = new Color(0.8f, 1f, 0.8f);  // verde deschis
    public Color corruptionHigh = new Color(0.2f, 0.5f, 0.2f); // verde bolnav

    void Update()
    {
        if (CreativeCore.Instance == null) return;

        // Bara de creativitate
        creativityBar.fillAmount = CreativeCore.Instance.creativity / 100f;

        // Culoare Empatie
        int emp = CreativeCore.Instance.empathy;
        if (emp < 0) empathyIcon.color = empathyNegative;
        else if (emp == 0) empathyIcon.color = empathyNeutral;
        else empathyIcon.color = empathyPositive;

        // Culoare Corupție – interpolare între healthy și toxic
        float t = CreativeCore.Instance.plantCorruption / 5f;
        corruptionIcon.color = Color.Lerp(corruptionLow, corruptionHigh, t);
    }

    public static CreativeHUD Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowStatChange(string statName, int amount)
    {
        string sign = amount > 0 ? "+" : "";
        string statNamePretty = "";

        switch (statName)
        {
            case "creativity": statNamePretty = "Creativitate"; break;
            case "empathy": statNamePretty = "Empatie"; break;
            case "plantCorruption": statNamePretty = "Corupție Plantă"; break;
        }

        string feedback = $"{statNamePretty} {sign}{amount}";


        switch (statName)
        {
            case "creativity":
                StartCoroutine(AnimateFeedback(creativityBar.transform.position, feedback));
                break;
            case "empathy":
                StartCoroutine(AnimateFeedback(empathyIcon.transform.position, feedback));
                break;
            case "plantCorruption":
                StartCoroutine(AnimateFeedback(corruptionIcon.transform.position, feedback));
                break;
        }
    }

    private IEnumerator AnimateFeedback(Vector3 position, string text)
    {
        GameObject feedbackGO = new GameObject("StatFeedback");
        feedbackGO.transform.SetParent(transform);

        TextMeshProUGUI tmp = feedbackGO.AddComponent<TextMeshProUGUI>();
        tmp.material = Resources.Load<Material>("DefaultTMPFontMaterial");
        tmp.font = Resources.Load<TMP_FontAsset>("LiberationSans SDF");
        tmp.color = Color.white;

        tmp.text = text;
        tmp.fontSize = 20; // ➕ Mărim fontul
        //tmp.color = Color.magenta;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        RectTransform rect = tmp.GetComponent<RectTransform>();
        rect.position = position + new Vector3(-250, 50, 0); // 🔧 Mutăm mai la stânga și puțin sus
        rect.localScale = Vector3.one;

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rect.position += Vector3.up * Time.deltaTime * 20;   // mișcare în sus
            tmp.alpha = Mathf.Lerp(1, 0, elapsed / duration);    // fade out
            yield return null;
        }

        Destroy(feedbackGO);
    }


}
