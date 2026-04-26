using UnityEngine;
using UnityEngine.UI;

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
}
