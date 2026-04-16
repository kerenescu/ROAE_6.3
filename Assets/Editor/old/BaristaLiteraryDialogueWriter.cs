using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BaristaLiteraryDialogueWriter
{
    [MenuItem("Tools/Barista/Write Literary Dialogue Set FIXED")]
    public static void WriteLiteraryDialogueSet()
    {
        var presets = BuildPresets();
        int changed = 0;

        foreach (var kvp in presets)
        {
            Object asset = FindAssetByExactNameGlobal(kvp.Key);
            if (asset == null)
            {
                Debug.LogWarning("Missing asset: " + kvp.Key);
                continue;
            }

            SerializedObject so = new SerializedObject(asset);

            bool linesChanged = ApplyDialogueLines(so, kvp.Value.lines);
            bool choicesChanged = ApplyChoiceTexts(so, kvp.Value.choiceTexts);

            if (linesChanged || choicesChanged)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(asset);
                changed++;
                Debug.Log("Updated: " + kvp.Key);
            }
            else
            {
                Debug.LogWarning("No writable fields found in: " + kvp.Key);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Done. Assets changed: " + changed);
    }

    private static bool ApplyDialogueLines(SerializedObject so, string[] lines)
    {
        if (lines == null || lines.Length == 0)
            return false;

        SerializedProperty dialogueLinesProp = so.FindProperty("dialogueLines");
        if (dialogueLinesProp == null || !dialogueLinesProp.isArray)
            return false;

        dialogueLinesProp.arraySize = lines.Length;

        bool changed = false;

        for (int i = 0; i < lines.Length; i++)
        {
            SerializedProperty entry = dialogueLinesProp.GetArrayElementAtIndex(i);
            SerializedProperty textProp = entry.FindPropertyRelative("Text");

            if (textProp != null && textProp.propertyType == SerializedPropertyType.String)
            {
                textProp.stringValue = lines[i];
                changed = true;
            }
        }

        return changed;
    }

    private static bool ApplyChoiceTexts(SerializedObject so, string[] choiceTexts)
    {
        if (choiceTexts == null || choiceTexts.Length == 0)
            return false;

        SerializedProperty choicesProp = so.FindProperty("choices");
        if (choicesProp == null || !choicesProp.isArray)
            return false;

        if (choicesProp.arraySize < choiceTexts.Length)
            choicesProp.arraySize = choiceTexts.Length;

        bool changed = false;

        for (int i = 0; i < choiceTexts.Length; i++)
        {
            SerializedProperty choice = choicesProp.GetArrayElementAtIndex(i);
            SerializedProperty textProp = choice.FindPropertyRelative("choiceText");

            if (textProp != null && textProp.propertyType == SerializedPropertyType.String)
            {
                textProp.stringValue = choiceTexts[i];
                changed = true;
            }
        }

        return changed;
    }

    private static Object FindAssetByExactNameGlobal(string exactName)
    {
        string[] guids = AssetDatabase.FindAssets(exactName);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);

            if (fileName == exactName)
                return AssetDatabase.LoadAssetAtPath<Object>(path);
        }

        return null;
    }

    private static Dictionary<string, DialoguePreset> BuildPresets()
    {
        return new Dictionary<string, DialoguePreset>
        {
            {
                "[neutral - barista]",
                new DialoguePreset(
                    new[]
                    {
                        "You wear composure like a borrowed coat.",
                        "It fits well enough from a distance. Up close, the seams are pleading."
                    },
                    new[]
                    {
                        "I was hoping you might understand.",
                        "I do not need comfort. I need answers."
                    })
            },
            {
                "[neutral - barista - test]",
                new DialoguePreset(
                    new[]
                    {
                        "You came back with that careful face again.",
                        "The one people use when they are trying not to spill their own weather."
                    },
                    new[]
                    {
                        "Then tell me what you see.",
                        "Spare me the poetry."
                    })
            },
            {
                "[warm - barista]",
                new DialoguePreset(
                    new[]
                    {
                        "Sit, if you like.",
                        "The world is already doing enough damage without forcing you to remain upright through all of it."
                    },
                    new[]
                    {
                        "That is kinder than I expected.",
                        "Do not mistake me for fragile."
                    })
            },
            {
                "[warm - barista - test]",
                new DialoguePreset(
                    new[]
                    {
                        "You look tired in the old, sacred way.",
                        "Not sleepy. Soul-tired. The sort of tired that coffee salutes and then quietly fails to cure."
                    },
                    new[]
                    {
                        "Thank you. I needed that.",
                        "I did not come here to be read."
                    })
            },
            {
                "[guarded - barista]",
                new DialoguePreset(
                    new[]
                    {
                        "Careful.",
                        "Some questions enter a room like thieves and then act offended when they are not welcomed."
                    },
                    new[]
                    {
                        "Fine. I can be patient.",
                        "Then stop circling and speak plainly."
                    })
            },
            {
                "[guarded - barista - test]",
                new DialoguePreset(
                    new[]
                    {
                        "I know that tone.",
                        "It belongs to people who think truth is a debt the world owes them on demand."
                    },
                    new[]
                    {
                        "I will not push.",
                        "You are being deliberately evasive."
                    })
            },
            {
                "[hint - barista]",
                new DialoguePreset(
                    new[]
                    {
                        "If you are looking for a thread, do not begin with what is said aloud.",
                        "Begin with what is polished too often, avoided too quickly, or named with suspicious ease."
                    },
                    new[]
                    {
                        "That is enough for me to begin.",
                        "That barely counts as help."
                    })
            },
            {
                "[hint - barista - test]",
                new DialoguePreset(
                    new[]
                    {
                        "The obvious trail is usually bait.",
                        "Look instead for the places people maintain with devotion and explain with discomfort."
                    },
                    new[]
                    {
                        "I understand. I will look closer.",
                        "You could afford to be less cryptic."
                    })
            },
            {
                "[warm hint - barista]",
                new DialoguePreset(
                    new[]
                    {
                        "Listen carefully.",
                        "I should not be giving this to you, but there are doors in this place cleaner than innocence and twice as guilty."
                    },
                    new[]
                    {
                        "Then I will treat your warning seriously.",
                        "You still are not saying enough."
                    })
            },
            {
                "[warm hint - barista - test]",
                new DialoguePreset(
                    new[]
                    {
                        "You seem earnest, which is either admirable or catastrophic.",
                        "So I will spare you one mistake: do not ignore the corridor behind the counter."
                    },
                    new[]
                    {
                        "All right. I will remember that.",
                        "That sounds like half a confession."
                    })
            },
            {
                "[guarded hint - barista]",
                new DialoguePreset(
                    new[]
                    {
                        "I will say this once, and I would prefer not to be punished by destiny for it later.",
                        "If you insist on digging, begin where the staff pretends not to linger."
                    },
                    new[]
                    {
                        "Understood. I will be careful.",
                        "You are talking as if I am the danger."
                    })
            },
            {
                "[guarded hint - barista - test]",
                new DialoguePreset(
                    new[]
                    {
                        "You are asking dangerous questions with the confidence of someone who has not yet paid for them.",
                        "Fine. Start with the greenhouse. Then leave my conscience out of it."
                    },
                    new[]
                    {
                        "That is more than enough. Thank you.",
                        "You make secrecy sound theatrical."
                    })
            },
            {
                "[choice positive - barista - test]",
                new DialoguePreset(
                    new[]
                    {
                        "Good.",
                        "Then perhaps this conversation has not been wasted on either of us."
                    },
                    null)
            },
            {
                "[choice negative - barista - test]",
                new DialoguePreset(
                    new[]
                    {
                        "There it is.",
                        "That old, ugly human instinct to confuse reluctance with guilt and mystery with obligation."
                    },
                    null)
            }
        };
    }

    private readonly struct DialoguePreset
    {
        public readonly string[] lines;
        public readonly string[] choiceTexts;

        public DialoguePreset(string[] lines, string[] choiceTexts)
        {
            this.lines = lines;
            this.choiceTexts = choiceTexts;
        }
    }
}