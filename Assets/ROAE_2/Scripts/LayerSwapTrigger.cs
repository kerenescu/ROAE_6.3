using UnityEngine;

public class FenceLayerTrigger : MonoBehaviour
{
    public SpriteRenderer playerRenderer;
    public int orderBehind = 5;
    public int orderInFront = 8;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player a intrat în gard – îl punem ÎN SPATE");
            playerRenderer.sortingOrder = orderBehind;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player a ieșit din gard – îl punem ÎN FAȚĂ");
            playerRenderer.sortingOrder = orderInFront;
        }
    }
}
