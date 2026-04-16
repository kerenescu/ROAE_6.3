using UnityEngine;
using AC;

public class HotspotManager : MonoBehaviour
{
    private Hotspot[] cachedHotspots;

    public void DisableAllHotspots()
    {
        cachedHotspots = FindObjectsOfType<Hotspot>();
        foreach (Hotspot h in cachedHotspots)
        {
            // Exclude hotspoturile din RaftCloseUpGood
            if (!h.transform.IsChildOf(GameObject.Find("RaftCloseUpGood").transform))
                h.enabled = false;
        }
    }

    public void EnableAllHotspots()
    {
        if (cachedHotspots == null) return;
        foreach (Hotspot h in cachedHotspots)
        {
            h.enabled = true;
        }
    }
}
