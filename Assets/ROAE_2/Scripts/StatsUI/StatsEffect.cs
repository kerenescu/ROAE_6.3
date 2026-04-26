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
        CreativeCore core = CreativeCore.Instance ?? Object.FindFirstObjectByType<CreativeCore>();
        if (core == null)
        {
            Debug.LogWarning("[STATS][StatsEffect.Apply] CreativeCore missing. Effect skipped.");
            return;
        }

        var hud = CreativeHUD.Instance;

        int creativityBefore = core.creativity;
        int empathyBefore = core.empathy;
        int corruptionBefore = core.plantCorruption;

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
            core.AdjustCreativity(creativity);
            if (hud != null) hud.ShowStatChange("creativity", creativity);
        }

        if (empathy != 0)
        {
            Debug.Log("[STATS][StatsEffect.Apply] applying empathy delta=" + empathy);
            core.AdjustEmpathy(empathy);
            if (hud != null) hud.ShowStatChange("empathy", empathy);
        }

        if (plantCorruption != 0)
        {
            core.AdjustCorruption(plantCorruption);
            if (hud != null) hud.ShowStatChange("plantCorruption", plantCorruption);
        }

        Debug.Log(
            "[STATS][StatsEffect.Apply][END] " +
            "afterCreativity=" + core.creativity + " " +
            "afterEmpathy=" + core.empathy + " " +
            "afterCorruption=" + core.plantCorruption);

        core.PrintStats();
    }
}
