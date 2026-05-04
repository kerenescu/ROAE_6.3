using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CaptionThoughtDatabase : MonoBehaviour
{
    public static CaptionThoughtDatabase Instance;
    private Dictionary<string, string> thoughts = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadFromJSON();
    }

    private void LoadFromJSON()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "thoughts.json");

        if (!File.Exists(path))
        {
            Debug.LogWarning("❌ Fișierul thoughts.json nu a fost găsit!");
            return;
        }

        string json = File.ReadAllText(path);
        ThoughtWrapper wrapper = JsonUtility.FromJson<ThoughtWrapper>(json);

        thoughts = new Dictionary<string, string>();
        foreach (var entry in wrapper.entries)
        {
            thoughts[entry.key] = entry.value;
        }

        // Debug.Log($"✅ Loaded {thoughts.Count} gânduri din thoughts.json");
    }

    public string GetThought(string contactName)
    {
        foreach (var kvp in thoughts)
        {
            if (contactName.Contains(kvp.Key))
                return kvp.Value;
        }
        return null;
    }

    [System.Serializable]
    private class ThoughtEntry
    {
        public string key;
        public string value;
    }

    [System.Serializable]
    private class ThoughtWrapper
    {
        public List<ThoughtEntry> entries;
    }
}
