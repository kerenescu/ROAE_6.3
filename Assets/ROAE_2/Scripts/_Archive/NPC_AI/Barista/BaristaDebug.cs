using UnityEngine;

public static class BaristaDebug
{
    public static bool Enabled = true;

    public static void Log(string source, string message)
    {
        if (!Enabled)
            return;

        Debug.Log("[ROAE][BARISTA][" + source + "] " + message);
    }

    public static void Warn(string source, string message)
    {
        if (!Enabled)
            return;

        Debug.LogWarning("[ROAE][BARISTA][" + source + "] " + message);
    }

    public static void Error(string source, string message)
    {
        if (!Enabled)
            return;

        Debug.LogError("[ROAE][BARISTA][" + source + "] " + message);
    }
}
