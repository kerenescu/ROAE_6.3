using UnityEngine;

public class DialogueFlagSetter : MonoBehaviour
{
    [SerializeField] private DialogueFlag flagToSet;

    public void SetFlag()
    {
        if (flagToSet != null)
        {
            flagToSet.MarkAsTriggered();
            Debug.Log($"[FLAG] '{flagToSet.name}' a fost SETAT.");
        }
    }
}
