using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NpcToneDialogueProfileWindow : EditorWindow
{
    private const float SidebarWidth = 340f;

    [SerializeField] private NpcToneDialogueProfile profile;
    [SerializeField] private int selectedPhaseIndex;

    private SerializedObject serializedProfile;
    private Vector2 sidebarScroll;
    private Vector2 detailsScroll;

    [MenuItem("Tools/ROAE/NPC/Tone Dialogue Phase Editor")]
    public static void OpenWindow()
    {
        GetWindow<NpcToneDialogueProfileWindow>("Tone Dialogue");
    }

    private void OnEnable()
    {
        Selection.selectionChanged += HandleSelectionChanged;
        TryAdoptSelectedProfile();
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= HandleSelectionChanged;
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (profile == null)
        {
            EditorGUILayout.HelpBox(
                "Select an NpcToneDialogueProfile asset or assign one here to edit activation, planner settings, and phases in one place.",
                MessageType.Info);
            return;
        }

        EnsureSerializedProfile();
        serializedProfile.Update();

        SerializedProperty phasesProperty = serializedProfile.FindProperty("phaseDefinitions");
        ClampSelectedPhaseIndex(phasesProperty);

        EditorGUILayout.BeginHorizontal();
        DrawSidebar(phasesProperty);
        DrawPhaseDetails(phasesProperty);
        EditorGUILayout.EndHorizontal();

        if (serializedProfile.ApplyModifiedProperties())
            EditorUtility.SetDirty(profile);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        EditorGUI.BeginChangeCheck();
        NpcToneDialogueProfile newProfile = (NpcToneDialogueProfile)EditorGUILayout.ObjectField(
            profile,
            typeof(NpcToneDialogueProfile),
            false,
            GUILayout.MinWidth(220f));
        if (EditorGUI.EndChangeCheck())
            SetProfile(newProfile);

        if (GUILayout.Button("Use Selection", EditorStyles.toolbarButton, GUILayout.Width(90f)))
            TryAdoptSelectedProfile();

        GUI.enabled = profile != null;
        if (GUILayout.Button("Ping", EditorStyles.toolbarButton, GUILayout.Width(50f)))
            EditorGUIUtility.PingObject(profile);
        GUI.enabled = true;

        if (GUILayout.Button("Backfill All Tones", EditorStyles.toolbarButton, GUILayout.Width(120f)))
        {
            NpcToneDialogueToneBackfillUtility.BackfillAllProfiles(true);
            AssetDatabase.Refresh();
            if (profile != null)
                SetProfile(profile);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSidebar(SerializedProperty phasesProperty)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(SidebarWidth));
        sidebarScroll = EditorGUILayout.BeginScrollView(sidebarScroll);

        EditorGUILayout.LabelField("Profile", EditorStyles.boldLabel);
        DrawProfileProperty("npcId");
        DrawProfileProperty("momentId");
        DrawProfileProperty("outcomeLogTag");
        DrawProfileProperty("dialogueLogTag");
        DrawProfileProperty("devSummaryLogTag");
        DrawProfileProperty("devResetLogTag");

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Activation", EditorStyles.boldLabel);
        DrawProfileProperty("isEnabled");
        DrawProfileProperty("priority");
        DrawProfileProperty("requiredChapterId");
        DrawProfileProperty("requiredSceneId");
        DrawProfileProperty("requiredNarrativeMomentId");
        DrawProfileProperty("requiredTrueFlags", true);
        DrawProfileProperty("requiredFalseFlags", true);
        DrawProfileProperty("skipWhenCompleted");
        DrawProfileProperty("completedFlagKey");

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Planner", EditorStyles.boldLabel);
        DrawProfileProperty("plannerMode");
        DrawProfileProperty("toneSelectionMode");
        DrawProfileProperty("gamma");
        DrawProfileProperty("epsilon");
        DrawProfileProperty("maxValueIterations");
        DrawProfileProperty("maxPolicyIterations");
        DrawProfileProperty("maxPolicyEvaluationSweeps");

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Reset", EditorStyles.boldLabel);
        DrawProfileProperty("resetNpcIds", true);
        DrawProfileProperty("playerPrefFlagsToReset", true);
        DrawProfileProperty("dialogueFlagsToReset", true);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Phases", EditorStyles.boldLabel);
        DrawPhaseList(phasesProperty);

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawPhaseList(SerializedProperty phasesProperty)
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Phase"))
        {
            phasesProperty.InsertArrayElementAtIndex(phasesProperty.arraySize);
            SerializedProperty newPhase = phasesProperty.GetArrayElementAtIndex(phasesProperty.arraySize - 1);
            newPhase.FindPropertyRelative("phaseId").stringValue = "NewPhase";
            selectedPhaseIndex = phasesProperty.arraySize - 1;
        }

        if (GUILayout.Button("Seed Barista Loop"))
            SeedStandardBaristaLoop();
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < phasesProperty.arraySize; i++)
        {
            SerializedProperty phaseProperty = phasesProperty.GetArrayElementAtIndex(i);
            string phaseId = phaseProperty.FindPropertyRelative("phaseId").stringValue;
            string label = string.IsNullOrWhiteSpace(phaseId) ? "Phase " + i : phaseId;

            GUIStyle style = i == selectedPhaseIndex ? EditorStyles.miniButtonMid : EditorStyles.miniButton;
            if (GUILayout.Button(label, style))
                selectedPhaseIndex = i;
        }

        EditorGUILayout.BeginHorizontal();
        GUI.enabled = phasesProperty.arraySize > 0 && selectedPhaseIndex >= 0;
        if (GUILayout.Button("Duplicate") && selectedPhaseIndex >= 0)
        {
            phasesProperty.InsertArrayElementAtIndex(selectedPhaseIndex);
            selectedPhaseIndex++;
        }

        if (GUILayout.Button("Delete") && selectedPhaseIndex >= 0)
        {
            phasesProperty.DeleteArrayElementAtIndex(selectedPhaseIndex);
            ClampSelectedPhaseIndex(phasesProperty);
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUI.enabled = phasesProperty.arraySize > 1 && selectedPhaseIndex > 0;
        if (GUILayout.Button("Up"))
        {
            phasesProperty.MoveArrayElement(selectedPhaseIndex, selectedPhaseIndex - 1);
            selectedPhaseIndex--;
        }

        GUI.enabled = phasesProperty.arraySize > 1 && selectedPhaseIndex >= 0 && selectedPhaseIndex < phasesProperty.arraySize - 1;
        if (GUILayout.Button("Down"))
        {
            phasesProperty.MoveArrayElement(selectedPhaseIndex, selectedPhaseIndex + 1);
            selectedPhaseIndex++;
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPhaseDetails(SerializedProperty phasesProperty)
    {
        EditorGUILayout.BeginVertical();
        detailsScroll = EditorGUILayout.BeginScrollView(detailsScroll);

        if (phasesProperty.arraySize == 0 || selectedPhaseIndex < 0)
        {
            EditorGUILayout.HelpBox("This profile has no phases yet. Add one or seed a standard Barista loop.", MessageType.Info);
        }
        else
        {
            SerializedProperty phaseProperty = phasesProperty.GetArrayElementAtIndex(selectedPhaseIndex);
            EditorGUILayout.LabelField("Selected Phase", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(phaseProperty, true);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void SeedStandardBaristaLoop()
    {
        if (profile == null)
            return;

        Undo.RecordObject(profile, "Seed Barista Tone Dialogue Phases");
        profile.phaseDefinitions = new List<NpcToneDialoguePhaseDefinition>
        {
            CreateBaristaPhase("Intro", false, BaristaDrinkType.None, false, BaristaDrinkType.None),
            CreateBaristaPhase("Order", true, BaristaDrinkType.None, false, BaristaDrinkType.None),
            CreateBaristaPhase("Preparing", true, BaristaDrinkType.PhotosyntheticSap, false, BaristaDrinkType.None),
            CreateBaristaPhase("PreparingReminder", true, BaristaDrinkType.PhotosyntheticSap, true, BaristaDrinkType.None),
            CreateBaristaPhase("AlreadyHasCola", true, BaristaDrinkType.None, false, BaristaDrinkType.Cola),
            CreateBaristaPhase("AlreadyHasSap", true, BaristaDrinkType.None, false, BaristaDrinkType.PhotosyntheticSap)
        };

        EditorUtility.SetDirty(profile);
        serializedProfile = new SerializedObject(profile);
        selectedPhaseIndex = 0;
    }

    private static NpcToneDialoguePhaseDefinition CreateBaristaPhase(
        string phaseId,
        bool expectedIntroDone,
        BaristaDrinkType pendingDrink,
        bool pendingAcknowledged,
        BaristaDrinkType heldDrink)
    {
        return new NpcToneDialoguePhaseDefinition
        {
            phaseId = phaseId,
            matchCurrentIntroDone = true,
            expectedIntroDone = expectedIntroDone,
            matchCurrentPendingDrink = true,
            matchCurrentPendingAcknowledged = true,
            matchCurrentHeldDrink = true,
            readUnknownText = new NpcToneDialogueBooleanSignal
            {
                source = NpcToneDialogueBooleanSource.PlayerPrefFlag,
                playerPrefKey = BaristaWelcomeKeys.ReadUnknownText01
            },
            introDone = new NpcToneDialogueBooleanSignal
            {
                source = NpcToneDialogueBooleanSource.PlayerPrefFlag,
                playerPrefKey = BaristaWelcomeKeys.BaristaIntroDone
            },
            pendingDrink = pendingDrink,
            pendingDrinkAcknowledged = pendingAcknowledged,
            heldDrink = heldDrink
        };
    }

    private void DrawProfileProperty(string propertyName, bool includeChildren = false)
    {
        SerializedProperty property = serializedProfile.FindProperty(propertyName);
        if (property != null)
            EditorGUILayout.PropertyField(property, includeChildren);
    }

    private void SetProfile(NpcToneDialogueProfile newProfile)
    {
        profile = newProfile;
        serializedProfile = profile != null ? new SerializedObject(profile) : null;
        selectedPhaseIndex = 0;
        Repaint();
    }

    private void TryAdoptSelectedProfile()
    {
        if (Selection.activeObject is NpcToneDialogueProfile selectedProfile)
            SetProfile(selectedProfile);
    }

    private void HandleSelectionChanged()
    {
        if (Selection.activeObject is NpcToneDialogueProfile)
            TryAdoptSelectedProfile();
    }

    private void EnsureSerializedProfile()
    {
        if (profile != null && (serializedProfile == null || serializedProfile.targetObject != profile))
            serializedProfile = new SerializedObject(profile);
    }

    private void ClampSelectedPhaseIndex(SerializedProperty phasesProperty)
    {
        if (phasesProperty == null || phasesProperty.arraySize == 0)
        {
            selectedPhaseIndex = -1;
            return;
        }

        if (selectedPhaseIndex < 0 || selectedPhaseIndex >= phasesProperty.arraySize)
            selectedPhaseIndex = 0;
    }
}
