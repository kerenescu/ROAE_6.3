using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueFlag", menuName = "Dialogue System/Dialogue Flag")]
public class DialogueFlag : ScriptableObject
{
    [SerializeField] private bool wasTriggered = false;

    public bool WasTriggered => wasTriggered;

    public void MarkAsTriggered()
    {
        wasTriggered = true;
    }

    public void ResetFlag()
    {
        wasTriggered = false;
    }
}
