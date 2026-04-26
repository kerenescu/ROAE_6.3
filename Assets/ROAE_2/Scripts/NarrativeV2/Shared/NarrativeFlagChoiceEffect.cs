using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NarrativeFlagChoiceEffect", menuName = "ROAE/Narrative/Flag Choice Effect")]
public class NarrativeFlagChoiceEffect : DialogueChoiceEffect
{
    [SerializeField] private string flagKey = "";
    [SerializeField] private bool flagValue = true;
    [SerializeField] private List<string> clearFlagKeys = new List<string>();
    [SerializeField] private bool debugLogs = true;

    public override void Apply()
    {
        for (int i = 0; i < clearFlagKeys.Count; i++)
        {
            string clearKey = clearFlagKeys[i];
            if (string.IsNullOrWhiteSpace(clearKey))
                continue;

            PlayerPrefs.SetInt(clearKey, 0);
        }

        if (!string.IsNullOrWhiteSpace(flagKey))
            PlayerPrefs.SetInt(flagKey, flagValue ? 1 : 0);

        PlayerPrefs.Save();

        if (!debugLogs)
            return;

        Debug.Log(
            "[ROAE][NarrativeFlagChoiceEffect] flagKey=" + flagKey +
            " value=" + flagValue +
            " cleared=" + string.Join(",", clearFlagKeys));
    }
}
