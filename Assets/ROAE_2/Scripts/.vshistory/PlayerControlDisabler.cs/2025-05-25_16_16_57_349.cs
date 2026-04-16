using UnityEngine;
using AC;

public class PlayerControlDisabler : MonoBehaviour
{
    public void DisablePlayer()
    {
        if (KickStarter.playerMovement != null)
        {
            KickStarter.playerMovement.SetPath(null); // oprește orice mișcare curentă
            KickStarter.playerMovement.lockMovement = true; // blochează input pe NavMesh
        }

        if (KickStarter.playerInteraction != null)
        {
            KickStarter.playerInteraction.DeselectHotspot(true); // oprește interacțiunea cu obiecte
        }

        Debug.Log("🛑 Click pe NavMesh dezactivat.");
    }

    public void EnablePlayer()
    {
        if (KickStarter.playerMovement != null)
        {
            KickStarter.playerMovement.lockMovement = false; // reactivăm click-ul pe NavMesh
        }

        Debug.Log("✅ Click pe NavMesh reactivat.");
    }
}
