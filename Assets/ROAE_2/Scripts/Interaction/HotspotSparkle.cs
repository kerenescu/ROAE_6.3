using UnityEngine;

public class HotspotSparkle : MonoBehaviour
{
    public GameObject sparklePrefab; // setezi în Inspector

    public void TriggerSparkle()
    {
        Vector3 pos = transform.position + Vector3.up * 1f; // sau ajustare în funcție de obiect
        GameObject sparkle = Instantiate(sparklePrefab, pos, Quaternion.identity);
        Destroy(sparkle, 2f); // distruge după ce s-a terminat efectul
    }
}
