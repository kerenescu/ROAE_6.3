using UnityEngine;

public class HotspotSparkleOnHover : MonoBehaviour
{
    public GameObject sparklePrefab; // Prefab-ul de particule
    private GameObject currentSparkle;

    void OnMouseEnter()
    {
        // Instantiem sclipiciul doar o dată la hover
        if (sparklePrefab != null && currentSparkle == null)
        {
            Vector3 pos = transform.position + Vector3.up * 1f;
            currentSparkle = Instantiate(sparklePrefab, pos, Quaternion.identity, transform);
        }
    }

    void OnMouseExit()
    {
        // Distrugem efectul când nu mai e hover
        if (currentSparkle != null)
        {
            Destroy(currentSparkle);
            currentSparkle = null;
        }
    }
}
