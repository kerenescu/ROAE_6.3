using UnityEngine;
using AC;

public class PlayerControlDisabler : MonoBehaviour
{
    public void DisablePlayer()
    {
        KickStarter.playerInput.activeArrows = false;
        KickStarter.playerInput.enabled = false;
    }

    public void EnablePlayer()
    {
        KickStarter.playerInput.enabled = true;
        KickStarter.playerInput.activeArrows = true;
    }
}
