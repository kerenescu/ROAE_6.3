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

        int creativityBefore = CreativeCore.Instance.creativity;
        int empathyBefore = CreativeCore.Instance.empathy;
        int corruptionBefore = CreativeCore.Instance.plantCorruption;

        Debug.Log(
            "[STATS][StatsEffect.Apply][START] " +
            "deltaCreativity=" + creativity + " " +
            "deltaEmpathy=" + empathy + " " +
            "deltaCorruption=" + plantCorruption + " " +
            "beforeCreativity=" + creativityBefore + " " +
            "beforeEmpathy=" + empathyBefore + " " +
            "beforeCorruption=" + corruptionBefore);

        if (creativity != 0)
        {
            Debug.Log("[STATS][StatsEffect.Apply] applying creativity delta=" + creativity);
            CreativeCore.Instance.AdjustCreativity(creativity);
            hud.ShowStatChange("creativity", creativity);
        }

        if (empathy != 0)
        {
            Debug.Log("[STATS][StatsEffect.Apply] applying empathy delta=" + empathy);
            CreativeCore.Instance.AdjustEmpathy(empathy);
            hud.ShowStatChange("empathy", empathy);
        }

        if (plantCorruption != 0)
        {
            Debug.Log("[STATS][StatsEffect.Apply] applying plantCorruption delta=" + plantCorruption);
            CreativeCore.Instance.AdjustCorruption(plantCorruption);
            hud.ShowStatChange("plantCorruption", plantCorruption);
        }

        Debug.Log(
            "[STATS][StatsEffect.Apply][END] " +
            "afterCreativity=" + CreativeCore.Instance.creativity + " " +
            "afterEmpathy=" + CreativeCore.Instance.empathy + " " +
            "afterCorruption=" + CreativeCore.Instance.plantCorruption);

        CreativeCore.Instance.PrintStats();
    }
}
