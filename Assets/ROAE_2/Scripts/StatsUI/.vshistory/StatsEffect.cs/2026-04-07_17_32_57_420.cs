using System.Diagnostics;
using UnityEngine;

[System.Serializable]
public class StatsEffect
{
    public int creativity;
    public int empathy;
    public int plantCorruption;
    public string debugSource;

    public void Apply()
    {
        if (CreativeCore.Instance == null)
        {
            BaristaDebug.Warn("StatsEffect.Apply", "CreativeCore.Instance is null source=" + ResolveSource());
            return;
        }

        int creativityBefore = CreativeCore.Instance.creativity;
        int empathyBefore = CreativeCore.Instance.empathy;
        int corruptionBefore = CreativeCore.Instance.plantCorruption;

        var hud = CreativeHUD.Instance;

        if (creativity != 0)
        {
            CreativeCore.Instance.AdjustCreativity(creativity);
            if (hud != null)
                hud.ShowStatChange("creativity", creativity);
        }

        if (empathy != 0)
        {
            CreativeCore.Instance.AdjustEmpathy(empathy);
            if (hud != null)
                hud.ShowStatChange("empathy", empathy);
        }

        if (plantCorruption != 0)
        {
            CreativeCore.Instance.AdjustCorruption(plantCorruption);
            if (hud != null)
                hud.ShowStatChange("plantCorruption", plantCorruption);
        }

        BaristaDebug.Log(
            "StatsEffect.Apply",
            "source=" + ResolveSource() +
            " delta(creativity=" + creativity + ", empathy=" + empathy + ", plantCorruption=" + plantCorruption + ")" +
            " before(creativity=" + creativityBefore + ", empathy=" + empathyBefore + ", plantCorruption=" + corruptionBefore + ")" +
            " after(creativity=" + CreativeCore.Instance.creativity + ", empathy=" + CreativeCore.Instance.empathy + ", plantCorruption=" + CreativeCore.Instance.plantCorruption + ")");

        CreativeCore.Instance.PrintStats();
    }

    private string ResolveSource()
    {
        if (!string.IsNullOrEmpty(debugSource))
            return debugSource;

        StackTrace trace = new StackTrace();
        if (trace.FrameCount > 2)
        {
            var method = trace.GetFrame(2).GetMethod();
            if (method != null && method.DeclaringType != null)
                return method.DeclaringType.Name + "." + method.Name;
        }

        return "unknown";
    }
}
