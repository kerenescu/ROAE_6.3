using UnityEngine;
using AC;

public class PlayerControlDisabler : MonoBehaviour
{
    public void DisablePlayer()
    {
        // Blochează mișcarea
        KickStarter.playerMovement.LockMovement(true);
        KickStarter.playerInteraction.DeselectHotspot(true);
    }

    public void EnablePlayer()
    {
        // Activează mișcarea
        KickStarter.playerMovement.UnlockMovement();
    }
}
