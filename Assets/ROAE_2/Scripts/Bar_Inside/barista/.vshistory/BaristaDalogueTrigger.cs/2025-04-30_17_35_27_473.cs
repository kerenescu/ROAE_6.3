using UnityEngine;

public class BaristaDialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData startingDialogue;

    public void TriggerDialogue()
    {
        Debug.Log("Click pe Barista — începe dialogul");
        dialogueManager.StartDialogue(startingDialogue);
    }
}
