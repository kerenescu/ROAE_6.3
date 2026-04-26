using UnityEngine;
using UnityEngine.UI;

public class CreativeHUD : MonoBehaviour
{
    public Image creativityBar;
    public Image empathyIcon;
    public Image corruptionIcon;

    public Color empathyNegative = Color.red;
    public Color empathyNeutral = Color.grey;
    public Color empathyPositive = Color.magenta;

    public Color corruptionLow = new Color(0.7f, 1f, 0.7f);
    public Color corruptionHigh = new Color(0.3f, 0.6f, 0.3f);

    void Update()
    {
        if (CreativeCore.Instance == null) return;

        // Creativitate: 0–100
        creativityBar.fillAmount = CreativeCore.Instance.creativity / 100f;

        // Empatie: -5 la +5
        int emp = CreativeCore.Instance.empathy;
        if (emp < 0) empathyIcon.color = empathyNegative;
        else if (emp == 0) empathyIcon.color = empathyNeutral;
        else empathyIcon.color = empathyPositive;

        // Corupție: 0–5 → interpolăm culoarea
        float t = CreativeCore.Instance.plantCorruption / 5f;
        corruptionIcon.color = Color.Lerp(corruptionLow, corruptionHigh, t);
    }
}
