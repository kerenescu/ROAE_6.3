using UnityEngine;
using AC;

public class SmartInteractionBlocker : MonoBehaviour
{
    private Collider2D[] blockedColliders;
    private Hotspot[] blockedHotspots;

    public void EnterCloseUp()
    {
        Time.timeScale = 0f;

        // Dezactivează colliderele de pe layere sub 40
        var colliders = FindObjectsOfType<Collider2D>();
        var colliderList = new System.Collections.Generic.List<Collider2D>();

        foreach (var col in colliders)
        {
            SpriteRenderer sr = col.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sortingOrder < 40 && col.enabled)
            {
                colliderList.Add(col);
                col.enabled = false;
            }
        }

        blockedColliders = colliderList.ToArray();

        // Dezactivează hotspoturile de pe layere sub 40
        var hotspots = FindObjectsOfType<Hotspot>();
        var hotspotList = new System.Collections.Generic.List<Hotspot>();

        foreach (var h in hotspots)
        {
            SpriteRenderer sr = h.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sortingOrder < 40 && h.enabled)
            {
                hotspotList.Add(h);
                h.enabled = false;
            }
        }

        blockedHotspots = hotspotList.ToArray();

        Debug.Log("🔒 Intrat în Close-Up: Time.timeScale = 0, blocat layere < 40");
    }

    public void ExitCloseUp()
    {
        Time.timeScale = 1f;

        foreach (var col in blockedColliders)
        {
            if (col != null)
                col.enabled = true;
        }

        foreach (var h in blockedHotspots)
        {
            if (h != null)
                h.enabled = true;
        }

        Debug.Log("🔓 Ieșit din Close-Up: Time.timeScale = 1, layere deblocate");
    }
}
