using UnityEngine;

public class LayerSwapTrigger : MonoBehaviour
{
    public string targetTag = "Player"; // Asigur?-te c? playerul are tag-ul Player
    public SpriteRenderer targetRenderer;
    public int orderInLayerWhenInside = 0;
    public int orderInLayerWhenOutside = 5;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            targetRenderer.sortingOrder = orderInLayerWhenInside;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            targetRenderer.sortingOrder = orderInLayerWhenOutside;
        }
    }
}
