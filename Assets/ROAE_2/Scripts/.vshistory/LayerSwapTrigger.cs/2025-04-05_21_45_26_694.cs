using UnityEngine;

public class FenceLayerTrigger : MonoBehaviour
{
    public SpriteRenderer playerRenderer;
    public int orderBehind = 0;
    public int orderInFront = 5;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerRenderer.sortingOrder = orderBehind;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerRenderer.sortingOrder = orderInFront;
        }
    }
}
