using UnityEngine;

public class ClickSpawner : MonoBehaviour
{
    [Header("Prefab de spawnat")]
    public GameObject objectToSpawn;

    [Tooltip("Offset față de poziția acestui obiect")]
    public Vector3 spawnOffset = Vector3.zero;

    [Tooltip("Distruge automat după x secunde (0 = nu distruge)")]
    public float autoDestroyTime = 0f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(worldPos.x, worldPos.y);

            Collider2D hit = Physics2D.OverlapPoint(clickPos);

            if (hit != null && hit.gameObject == gameObject)
            {
                Spawn();
            }
        }
    }

    private void Spawn()
    {
        if (objectToSpawn != null)
        {
            GameObject spawned = Instantiate(objectToSpawn, transform.position + spawnOffset, Quaternion.identity);

            if (autoDestroyTime > 0f)
            {
                Destroy(spawned, autoDestroyTime);
            }

            Debug.Log($"✨ S-a spawnat {objectToSpawn.name} la click pe {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ objectToSpawn nu e setat în ClickSpawner");
        }
    }
}
