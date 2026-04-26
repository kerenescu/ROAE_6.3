using UnityEngine;

public class RemoveMadameIfFlag : MonoBehaviour
{
    [Header("Setează flagul de tip DialogueFlag în Inspector")]
    public DialogueFlag dialogueFlag;

    void Start()
    {
        if (dialogueFlag != null && dialogueFlag.WasTriggered)
        {
            gameObject.SetActive(false);
        }
    }
}
