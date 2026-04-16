using System.Collections.Generic;
using UnityEngine;

public class DialogueFlags : MonoBehaviour
{
    public static DialogueFlags Instance;

    private HashSet<string> triggeredFlags = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetFlag(string flagName)
    {
        triggeredFlags.Add(flagName);
    }

    public bool HasFlag(string flagName)
    {
        return triggeredFlags.Contains(flagName);
    }
}
