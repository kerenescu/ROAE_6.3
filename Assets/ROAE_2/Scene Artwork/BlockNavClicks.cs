using UnityEngine;
using AC;

public class MushroomHotspotBlocker : MonoBehaviour
{
    private Hotspot hotspot;

    void Awake()
    {
        hotspot = GetComponent<Hotspot>();
    }

    void OnEnable()
    {
        EventManager.OnHotspotInteract += BlockMovement;
    }

    void OnDisable()
    {
        EventManager.OnHotspotInteract -= BlockMovement;
    }

    void BlockMovement(Hotspot interactedHotspot, AC.Button button)
    {
        if (hotspot != null && interactedHotspot == hotspot)
        {
            // Opreste miscarea jucatorului
            if (KickStarter.player != null)
            {
                KickStarter.player.EndPath();
                Debug.Log("Miscare blocata: ciuperca este doar decor.");
            }

            // Optional: poti anula si interactiunea, daca vrei
            // button.isDisabled = true;
        }
    }
}
