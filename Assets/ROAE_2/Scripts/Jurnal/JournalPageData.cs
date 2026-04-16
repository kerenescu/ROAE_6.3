using UnityEngine;
using System.Collections.Generic;

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Journal/Page")]
public class JournalPageData : ScriptableObject
{
    public List<string> thoughts;
    public List<TaskEntry> tasks; // ← actualizat
    public List<Sprite> collectibles;
}


[System.Serializable]
public class TaskEntry
{
    public string taskDescription;
    public bool isCompleted; // pentru bifare
}

