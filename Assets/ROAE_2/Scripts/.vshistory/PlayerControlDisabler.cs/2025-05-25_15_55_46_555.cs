using UnityEngine;
using AC;

public class PlayerControlDisabler : MonoBehaviour
{
    public void DisablePlayer()
    {
        // Blocheaz? interac?iunile cu Hotspoturile
        KickStarter.playerInteraction.DeselectHotspot(true);
        KickStarter.playerInteraction.DisableHotspotInteraction();

        // Blocheaz? mi?carea
        KickStarter.playerMovement.LockMovement(true);
        KickStarter.playerMovement.UnlockCursor(); // op?ional
    }

    public void EnablePlayer()
    {
        // Activeaz? interac?iunile
        KickStarter.playerInteraction.EnableHotspotInteraction();

        // Activeaz? mi?carea
        KickStarter.playerMovement.UnlockMovement();
    }
}
