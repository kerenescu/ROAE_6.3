using UnityEngine;
using AC;

public class PlayerControlDisabler : MonoBehaviour
{
    public void DisablePlayer()
    {
        KickStarter.playerInput.inputIsEnabled = false;
        KickStarter.playerInteraction.DisableHotspotInteraction();
        KickStarter.playerMovement.IsLocked = true;
    }

    public void EnablePlayer()
    {
        KickStarter.playerInput.inputIsEnabled = true;
        KickStarter.playerInteraction.EnableHotspotInteraction();
        KickStarter.playerMovement.IsLocked = false;
    }
}
