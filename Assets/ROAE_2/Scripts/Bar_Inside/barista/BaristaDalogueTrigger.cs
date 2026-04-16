using UnityEngine;

public class BaristaDialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData startingDialogue;

    public void TriggerDialogue()
    {
        Debug.Log("🗨️ Barista clicked → start dialogue");
        dialogueManager.StartDialogue(startingDialogue);
    }
}
