using UnityEngine;

[System.Serializable]
public class StatsEffect
{
    public int creativity;
    public int empathy;
    public int plantCorruption;

    public void Apply()
    {
        if (CreativeCore.Instance == null)
        {
            Debug.LogWarning("CreativeCore nu a fost găsit!");
            return;
        }

        CreativeCore.Instance.AdjustCreativity(creativity);
        CreativeCore.Instance.AdjustEmpathy(empathy);
        CreativeCore.Instance.AdjustCorruption(plantCorruption);
        CreativeCore.Instance.PrintStats();
    }
}
