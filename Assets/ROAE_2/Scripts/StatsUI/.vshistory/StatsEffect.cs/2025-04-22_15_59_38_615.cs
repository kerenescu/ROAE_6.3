using UnityEngine;
using System.Collections;

[System.Serializable]
public class StatsEffect
{
    public int creativity;
    public int empathy;
    public int plantCorruption;

    public void Apply()
    {
        var hud = CreativeHUD.Instance;

        if (creativity != 0)
        {
            CreativeCore.Instance.AdjustCreativity(creativity);
            hud.ShowStatChange("creativity", creativity);
        }

        if (empathy != 0)
        {
            CreativeCore.Instance.AdjustEmpathy(empathy);
            hud.ShowStatChange("empathy", empathy);
        }

        if (plantCorruption != 0)
        {
            CreativeCore.Instance.AdjustCorruption(plantCorruption);
            hud.ShowStatChange("plantCorruption", plantCorruption);
        }

        CreativeCore.Instance.PrintStats();
    }

}
