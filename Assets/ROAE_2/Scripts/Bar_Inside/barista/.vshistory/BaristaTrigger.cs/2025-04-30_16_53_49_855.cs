using UnityEngine;

public class BaristaTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData startingDialogue;
    [SerializeField] private GameObject fallbackLocalPlayer; // doar pt testare

    private GameObject player;
    private bool triggered = false;

    private void Start()
    {
        yield return new WaitForSeconds(0.5f); // așteaptă jumătate de secundă
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null && fallbackLocalPlayer != null)
        {
            Debug.LogWarning("Player not found via tag. Using fallback player for test.");
            player = fallbackLocalPlayer;
        }

        if (player == null)
        {
            Debug.LogError("No Player found and no fallback assigned.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || player == null) return;

        if (other.gameObject == player)
        {
            triggered = true;
            dialogueManager.StartDialogue(startingDialogue);
        }
    }
}
