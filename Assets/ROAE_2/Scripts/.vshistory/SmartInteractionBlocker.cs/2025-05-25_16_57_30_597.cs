using UnityEngine;
using AC;
using System.Collections.Generic;

public class SmartInteractionBlocker : MonoBehaviour
{
    private List<Collider2D> blockedColliders = new List<Collider2D>();
    private List<Hotspot> blockedHotspots = new List<Hotspot>();

    /// <summary>
    /// Blochează tot ce are sortingOrder < prag
    /// </summary>
    public void EnterCloseUpWithLayerLimit(int minSortingOrder)
    {
        Time.timeScale = 0f;

        blockedColliders.Clear();
        blockedHotspots.Clear();

        // Dezactivează collider-ele
        foreach (var col in FindObjectsOfType<Collider2D>())
        {
            SpriteRenderer sr = col.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sortingOrder < minSortingOrder && col.enabled)
            {
                col.enabled = false;
                blockedColliders.Add(col);
            }
        }

        // Dezactivează hotspoturile
        foreach (var h in FindObjectsOfType<Hotspot>())
        {
            SpriteRenderer sr = h.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sortingOrder < minSortingOrder && h.enabled)
            {
                h.enabled = false;
                blockedHotspots.Add(h);
            }
        }

        Debug.Log($"🔒 Intrat în Close-Up: Time.timeScale = 0, blocat layere < {minSortingOrder}");
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

        blockedColliders.Clear();
        blockedHotspots.Clear();

        Debug.Log("🔓 Ieșit din Close-Up: Time.timeScale = 1, layere deblocate");
    }

    // Variante pentru AC (pentru SendMessage fără parametri)
    public void EnterLayer40() => EnterCloseUpWithLayerLimit(40);
    public void EnterLayer52() => EnterCloseUpWithLayerLimit(52);
    public void EnterLayer60() => EnterCloseUpWithLayerLimit(60);
}
