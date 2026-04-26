using UnityEngine;

public class FenceLayerTrigger : MonoBehaviour
{
    public SpriteRenderer playerRenderer;
    public int orderBehind = 5;
    public int orderInFront = 8;

    private void Awake()
    {
        if (playerRenderer == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerRenderer = player.GetComponentInChildren<SpriteRenderer>();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (playerRenderer == null)
        {
            playerRenderer = other.GetComponentInChildren<SpriteRenderer>();
            if (playerRenderer == null) return;
        }

        Debug.Log("Player entered fence - set behind");
        playerRenderer.sortingOrder = orderBehind;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (playerRenderer == null)
        {
            playerRenderer = other.GetComponentInChildren<SpriteRenderer>();
            if (playerRenderer == null) return;
        }

        Debug.Log("Player exited fence - set in front");
        playerRenderer.sortingOrder = orderInFront;
    }
}