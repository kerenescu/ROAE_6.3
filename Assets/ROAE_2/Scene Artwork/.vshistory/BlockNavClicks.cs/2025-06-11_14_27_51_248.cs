using UnityEngine;
using AC;

public class BlockNavClick : MonoBehaviour
{
    [Tooltip("Daca e true, clickul pe acest obiect NU va genera miscare.")]
    public bool blockMovement = true;

    void OnMouseDown()
    {
        if (blockMovement && KickStarter.playerMovement != null)
        {
            // Opreste deplasarea
            KickStarter.playerMovement.StopMovingTo();

            // Optional: executa o alta actiune
            Debug.Log("Click blocat pentru miscare – ciuperca ignorata.");
        }
    }
}
