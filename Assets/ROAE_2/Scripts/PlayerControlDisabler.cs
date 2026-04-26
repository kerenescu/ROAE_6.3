using UnityEngine;

public class LayerClickBlocker : MonoBehaviour
{
    private Collider2D[] collidersToToggle;

    public void DisableClickOnLowerLayers()
    {
        SpriteRenderer[] sprites = FindObjectsOfType<SpriteRenderer>();
        var tempList = new System.Collections.Generic.List<Collider2D>();

        foreach (SpriteRenderer sr in sprites)
        {
            if (sr.sortingOrder < 30)
            {
                Collider2D col = sr.GetComponent<Collider2D>();
                if (col != null && col.enabled)
                {
                    tempList.Add(col);
                    col.enabled = false;
                }
            }
        }

        collidersToToggle = tempList.ToArray();
        Debug.Log("🛑 Click blocat pe toate layerele sub 30");
    }

    public void EnableClickOnLowerLayers()
    {
        if (collidersToToggle == null) return;

        foreach (Collider2D col in collidersToToggle)
        {
            if (col != null)
                col.enabled = true;
        }

        Debug.Log("✅ Click reactivat pe layerele sub 30");
    }
}
