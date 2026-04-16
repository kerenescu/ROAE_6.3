using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BaristaLiteraryDialogueWriter
{
    private const string FolderPath = "Assets/ROAE_2/Scripts/NPC_AI/Barista";

    [MenuItem("Tools/Barista/Write Literary Dialogue Set")]
    public static void WriteLiteraryDialogueSet()
    {
        var presets = BuildPresets();
        int changed = 0;

        foreach (var kvp in presets)
        {
            Object asset = FindAssetByExactName(kvp.Key);
            if (asset == null)
            {
                Debug.LogWarning("Missing asset: " + kvp.Key);
                continue;
            }

            SerializedObject so = new SerializedObject(asset);

            ApplyDialogueLines(so, kvp.Value.lines);
            ApplyChoiceTexts(so, kvp.Value.choiceTexts);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            changed++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Barista literary set written. Assets changed: " + changed);
    }

    private static void ApplyDialogueLines(SerializedObject so, string[] lines)
    {
        if (lines == null || lines.Length == 0)
            return;

        SerializedProperty dialogueLinesProp = so.FindProperty("dialogueLines");
        if (dialogueLinesProp == null || !dialogueLinesProp.isArray)
            return;

        dialogueLinesProp.arraySize = lines.Length;

        for (int i = 0; i < lines.Length; i++)
        {
            SerializedProperty entry = dialogueLinesProp.GetArrayElementAtIndex(i);
            SerializedProperty textProp = FindRelative(entry, "lineText", "text", "dialogueText", "sentence");
            if (textProp != null && textProp.propertyType == SerializedPropertyType.String)
                textProp.stringValue = lines[i];
        }
    }

    private static void ApplyChoiceTexts(SerializedObject so, string[] choiceTexts)
    {
        if (choiceTexts == null || choiceTexts.Length == 0)
            return;

        SerializedProperty choicesProp = so.FindProperty("choices");
        if (choicesProp == null || !choicesProp.isArray)
            return;

        if (choicesProp.arraySize < choiceTexts.Length)
            choicesProp.arraySize = choiceTexts.Length;

        for (int i = 0; i < choiceTexts.Length; i++)
        {
            SerializedProperty choice = choicesProp.GetArrayElementAtIndex(i);
            SerializedProperty textProp = FindRelative(choice, "choiceText", "text", "label");
            if (textProp != null && textProp.propertyType == SerializedPropertyType.String)
                textProp.stringValue = choiceTexts[i];
        }
    }

    private static SerializedProperty FindRelative(SerializedProperty root, params string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            SerializedProperty prop = root.FindPropertyRelative(names[i]);
            if (prop != null)
                return prop;
        }

        return null;
    }

    private static Object FindAssetByExactName(string exactName)
    {
        string[] guids = AssetDatabase.FindAssets("t:Object", new[] { FolderPath });

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
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
                        "You have the look of someone pretending to be composed.",
                        "The city teaches that posture. It rarely teaches what to do after it fails."
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
                        "There is something unfinished about you tonight.",
                        "Not broken. Merely interrupted."
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
                        "Sit down, if only to give your thoughts somewhere softer to fall.",
                        "You look like the kind of person who has been holding a private trial all day."
                    },
                    new[]
                    {
                        "That is kinder than I expected.",
                        "Do not mistake my silence for weakness."
                    })
            },
            {
                "[warm - barista - test]",
                new DialoguePreset(
                    new[]
                    {
                        "You do not have to justify your exhaustion here.",
                        "Some faces ask for coffee. Yours asks for amnesty."
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
                        "Careful now.",
                        "Questions have a way of revealing more about the asker than the answer ever reveals in return."
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
                        "It belongs to people who think truth is a drawer they are entitled to open."
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
                        "If you are intent on following this thread, do not start with what people say.",
                        "Start with what they keep polished, locked, or strangely absent."
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
                        "The answer is never where the frightened tell you to look.",
                        "Try the places maintained with suspicious devotion."
                    },
                    new[]
                    {
                        "I understand. I will look closer.",
                        "You could afford to be less cryptic."
                    })
            },
            {
                "[warm - hint - barista]",
                new DialoguePreset(
                    new[]
                    {
                        "Listen carefully.",
                        "I should not be giving this away, but there are doors in this place that are cleaner than innocence and used twice as often."
                    },
                    new[]
                    {
                        "Then I will treat your warning seriously.",
                        "You still are not saying enough."
                    })
            },
            {
                "[warm hint - barista]",
                new DialoguePreset(
                    new[]
                    {
                        "Listen carefully.",
                        "I should not be giving this away, but there are doors in this place that are cleaner than innocence and used twice as often."
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
                        "I will say this once, and I would prefer not to be quoted by fate afterward.",
                        "If you insist on digging, start where the staff pretends not to linger."
                    },
                    new[]
                    {
                        "Understood. I will be careful.",
                        "You are talking like I am the danger."
                    })
            },
            {
                "[guarded hint - barista - test]",
                new DialoguePreset(
                    new[]
                    {
                        "You are asking dangerous questions with the confidence of someone who has not yet paid for them.",
                        "Fine. Begin with the greenhouse. Then leave my conscience out of it."
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
                        "Then perhaps this conversation has not gone to waste after all."
                    },
                    null)
            },
            {
                "[choice negative - barista - test]",
                new DialoguePreset(
                    new[]
                    {
                        "There it is.",
                        "The old human impatience: to confuse reluctance with guilt and mystery with obligation."
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