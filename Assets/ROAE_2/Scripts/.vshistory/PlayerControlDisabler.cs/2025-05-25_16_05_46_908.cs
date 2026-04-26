using UnityEngine;
using AC;

public class PlayerControlDisabler : MonoBehaviour
{
    public void DisablePlayer()
    {
        // Blochează mișcarea playerului
        if (KickStarter.playerMovement != null)
            KickStarter.playerMovement.LockMovement(true);

        // Dezactivează hotspotul curent (deselectează orice obiect activ)
        if (KickStarter.playerInteraction != null)
            KickStarter.playerInteraction.DeselectHotspot(true);
    }

    public void EnablePlayer()
    {
        // Activează din nou mișcarea playerului
        if (KickStarter.playerMovement != null)
            KickStarter.playerMovement.UnlockMovement();
    }
}
