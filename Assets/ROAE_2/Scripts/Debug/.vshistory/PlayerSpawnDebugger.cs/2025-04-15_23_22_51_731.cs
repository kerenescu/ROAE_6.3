using UnityEngine;

public class PlayerSpawnDebugger : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            Debug.Log("[DEBUG] Player s-a spawnat la poziția: " + pos);

            if (pos.magnitude < 0.1f)
            {
                Debug.LogWarning("[ALERTĂ] Playerul a fost plasat aproape de (0,0) — posibil fallback/default spawn. Markerul nu a fost folosit corect?");
            }
        }
        else
        {
            Debug.LogError("[EROARE] Playerul NU a fost găsit în scenă!");
        }
    }
}
