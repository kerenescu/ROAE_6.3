using UnityEngine;
using AC;

public class HotspotManager : MonoBehaviour
{
    private Hotspot[] cachedHotspots;

    public void DisableAllHotspots()
    {
        Hotspot[] allHotspots = FindObjectsOfType<Hotspot>();
        var hotspotList = new System.Collections.Generic.List<Hotspot>();

        foreach (Hotspot h in allHotspots)
        {
            SpriteRenderer sr = h.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sortingOrder < 40 && h.enabled)
            {
                h.enabled = false;
                hotspotList.Add(h);
            }
        }

        cachedHotspots = hotspotList.ToArray();
        Debug.Log("🔕 Hotspoturi dezactivate pe layere < 40");
    }

    public void EnableAllHotspots()
    {
        if (cachedHotspots == null) return;

        foreach (Hotspot h in cachedHotspots)
        {
            if (h != null)
                h.enabled = true;
        }

        Debug.Log("🔔 Hotspoturi reactivate");
    }
}
