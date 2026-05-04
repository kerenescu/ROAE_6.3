using UnityEngine;

public class ShardInventory : MonoBehaviour
{
    public static ShardInventory Instance;
    public int collectedShards = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddShard()
    {
        collectedShards++;
        Debug.Log($"🔹 Ciob adunat! Total: {collectedShards}");
    }
}
