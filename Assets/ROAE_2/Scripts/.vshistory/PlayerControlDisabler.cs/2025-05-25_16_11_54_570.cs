using UnityEngine;
using AC;

public class PlayerControlDisabler : MonoBehaviour
{
    public void DisablePlayer()
    {
        // Oprește orice input de la jucător
        if (KickStarter.playerInput != null)
            KickStarter.playerInput.enabled = false;

        // Blochează mișcarea (precauție suplimentară)
        if (KickStarter.playerMovement != null)
            KickStarter.playerMovement.LockMovement(true);

        // Deselectează orice hotspot curent
        if (KickStarter.playerInteraction != null)
            KickStarter.playerInteraction.DeselectHotspot(true);

        Debug.Log("🔒 Player control DISABLED");
    }

    public void EnablePlayer()
    {
        // Reactivăm input-ul
        if (KickStarter.playerInput != null)
            KickStarter.playerInput.enabled = true;

        // De-blochează mișcarea
        if (KickStarter.playerMovement != null)
            KickStarter.playerMovement.UnlockMovement();

        Debug.Log("🔓 Player control ENABLED");
    }
}
