using UnityEngine;
using AC;

public class ShardGiver : MonoBehaviour
{
    public string itemName = "Ciob1"; // Numele EXACT ca în Inventar

    public void GiveShardToPlayer()
    {
        KickStarter.runtimeInventory.Add(itemName, 1);
        Debug.Log("Ciob adaugat in inventar!");
    }
}
