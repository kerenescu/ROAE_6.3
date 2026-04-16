using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueFlag", menuName = "Dialogue System/Dialogue Flag")]
public class DialogueFlag : ScriptableObject
{
    [Header("Unique Key for saving (PlayerPrefs)")]
    [SerializeField] private string flagKey;

    [Header("Is Triggered (for test/debug only)")]
    [SerializeField] private bool isTriggeredEditor = false;  // ✅ checkbox manual

    public bool WasTriggered => isTriggeredEditor;

    public bool IsTriggered()
    {
        return isTriggeredEditor;
    }

    public void MarkAsTriggered()
    {
        isTriggeredEditor = true;
    }

    public void ResetFlag()
    {
        isTriggeredEditor = false;
    }
}
