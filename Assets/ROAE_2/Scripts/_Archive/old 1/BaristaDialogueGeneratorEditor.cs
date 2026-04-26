using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BaristaDialogueGeneratorEditor
{
    private const string RootFolder = "Assets/ROAE_2/Scripts/NPC_AI/Barista";
    private const string GeneratedFolder = RootFolder + "/Generated";
    private const string MenuPath = "ROAE/Barista/Generate Generated Dialogues";

    private sealed class DialogueSpec
    {
        public string assetName;
        public LineSpec[] lines;
        public ChoiceSpec[] choices;
    }

    private sealed class LineSpec
    {
        public string speaker;
        public string text;

        public LineSpec(string speaker, string text)
        {
            this.speaker = speaker;
            this.text = text;
        }
    }

    private sealed class ChoiceSpec
    {
        public string id;
        public string text;
        public int creativity;
        public int empathy;
        public int corruption;
        public int relationship;
        public DialogueSpec nextDialogue;

        public ChoiceSpec(string id, string text, int creativity, int empathy, int corruption, int relationship, DialogueSpec nextDialogue)
        {
            this.id = id;
            this.text = text;
            this.creativity = creativity;
            this.empathy = empathy;
            this.corruption = corruption;
            this.relationship = relationship;
            this.nextDialogue = nextDialogue;
        }
    }

    [MenuItem(MenuPath)]
    public static void Generate()
    {
        EnsureFolder("Assets/ROAE_2");
        EnsureFolder("Assets/ROAE_2/Scripts");
        EnsureFolder("Assets/ROAE_2/Scripts/NPC_AI");
        EnsureFolder(RootFolder);
        EnsureFolder(GeneratedFolder);

        Type dialogueType = FindScriptableObjectType("dialogueLines", "choices");
        Type reactionProfileType = FindScriptableObjectType(
            "neutralDialogue",
            "warmDialogue",
            "guardedDialogue",
            "hintDialogue",
            "warmHintDialogue",
            "guardedHintDialogue");

        if (dialogueType == null)
        {
            Debug.LogError("[ROAE][BARISTA][EDITOR] Could not find dialogue asset type. Expected a ScriptableObject with fields 'dialogueLines' and 'choices'.");
            return;
        }

        if (reactionProfileType == null)
        {
            Debug.LogError("[ROAE][BARISTA][EDITOR] Could not find reaction profile asset type. Expected the six dialogue reference fields.");
            return;
        }

        Dictionary<string, DialogueSpec> specs = BuildDialogueSpecs();
        Dictionary<string, ScriptableObject> createdDialogues = new Dictionary<string, ScriptableObject>();

        foreach (DialogueSpec spec in specs.Values)
        {
            string path = GeneratedFolder + "/" + spec.assetName + ".asset";
            ScriptableObject asset = CreateOrReplaceAsset(dialogueType, path);
            createdDialogues[spec.assetName] = asset;
        }

        foreach (DialogueSpec spec in specs.Values)
            PopulateDialogueAsset(createdDialogues[spec.assetName], spec, createdDialogues);

        string reactionProfilePath = GeneratedFolder + "/barista_reaction_profile_generated.asset";
        ScriptableObject reactionProfile = CreateOrReplaceAsset(reactionProfileType, reactionProfilePath);
        PopulateReactionProfile(reactionProfile, createdDialogues);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ROAE][BARISTA][EDITOR] Generated barista dialogue set in " + GeneratedFolder);
    }

    private static Dictionary<string, DialogueSpec> BuildDialogueSpecs()
    {
        var all = new Dictionary<string, DialogueSpec>();

        DialogueSpec neutralEmpathyFollowup = Followup(
            "barista_neutral_empathy_followup",
            new LineSpec("Barista", "That sounded honest. Honest things survive longer here than brave ones."),
            new LineSpec("Rina", "Then I will try honesty first."));

        DialogueSpec neutralCreativeFollowup = Followup(
            "barista_neutral_creative_followup",
            new LineSpec("Barista", "Patterns help if you can stand what they imply."),
            new LineSpec("Rina", "I have lived off implication for years."));

        DialogueSpec neutralCorruptFollowup = Followup(
            "barista_neutral_corrupt_followup",
            new LineSpec("Barista", "Curiosity becomes appetite very quickly in this place."),
            new LineSpec("Rina", "Maybe appetite is the only honest compass left."));

        DialogueSpec warmEmpathyFollowup = Followup(
            "barista_warm_empathy_followup",
            new LineSpec("Barista", "Good. Keep your hands steady and your pity selective."),
            new LineSpec("Rina", "I can manage one of those immediately."));

        DialogueSpec warmCreativeFollowup = Followup(
            "barista_warm_creative_followup",
            new LineSpec("Barista", "Then notice the cups. Nothing in this room is arranged without intent."),
            new LineSpec("Rina", "Intent leaves cleaner fingerprints than guilt."));

        DialogueSpec warmCorruptFollowup = Followup(
            "barista_warm_corrupt_followup",
            new LineSpec("Barista", "You say that like ruin is a craft."),
            new LineSpec("Rina", "Sometimes craft is just ruin with better posture."));

        DialogueSpec guardedEmpathyFollowup = Followup(
            "barista_guarded_empathy_followup",
            new LineSpec("Barista", "Patience does not suit many people. It might suit you."),
            new LineSpec("Rina", "I only borrow it when panic becomes embarrassing."));

        DialogueSpec guardedCreativeFollowup = Followup(
            "barista_guarded_creative_followup",
            new LineSpec("Barista", "Then map the omissions. They are louder than the answers."),
            new LineSpec("Rina", "Absence has always been theatrical."));

        DialogueSpec guardedHarshFollowup = Followup(
            "barista_guarded_harsh_followup",
            new LineSpec("Barista", "Plain speech is expensive. You have not paid for it yet."),
            new LineSpec("Rina", "Then I will remember the invoice."));

        DialogueSpec hintEmpathyFollowup = Followup(
            "barista_hint_empathy_followup",
            new LineSpec("Barista", "Start with who is protecting whom. That question peels varnish fast."),
            new LineSpec("Rina", "That is almost generous."));

        DialogueSpec hintCreativeFollowup = Followup(
            "barista_hint_creative_followup",
            new LineSpec("Barista", "Count what repeats. Repetition is devotion or warning."),
            new LineSpec("Rina", "Either way it leaves a rhythm."));

        DialogueSpec hintCorruptFollowup = Followup(
            "barista_hint_corrupt_followup",
            new LineSpec("Barista", "If you insist on digging, begin where the smell turns sweet."),
            new LineSpec("Rina", "Sweetness is how rot convinces the unprepared."));

        DialogueSpec warmHintEmpathyFollowup = Followup(
            "barista_warm_hint_empathy_followup",
            new LineSpec("Barista", "Take the side door if you must. Knock before you pity anyone inside."),
            new LineSpec("Rina", "I can offer restraint before comfort."));

        DialogueSpec warmHintCreativeFollowup = Followup(
            "barista_warm_hint_creative_followup",
            new LineSpec("Barista", "There is chalk under the counter. Someone wanted the walls to remember."),
            new LineSpec("Rina", "Then I know where to begin reading."));

        DialogueSpec warmHintCorruptFollowup = Followup(
            "barista_warm_hint_corrupt_followup",
            new LineSpec("Barista", "I am helping you because hesitation may be worse."),
            new LineSpec("Rina", "That is a dangerous kind of kindness."));

        DialogueSpec guardedHintEmpathyFollowup = Followup(
            "barista_guarded_hint_empathy_followup",
            new LineSpec("Barista", "Greenhouse first. Speak softly there, even if nothing answers."),
            new LineSpec("Rina", "Softness costs less than regret."));

        DialogueSpec guardedHintCreativeFollowup = Followup(
            "barista_guarded_hint_creative_followup",
            new LineSpec("Barista", "Look for what is polished too often. Fear loves maintenance."),
            new LineSpec("Rina", "Then the shine is the confession."));

        DialogueSpec guardedHintHarshFollowup = Followup(
            "barista_guarded_hint_harsh_followup",
            new LineSpec("Barista", "Mockery is not insight. Do not confuse the two."),
            new LineSpec("Rina", "I only confuse them when I am tired."));

        DialogueSpec neutral = new DialogueSpec
        {
            assetName = "barista_neutral_generated",
            lines = new[]
            {
                new LineSpec("Barista", "You wear composure like a borrowed coat."),
                new LineSpec("Rina", "It fits well enough from a distance. Up close, the seams are pleading.")
            },
            choices = new[]
            {
                new ChoiceSpec("empathy", "I was hoping someone here might understand.", 0, 1, 0, 2, neutralEmpathyFollowup),
                new ChoiceSpec("creative", "Then tell me what in this room is lying first.", 10, 0, 0, 1, neutralCreativeFollowup),
                new ChoiceSpec("corrupt", "If the place wants something from me, I would rather hear it directly.", 0, 0, 1, -1, neutralCorruptFollowup)
            }
        };

        DialogueSpec warm = new DialogueSpec
        {
            assetName = "barista_warm_generated",
            lines = new[]
            {
                new LineSpec("Barista", "Sit, if you like. The room behaves better when people stop pretending they are above fatigue."),
                new LineSpec("Rina", "That is kinder than most architecture I have known.")
            },
            choices = new[]
            {
                new ChoiceSpec("empathy", "Thank you. Kindness still surprises me, which is probably not flattering.", 0, 1, 0, 2, warmEmpathyFollowup),
                new ChoiceSpec("creative", "Then let me look properly. Rooms this careful are usually hiding a diagram.", 10, 0, 0, 1, warmCreativeFollowup),
                new ChoiceSpec("corrupt", "If the room behaves, maybe it already knows what I came here to become.", 0, 0, 1, -1, warmCorruptFollowup)
            }
        };

        DialogueSpec guarded = new DialogueSpec
        {
            assetName = "barista_guarded_generated",
            lines = new[]
            {
                new LineSpec("Barista", "Careful."),
                new LineSpec("Rina", "Some questions arrive like thieves and then act wounded when the locks object.")
            },
            choices = new[]
            {
                new ChoiceSpec("empathy", "Fine. I can be patient if patience gets us closer to the truth.", 0, 1, 0, 2, guardedEmpathyFollowup),
                new ChoiceSpec("creative", "Then I will read the omissions instead of the answers.", 10, 0, 0, 1, guardedCreativeFollowup),
                new ChoiceSpec("harsh", "Then stop performing mystery and say what you mean.", 0, -1, 0, -2, guardedHarshFollowup)
            }
        };

        DialogueSpec hint = new DialogueSpec
        {
            assetName = "barista_hint_generated",
            lines = new[]
            {
                new LineSpec("Barista", "If you are looking for a thread, do not begin with what is said aloud."),
                new LineSpec("Rina", "Begin with what is polished too often, avoided too quickly, or named with suspicious ease.")
            },
            choices = new[]
            {
                new ChoiceSpec("empathy", "That is enough for me to begin without hurting the wrong person.", 0, 1, 0, 2, hintEmpathyFollowup),
                new ChoiceSpec("creative", "Then I will follow the repetitions. Repetition is never innocent.", 10, 0, 0, 1, hintCreativeFollowup),
                new ChoiceSpec("corrupt", "If rot is hiding under the sugar, I would rather taste it now.", 0, 0, 1, -1, hintCorruptFollowup)
            }
        };

        DialogueSpec warmHint = new DialogueSpec
        {
            assetName = "barista_warm_hint_generated",
            lines = new[]
            {
                new LineSpec("Barista", "You seem earnest, which is either admirable or catastrophic."),
                new LineSpec("Rina", "Then spare me one mistake and let me earn the rest myself.")
            },
            choices = new[]
            {
                new ChoiceSpec("empathy", "I will be careful with whoever is caught in this before I am careful with myself.", 0, 1, 0, 2, warmHintEmpathyFollowup),
                new ChoiceSpec("creative", "Point me toward the mark, the pattern, the stain that repeats.", 10, 0, 0, 1, warmHintCreativeFollowup),
                new ChoiceSpec("corrupt", "If hesitation is the trap, then maybe I should stop hesitating.", 0, 0, 1, -1, warmHintCorruptFollowup)
            }
        };

        DialogueSpec guardedHint = new DialogueSpec
        {
            assetName = "barista_guarded_hint_generated",
            lines = new[]
            {
                new LineSpec("Barista", "You are asking dangerous questions with the confidence of someone who has not yet paid for them."),
                new LineSpec("Rina", "Then give me a direction, not absolution.")
            },
            choices = new[]
            {
                new ChoiceSpec("empathy", "Fine. A direction is enough. I can manage the rest without trampling anyone.", 0, 1, 0, 2, guardedHintEmpathyFollowup),
                new ChoiceSpec("creative", "Tell me what detail people keep cleaning. Fear always leaves a shine.", 10, 0, 0, 1, guardedHintCreativeFollowup),
                new ChoiceSpec("harsh", "You make secrecy sound theatrical. I am not here for theater.", 0, -1, 0, -2, guardedHintHarshFollowup)
            }
        };

        DialogueSpec[] specs =
        {
            neutralEmpathyFollowup,
            neutralCreativeFollowup,
            neutralCorruptFollowup,
            warmEmpathyFollowup,
            warmCreativeFollowup,
            warmCorruptFollowup,
            guardedEmpathyFollowup,
            guardedCreativeFollowup,
            guardedHarshFollowup,
            hintEmpathyFollowup,
            hintCreativeFollowup,
            hintCorruptFollowup,
            warmHintEmpathyFollowup,
            warmHintCreativeFollowup,
            warmHintCorruptFollowup,
            guardedHintEmpathyFollowup,
            guardedHintCreativeFollowup,
            guardedHintHarshFollowup,
            neutral,
            warm,
            guarded,
            hint,
            warmHint,
            guardedHint
        };

        foreach (DialogueSpec spec in specs)
            all[spec.assetName] = spec;

        return all;
    }

    private static DialogueSpec Followup(string assetName, params LineSpec[] lines)
    {
        return new DialogueSpec
        {
            assetName = assetName,
            lines = lines,
            choices = Array.Empty<ChoiceSpec>()
        };
    }

    private static void PopulateDialogueAsset(UnityEngine.Object asset, DialogueSpec spec, Dictionary<string, ScriptableObject> createdDialogues)
    {
        SerializedObject so = new SerializedObject(asset);
        so.FindProperty("m_Name").stringValue = spec.assetName;

        SerializedProperty dialogueLines = so.FindProperty("dialogueLines");
        if (dialogueLines == null)
            throw new Exception("Dialogue asset is missing 'dialogueLines'.");

        dialogueLines.arraySize = spec.lines.Length;
        for (int i = 0; i < spec.lines.Length; i++)
        {
            SerializedProperty lineProp = dialogueLines.GetArrayElementAtIndex(i);
            SetString(lineProp, "Speaker", spec.lines[i].speaker);
            SetString(lineProp, "Text", spec.lines[i].text);
        }

        SerializedProperty choices = so.FindProperty("choices");
        if (choices == null)
            throw new Exception("Dialogue asset is missing 'choices'.");

        choices.arraySize = spec.choices.Length;
        for (int i = 0; i < spec.choices.Length; i++)
        {
            ChoiceSpec choice = spec.choices[i];
            SerializedProperty choiceProp = choices.GetArrayElementAtIndex(i);
            SetString(choiceProp, "choiceText", choice.text);

            SerializedProperty nextDialogueProp = choiceProp.FindPropertyRelative("nextDialogue");
            if (nextDialogueProp != null)
                nextDialogueProp.objectReferenceValue = choice.nextDialogue == null ? null : createdDialogues[choice.nextDialogue.assetName];

            SerializedProperty statEffect = choiceProp.FindPropertyRelative("statEffect");
            if (statEffect != null)
            {
                SetInt(statEffect, "creativity", choice.creativity);
                SetInt(statEffect, "empathy", choice.empathy);
                SetInt(statEffect, "plantCorruption", choice.corruption);
                SetOptionalString(statEffect, "debugSource", spec.assetName + "::" + choice.id);
            }

            SerializedProperty relationshipEffect = choiceProp.FindPropertyRelative("relationshipEffect");
            if (relationshipEffect != null)
            {
                SetString(relationshipEffect, "npcId", "barista");
                SetInt(relationshipEffect, "amount", choice.relationship);
                SetOptionalString(relationshipEffect, "debugSource", spec.assetName + "::" + choice.id);
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static void PopulateReactionProfile(UnityEngine.Object asset, Dictionary<string, ScriptableObject> createdDialogues)
    {
        SerializedObject so = new SerializedObject(asset);
        so.FindProperty("m_Name").stringValue = "barista_reaction_profile_generated";

        SetObject(so, "neutralDialogue", createdDialogues["barista_neutral_generated"]);
        SetObject(so, "warmDialogue", createdDialogues["barista_warm_generated"]);
        SetObject(so, "guardedDialogue", createdDialogues["barista_guarded_generated"]);
        SetObject(so, "hintDialogue", createdDialogues["barista_hint_generated"]);
        SetObject(so, "warmHintDialogue", createdDialogues["barista_warm_hint_generated"]);
        SetObject(so, "guardedHintDialogue", createdDialogues["barista_guarded_hint_generated"]);

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static ScriptableObject CreateOrReplaceAsset(Type type, string path)
    {
        ScriptableObject existing = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (existing != null)
        {
            if (existing.GetType() == type)
                return existing;

            AssetDatabase.DeleteAsset(path);
        }

        ScriptableObject asset = ScriptableObject.CreateInstance(type);
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static Type FindScriptableObjectType(params string[] requiredFields)
    {
        return TypeCache.GetTypesDerivedFrom<ScriptableObject>()
            .Where(t => !t.IsAbstract)
            .FirstOrDefault(t => requiredFields.All(fieldName => HasFieldInHierarchy(t, fieldName)));
    }

    private static bool HasFieldInHierarchy(Type type, string fieldName)
    {
        Type current = type;
        while (current != null)
        {
            var field = current.GetField(fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.DeclaredOnly);

            if (field != null)
                return true;

            current = current.BaseType;
        }

        return false;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        int slash = path.LastIndexOf('/');
        string parent = path.Substring(0, slash);
        string name = path.Substring(slash + 1);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static void SetString(SerializedProperty parent, string childName, string value)
    {
        SerializedProperty prop = parent.FindPropertyRelative(childName);
        if (prop != null)
            prop.stringValue = value;
    }

    private static void SetOptionalString(SerializedProperty parent, string childName, string value)
    {
        SerializedProperty prop = parent.FindPropertyRelative(childName);
        if (prop != null)
            prop.stringValue = value;
    }

    private static void SetInt(SerializedProperty parent, string childName, int value)
    {
        SerializedProperty prop = parent.FindPropertyRelative(childName);
        if (prop != null)
            prop.intValue = value;
    }

    private static void SetObject(SerializedObject so, string propertyName, UnityEngine.Object value)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
            prop.objectReferenceValue = value;
    }
}
