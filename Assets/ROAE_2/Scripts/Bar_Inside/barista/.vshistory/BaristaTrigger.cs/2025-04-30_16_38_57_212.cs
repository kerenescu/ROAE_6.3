using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BaristaTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData startingDialogue;

    private GameObject _player;
    private bool _triggered = false;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null)
        {
            Debug.LogError("Player not found! Make sure the Player has the tag 'Player'.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered || _player == null) return;

        if (other.gameObject == _player)
        {
            _triggered = true;
            dialogueManager.StartDialogue(startingDialogue);
        }
    }
}
