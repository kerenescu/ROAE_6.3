using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Journal/Page")]
public class JournalPageData : ScriptableObject
{
    public List<string> thoughts;
    public List<string> tasks;
    public List<Sprite> collectibles;
}
