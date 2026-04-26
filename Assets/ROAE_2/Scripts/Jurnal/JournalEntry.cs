using UnityEngine;
using System.Collections.Generic;

public class JournalAutoEntryManager : MonoBehaviour
{
    [System.Serializable]
    public class FlagPagePair
    {
        public DialogueFlag flag;
        public JournalPageData page;
        [HideInInspector] public bool wasAdded = false;
    }

    [Header("Mapări flaguri → pagini jurnal")]
    public List<FlagPagePair> entries = new List<FlagPagePair>();

    [Header("🎵 Sunet la adăugare pagină")]
    public AudioSource audioSource;
    public AudioClip entrySound;

    void Update()
    {
        foreach (var entry in entries)
        {
            if (!entry.wasAdded && entry.flag != null && entry.flag.IsTriggered())
            {
                if (entry.page != null && JournalUIFlow.Instance != null)
                {
                    JournalUIFlow.Instance.AddPageIfNotPresent(entry.page);
                    entry.wasAdded = true;

                    Debug.Log($"Pagină \"{entry.page.name}\" adăugată automat în jurnal (flag: {entry.flag.name})");

                    // 🔊 Play sound
                    if (audioSource != null && entrySound != null)
                        audioSource.PlayOneShot(entrySound);
                }
            }
        }
    }
}
