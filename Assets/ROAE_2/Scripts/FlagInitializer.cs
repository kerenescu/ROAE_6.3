using UnityEngine;

[System.Serializable]
public class FlagInitEntry
{
    public DialogueFlag flag;
    public bool defaultValue;
}

public class FlagInitializer : MonoBehaviour
{
    [Header("Setează flaguri la start")]
    public FlagInitEntry[] flagsToSet;

    void Awake()
    {
        foreach (FlagInitEntry entry in flagsToSet)
        {
            if (entry.flag == null) continue;

            if (entry.defaultValue)
                entry.flag.MarkAsTriggered();
            else
                entry.flag.ResetFlag();
        }

        Debug.Log("✅ FlagInitializer: Toate flagurile au fost setate la valori implicite.");
    }
}
