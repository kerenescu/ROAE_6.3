using System.Collections;
using UnityEngine;

public class PlayerSpawnFromBarInteriorDebugger : MonoBehaviour
{
    public string expectedScene = "Bar_Interior";
    public string markerName = "IntoarcereDinBarInterior";

    void Start()
    {
        StartCoroutine(DelayedCheck());
    }

    IEnumerator DelayedCheck()
    {
        yield return new WaitForSeconds(0.2f);

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

                    if (dist > 2f)
                    {
                        Debug.LogWarning("[FIX] Mutez playerul forțat pe marker!");
                        player.transform.position = marker.transform.position;
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
                Debug.Log("[INFO] Nu am venit din 'Bar_Interior' — nu testăm plasarea pe marker.");
            }
        }
        else
        {
            Debug.LogError("[EROARE] Playerul NU a fost găsit în scenă după întârziere!");
        }
    }
}
