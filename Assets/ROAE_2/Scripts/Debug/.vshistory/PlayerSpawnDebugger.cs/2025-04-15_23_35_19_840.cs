using UnityEngine;

public class PlayerSpawnDebugger : MonoBehaviour
{
    public string expectedScene = "Anticariat";
    public string markerName = "IntoarcereDinAnticariat";

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            Vector3 pos = player.transform.position;
            Debug.Log($"[DEBUG] Player s-a spawnat la poziția: {pos}");

            string previousScene = AC.KickStarter.sceneChanger.GetPreviousSceneName();
            Debug.Log($"[DEBUG] Scena anterioară: {previousScene}");

            if (previousScene == expectedScene)
            {
                GameObject marker = GameObject.Find(markerName);
                if (marker != null)
                {
                    Debug.Log($"[DEBUG] Marker '{markerName}' este prezent în scenă la poziția: {marker.transform.position}");

                    float dist = Vector3.Distance(player.transform.position, marker.transform.position);
                    Debug.Log($"[DEBUG] Distanța dintre player și marker: {dist}");

                    if (dist > 1f)
                    {
                        Debug.LogWarning("[ALERTĂ] Playerul NU a fost plasat corect la marker!");
                    }
                    else
                    {
                        Debug.Log("[SUCCESS] Playerul este corect poziționat la marker!");
                    }
                }
                else
                {
                    Debug.LogError($"[EROARE] Markerul '{markerName}' NU a fost găsit în scenă!");
                }
            }
            else
            {
                Debug.Log("[INFO] Nu am venit din 'Anticariat' — nu testăm plasarea pe marker.");
            }
        }
        else
        {
            Debug.LogError("[EROARE] Playerul NU a fost găsit în scenă!");
        }
    }
}
