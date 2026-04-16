using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData dialogue;

    public void TriggerDialogue()
    {
        if (dialogueManager && dialogue)
        {
            dialogueManager.StartDialogue(dialogue);
        }
        else
        {
            Debug.LogWarning($"{name} nu are DialogueManager sau DialogueData setat.");
        }
    }
}
