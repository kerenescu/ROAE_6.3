using UnityEngine;
using AC;

public class PlayerControlDisabler : MonoBehaviour
{
    public void DisablePlayer()
    {
        KickStarter.stateHandler.gameState = GameState.Cutscene;
    }

    public void EnablePlayer()
    {
        KickStarter.stateHandler.gameState = GameState.Normal;
    }
}
