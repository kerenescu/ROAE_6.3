// JournalEntry.cs – fiecare gând de-al Rinei, declanșat de evenimente din joc
using UnityEngine;

[CreateAssetMenu(menuName = "Journal/Entry")]
public class JournalEntry : ScriptableObject
{
    public string title;
    [TextArea(4, 20)] public string body;
    public Sprite sketch; // opțional, o schiță sau imagine asociată
    public bool unlocked = false; // marcată true când e activată în joc
}
