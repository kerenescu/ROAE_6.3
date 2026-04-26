using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CaptionThoughtDatabase : MonoBehaviour
{
    public static CaptionThoughtDatabase Instance;

    private Dictionary<string, string> thoughts = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadThoughtsFromJSON();
    }

    private void LoadThoughtsFromJSON()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "thoughts.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            thoughts = JsonUtility.FromJson<ThoughtWrapper>("{\"entries\":" + json + "}").ToDictionary();
            Debug.Log($"🧠 {thoughts.Count} gânduri încărcate din JSON.");
        }
        else
        {
            Debug.LogWarning("⚠️ Fișierul thoughts.json nu a fost găsit!");
        }
    }

    public string GetThought(string contactName)
    {
        foreach (var pair in thoughts)
        {
            if (contactName.Contains(pair.Key))
                return pair.Value;
        }

        return null;
    }

    [System.Serializable]
    private class ThoughtEntry
    {
        public string Key;
        public string Value;
    }

    [System.Serializable]
    private class ThoughtWrapper
    {
        public List<ThoughtEntry> entries;

        public Dictionary<string, string> ToDictionary()
        {
            Dictionary<string, string> dict = new();
            foreach (var entry in entries)
                dict[entry.Key] = entry.Value;
            return dict;
        }
    }
}
