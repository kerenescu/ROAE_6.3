using UnityEngine;

public class BaristaTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData startingDialogue;
    private GameObject player;


    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.gameObject == player)
        {
            triggered = true;
            dialogueManager.StartDialogue(startingDialogue);
            Time.timeScale = 0f; // blochează gameplay-ul
        }
    }
}
