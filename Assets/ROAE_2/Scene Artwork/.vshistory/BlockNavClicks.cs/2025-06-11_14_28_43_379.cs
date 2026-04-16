using UnityEngine;
using AC;

public class BlockNavClick : MonoBehaviour
{
    [Tooltip("Daca e true, clickul pe acest obiect NU va genera miscare.")]
    public bool blockMovement = true;

    void OnMouseDown()
    {
        if (blockMovement && KickStarter.player != null)
        {
            // Opreste orice miscare
            KickStarter.player.EndPath();

            Debug.Log("Click blocat – ciuperca nu declanseaza miscare.");
        }
    }
}
