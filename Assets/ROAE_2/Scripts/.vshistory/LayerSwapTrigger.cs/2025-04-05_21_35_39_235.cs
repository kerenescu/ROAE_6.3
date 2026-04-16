using UnityEngine;

public class LayerSwapTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Cineva a intrat în trigger: " + other.name);
    }
}
