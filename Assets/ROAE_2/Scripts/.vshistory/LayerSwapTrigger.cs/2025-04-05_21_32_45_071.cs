using UnityEngine;

public class LayerSwapTrigger : MonoBehaviour
{
    [Tooltip("SpriteRenderer-ul pe care vrei să-i schimbi sortingOrder-ul (ex: Player_Graphic)")]
    public SpriteRenderer targetRenderer;

    [Tooltip("Valoarea de sortingOrder când jucătorul e în spatele gardului (sub el)")]
    public int orderInLayerWhenInside = 0;

    [Tooltip("Valoarea de sortingOrder când jucătorul e în fața gardului (deasupra)")]
    public int orderInLayerWhenOutside = 5;

    [Tooltip("Tag-ul obiectului care activează triggerul (de obicei Player)")]
    public string targetTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            Debug.Log("Player a intrat în trigger – în spatele gardului");
            if (targetRenderer != null)
                targetRenderer.sortingOrder = orderInLayerWhenInside;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            Debug.Log("Player a ieșit din trigger – în fața gardului");
            if (targetRenderer != null)
                targetRenderer.sortingOrder = orderInLayerWhenOutside;
        }
    }
}
