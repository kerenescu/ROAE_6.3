using UnityEngine;
using AC;

public class StopNavClickOnMushroom : MonoBehaviour
{
    public LayerMask mushroomLayer;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, mushroomLayer))
            {
                // A dat click pe ciuperca → blocheaza deplasarea
                if (KickStarter.player != null)
                {
                    KickStarter.player.EndPath();
                    Debug.Log("Click interceptat pe ciuperca. Nu ne miscam!");
                }
            }
        }
    }
}
