using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class ROAE_BaristaWelcomeBootstrapper
{
    private const string MarkerKey = "ROAE.BaristaWelcome.CreateAssetsAfterCompile";

    private const string RootScripts = "Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome";
    private const string RootData = "Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome";
    private const string RootEditor = "Assets/Editor/ROAE_NarrativeV2";

    [MenuItem("Tools/ROAE/Barista Welcome/Generate Scaffolding")]
    public static void GenerateScaffolding()
    {
        EnsureFolders();

        WriteFile(Path.Combine(RootScripts, "Core/BaristaIntroTone.cs"), GetBaristaIntroToneCode());
        WriteFile(Path.Combine(RootScripts, "Core/BaristaPlannerMode.cs"), GetBaristaPlannerModeCode());
        WriteFile(Path.Combine(RootScripts, "Core/BaristaDrinkType.cs"), GetBaristaDrinkTypeCode());
        WriteFile(Path.Combine(RootScripts, "Core/BaristaWelcomeKeys.cs"), GetBaristaWelcomeKeysCode());
        WriteFile(Path.Combine(RootScripts, "Core/BaristaWelcomeState.cs"), GetBaristaWelcomeStateCode());

        WriteFile(Path.Combine(RootScripts, "Config/BaristaWelcomeConfig.cs"), GetBaristaWelcomeConfigCode());

        WriteFile(Path.Combine(RootScripts, "AI/BaristaIntroPlanningSolvers.cs"), GetBaristaIntroPlanningSolversCode());
        WriteFile(Path.Combine(RootScripts, "AI/BaristaWelcomeBrain.cs"), GetBaristaWelcomeBrainCode());

        WriteFile(Path.Combine(RootScripts, "Runtime/BaristaWelcomeController.cs"), GetBaristaWelcomeControllerCode());
        WriteFile(Path.Combine(RootScripts, "Runtime/BaristaWelcomeDebugMenu.cs"), GetBaristaWelcomeDebugMenuCode());

        WriteFile(Path.Combine(RootData, "README_BARISTA_WELCOME_SETUP.txt"), GetReadmeText());

        SessionState.SetBool(MarkerKey, true);
        AssetDatabase.Refresh();

        Debug.Log("[ROAE][Bootstrap] Source files generated. Unity will recompile, then default assets will be created automatically.");
    }

    [DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        if (!SessionState.GetBool(MarkerKey, false))
            return;

        SessionState.SetBool(MarkerKey, false);
        EnsureFolders();
        CreateDefaultAssets();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ROAE][Bootstrap] Barista Welcome scaffolding is ready.");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/ROAE_2");
        EnsureFolder("Assets/ROAE_2/Scripts");
        EnsureFolder("Assets/ROAE_2/Scripts/NarrativeV2");
        EnsureFolder(RootScripts);
        EnsureFolder(Path.Combine(RootScripts, "Core"));
        EnsureFolder(Path.Combine(RootScripts, "Config"));
        EnsureFolder(Path.Combine(RootScripts, "AI"));
        EnsureFolder(Path.Combine(RootScripts, "Runtime"));

        EnsureFolder("Assets/ROAE_2/Data");
        EnsureFolder("Assets/ROAE_2/Data/NarrativeV2");
        EnsureFolder(RootData);
        EnsureFolder(Path.Combine(RootData, "Configs"));

        EnsureFolder("Assets/Editor");
        EnsureFolder(RootEditor);
    }

    private static void EnsureFolder(string assetPath)
    {
        assetPath = assetPath.Replace("\\", "/");
        if (AssetDatabase.IsValidFolder(assetPath))
            return;

        string parent = Path.GetDirectoryName(assetPath).Replace("\\", "/");
        string folderName = Path.GetFileName(assetPath);

        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, folderName);
    }

    private static void WriteFile(string assetPath, string contents)
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        string dir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if (File.Exists(fullPath))
        {
            string existing = File.ReadAllText(fullPath, Encoding.UTF8);
            if (existing == contents)
                return;
        }

        File.WriteAllText(fullPath, contents, new UTF8Encoding(false));
    }

    private static void CreateDefaultAssets()
    {
        string configPath = RootData + "/Configs/BaristaWelcomeConfig_Default.asset";
        Type configType = FindTypeByName("BaristaWelcomeConfig");
        if (configType == null)
        {
            Debug.LogWarning("[ROAE][Bootstrap] Could not find generated type 'BaristaWelcomeConfig' after reload.");
            return;
        }

        UnityEngine.Object config = AssetDatabase.LoadAssetAtPath(configPath, configType);
        if (config == null)
        {
            ScriptableObject instance = ScriptableObject.CreateInstance(configType);
            SerializedObject so = new SerializedObject(instance);

            SerializedProperty plannerMode = so.FindProperty("plannerMode");
            if (plannerMode != null)
                plannerMode.enumValueIndex = 0; // ValueIteration

            SetFloat(so, "gamma", 0.85f);
            SetFloat(so, "epsilon", 0.0001f);
            SetInt(so, "maxIterations", 50);
            SetInt(so, "corruptionThresholdForMischief", 2);
            SetFloat(so, "rewardNeutralBase", 2f);
            SetFloat(so, "rewardNeutralIfLowCorruption", 2f);
            SetFloat(so, "rewardMischievousBase", 1f);
            SetFloat(so, "rewardMischievousIfReadUnknownText", 4f);
            SetFloat(so, "rewardMischievousIfCorruptionHigh", 3f);
            SetFloat(so, "selfLoopProbability", 0.70f);

            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(instance, configPath);
            config = instance;
        }

        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);
    }

    private static Type FindTypeByName(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(typeName);
            if (type != null)
                return type;

            foreach (Type candidate in assembly.GetTypes())
            {
                if (candidate.Name == typeName)
                    return candidate;
            }
        }

        return null;
    }

    private static void SetFloat(SerializedObject so, string propertyName, float value)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p != null)
            p.floatValue = value;
    }

    private static void SetInt(SerializedObject so, string propertyName, int value)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p != null)
            p.intValue = value;
    }

    private static string GetBaristaIntroToneCode() => @"public enum BaristaIntroTone
{
    Neutral = 0,
    Mischievous = 1
}
";

    private static string GetBaristaPlannerModeCode() => @"public enum BaristaPlannerMode
{
    ValueIteration = 0,
    PolicyIteration = 1
}
";

    private static string GetBaristaDrinkTypeCode() => @"public enum BaristaDrinkType
{
    None = 0,
    Cola = 1,
    PhotosyntheticSap = 2
}
";

    private static string GetBaristaWelcomeKeysCode() => @"public static class BaristaWelcomeKeys
{
    public const string ReadUnknownText01 = ""read_unknown_text_01"";
    public const string BaristaIntroDone = ""barista_intro_done"";
    public const string AcceptedFirstDrink = ""accepted_first_drink"";
    public const string DrankPhotosyntheticDrink = ""drank_photosynthetic_drink"";
    public const string DrankCola = ""drank_cola"";
    public const string HeldDrink = ""held_drink"";
    public const string BaristaIntroTone = ""barista_intro_tone"";
    public const string BaristaRelationship = ""relation_barista"";
}
";

    private static string GetBaristaWelcomeStateCode() => @"using UnityEngine;

public static class BaristaWelcomeState
{
    public static bool GetFlag(string key)
    {
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public static void SetFlag(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    public static BaristaDrinkType GetHeldDrink()
    {
        return (BaristaDrinkType)GetInt(BaristaWelcomeKeys.HeldDrink, 0);
    }

    public static void SetHeldDrink(BaristaDrinkType drink)
    {
        SetInt(BaristaWelcomeKeys.HeldDrink, (int)drink);
    }

    public static BaristaIntroTone GetIntroTone()
    {
        return (BaristaIntroTone)GetInt(BaristaWelcomeKeys.BaristaIntroTone, 0);
    }

    public static void SetIntroTone(BaristaIntroTone tone)
    {
        SetInt(BaristaWelcomeKeys.BaristaIntroTone, (int)tone);
    }

    public static int GetBaristaRelationship()
    {
        return GetInt(BaristaWelcomeKeys.BaristaRelationship, 0);
    }

    public static void AdjustBaristaRelationship(int delta)
    {
        SetInt(BaristaWelcomeKeys.BaristaRelationship, GetBaristaRelationship() + delta);
    }

    public static void ApplyNaiveResponseEffects()
    {
        SetFlag(BaristaWelcomeKeys.AcceptedFirstDrink, true);
        AdjustBaristaRelationship(5);
        ApplyStats(0, 1, 2);
    }

    public static void ApplyGuardedResponseEffects()
    {
        SetFlag(BaristaWelcomeKeys.AcceptedFirstDrink, false);
        AdjustBaristaRelationship(-2);
        ApplyStats(0, -1, 0);
    }

    public static void GiveFirstAcceptedDrinkIfPossible()
    {
        if (!GetFlag(BaristaWelcomeKeys.AcceptedFirstDrink))
            return;

        if (GetHeldDrink() != BaristaDrinkType.None)
            return;

        SetHeldDrink(BaristaDrinkType.PhotosyntheticSap);
    }

    public static bool HasHeldDrink()
    {
        return GetHeldDrink() != BaristaDrinkType.None;
    }

    public static bool TryOrderCola()
    {
        if (HasHeldDrink())
            return false;

        SetHeldDrink(BaristaDrinkType.Cola);
        return true;
    }

    public static bool TryOrderPhotosyntheticSap()
    {
        if (HasHeldDrink())
            return false;

        SetHeldDrink(BaristaDrinkType.PhotosyntheticSap);
        SetFlag(BaristaWelcomeKeys.AcceptedFirstDrink, true);
        return true;
    }

    public static bool TryDrinkHeldDrink()
    {
        BaristaDrinkType held = GetHeldDrink();
        if (held == BaristaDrinkType.None)
            return false;

        if (held == BaristaDrinkType.Cola)
            SetFlag(BaristaWelcomeKeys.DrankCola, true);

        if (held == BaristaDrinkType.PhotosyntheticSap)
            SetFlag(BaristaWelcomeKeys.DrankPhotosyntheticDrink, true);

        SetHeldDrink(BaristaDrinkType.None);
        return true;
    }

    public static bool IsMomentComplete()
    {
        return GetFlag(BaristaWelcomeKeys.BaristaIntroDone) && GetFlag(BaristaWelcomeKeys.DrankPhotosyntheticDrink);
    }

    public static void ResetAll()
    {
        SetFlag(BaristaWelcomeKeys.BaristaIntroDone, false);
        SetFlag(BaristaWelcomeKeys.AcceptedFirstDrink, false);
        SetFlag(BaristaWelcomeKeys.DrankPhotosyntheticDrink, false);
        SetFlag(BaristaWelcomeKeys.DrankCola, false);
        SetHeldDrink(BaristaDrinkType.None);
        SetIntroTone(BaristaIntroTone.Neutral);
        SetInt(BaristaWelcomeKeys.BaristaRelationship, 0);
    }

    private static void ApplyStats(int creativityDelta, int empathyDelta, int corruptionDelta)
    {
        if (CreativeCore.Instance == null)
            return;

        CreativeHUD hud = CreativeHUD.Instance;

        if (creativityDelta != 0)
        {
            CreativeCore.Instance.AdjustCreativity(creativityDelta);
            if (hud != null) hud.ShowStatChange(""creativity"", creativityDelta);
        }

        if (empathyDelta != 0)
        {
            CreativeCore.Instance.AdjustEmpathy(empathyDelta);
            if (hud != null) hud.ShowStatChange(""empathy"", empathyDelta);
        }

        if (corruptionDelta != 0)
        {
            CreativeCore.Instance.AdjustCorruption(corruptionDelta);
            if (hud != null) hud.ShowStatChange(""plantCorruption"", corruptionDelta);
        }

        CreativeCore.Instance.PrintStats();
    }
}
";

    private static string GetBaristaWelcomeConfigCode() => @"using UnityEngine;

[CreateAssetMenu(fileName = ""BaristaWelcomeConfig"", menuName = ""ROAE/Barista Welcome Config"")]
public class BaristaWelcomeConfig : ScriptableObject
{
    [Header(""Planner"")]
    public BaristaPlannerMode plannerMode = BaristaPlannerMode.ValueIteration;
    [Range(0f, 0.99f)] public float gamma = 0.85f;
    public float epsilon = 0.0001f;
    public int maxIterations = 50;

    [Header(""Decision thresholds"")]
    public int corruptionThresholdForMischief = 2;

    [Header(""Reward weights"")]
    public float rewardNeutralBase = 2f;
    public float rewardNeutralIfLowCorruption = 2f;
    public float rewardMischievousBase = 1f;
    public float rewardMischievousIfReadUnknownText = 4f;
    public float rewardMischievousIfCorruptionHigh = 3f;

    [Header(""Transition bias"")]
    [Range(0.5f, 0.95f)] public float selfLoopProbability = 0.70f;
}
";

    private static string GetBaristaIntroPlanningSolversCode() => @"using System;

public static class BaristaIntroPlanningSolvers
{
    public static int[] SolveValueIteration(float[,] rewards, float[,,] transitions, float gamma, float epsilon, int maxIterations)
    {
        int stateCount = rewards.GetLength(0);
        int actionCount = rewards.GetLength(1);

        float[] values = new float[stateCount];
        float[] nextValues = new float[stateCount];
        int[] policy = new int[stateCount];

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            float delta = 0f;

            for (int s = 0; s < stateCount; s++)
            {
                float best = float.NegativeInfinity;
                int bestAction = 0;

                for (int a = 0; a < actionCount; a++)
                {
                    float q = rewards[s, a];
                    for (int sp = 0; sp < stateCount; sp++)
                        q += gamma * transitions[s, a, sp] * values[sp];

                    if (q > best)
                    {
                        best = q;
                        bestAction = a;
                    }
                }

                nextValues[s] = best;
                policy[s] = bestAction;
                delta = Math.Max(delta, Math.Abs(nextValues[s] - values[s]));
            }

            Array.Copy(nextValues, values, stateCount);
            if (delta < epsilon)
                break;
        }

        return policy;
    }

    public static int[] SolvePolicyIteration(float[,] rewards, float[,,] transitions, float gamma, float epsilon, int maxIterations)
    {
        int stateCount = rewards.GetLength(0);
        int actionCount = rewards.GetLength(1);

        int[] policy = new int[stateCount];
        float[] values = new float[stateCount];

        bool stable = false;
        int guard = 0;

        while (!stable && guard < maxIterations)
        {
            guard++;

            for (int eval = 0; eval < maxIterations; eval++)
            {
                float delta = 0f;
                float[] nextValues = new float[stateCount];

                for (int s = 0; s < stateCount; s++)
                {
                    int a = policy[s];
                    float v = rewards[s, a];
                    for (int sp = 0; sp < stateCount; sp++)
                        v += gamma * transitions[s, a, sp] * values[sp];

                    nextValues[s] = v;
                    delta = Math.Max(delta, Math.Abs(nextValues[s] - values[s]));
                }

                Array.Copy(nextValues, values, stateCount);
                if (delta < epsilon)
                    break;
            }

            stable = true;

            for (int s = 0; s < stateCount; s++)
            {
                int oldAction = policy[s];
                int bestAction = oldAction;
                float bestQ = float.NegativeInfinity;

                for (int a = 0; a < actionCount; a++)
                {
                    float q = rewards[s, a];
                    for (int sp = 0; sp < stateCount; sp++)
                        q += gamma * transitions[s, a, sp] * values[sp];

                    if (q > bestQ)
                    {
                        bestQ = q;
                        bestAction = a;
                    }
                }

                policy[s] = bestAction;
                if (bestAction != oldAction)
                    stable = false;
            }
        }

        return policy;
    }
}
";

    private static string GetBaristaWelcomeBrainCode() => @"using UnityEngine;

public class BaristaWelcomeBrain : MonoBehaviour
{
    [SerializeField] private BaristaWelcomeConfig config;
    [SerializeField] private bool debugLog = true;

    public BaristaIntroTone DecideOpeningTone()
    {
        if (config == null)
        {
            Debug.LogWarning(""[ROAE][BaristaWelcomeBrain] Missing config. Falling back to Neutral.""
            );
            return BaristaIntroTone.Neutral;
        }

        float[,] rewards;
        float[,,] transitions;
        BuildMdp(out rewards, out transitions);

        int[] policy = config.plannerMode == BaristaPlannerMode.PolicyIteration
            ? BaristaIntroPlanningSolvers.SolvePolicyIteration(rewards, transitions, config.gamma, config.epsilon, config.maxIterations)
            : BaristaIntroPlanningSolvers.SolveValueIteration(rewards, transitions, config.gamma, config.epsilon, config.maxIterations);

        int stateIndex = EncodeCurrentState();
        BaristaIntroTone tone = (BaristaIntroTone)policy[stateIndex];
        BaristaWelcomeState.SetIntroTone(tone);

        if (debugLog)
        {
            Debug.Log(""[ROAE][BaristaWelcomeBrain] planner="" + config.plannerMode +
                      "" stateIndex="" + stateIndex +
                      "" readUnknownText="" + BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.ReadUnknownText01) +
                      "" corruption="" + GetCorruptionValue() +
                      "" resultTone="" + tone);
        }

        return tone;
    }

    private int EncodeCurrentState()
    {
        bool readUnknownText = BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.ReadUnknownText01);
        bool highCorruption = GetCorruptionValue() >= config.corruptionThresholdForMischief;

        int state = 0;
        if (readUnknownText) state += 2;
        if (highCorruption) state += 1;
        return state;
    }

    private int GetCorruptionValue()
    {
        if (CreativeCore.Instance == null)
            return 0;

        return CreativeCore.Instance.plantCorruption;
    }

    private void BuildMdp(out float[,] rewards, out float[,,] transitions)
    {
        rewards = new float[4, 2];
        transitions = new float[4, 2, 4];

        for (int state = 0; state < 4; state++)
        {
            bool readUnknownText = (state & 2) != 0;
            bool highCorruption = (state & 1) != 0;

            rewards[state, 0] = ComputeNeutralReward(readUnknownText, highCorruption);
            rewards[state, 1] = ComputeMischievousReward(readUnknownText, highCorruption);

            FillTransitionsForNeutral(state, transitions);
            FillTransitionsForMischievous(state, transitions);
        }
    }

    private float ComputeNeutralReward(bool readUnknownText, bool highCorruption)
    {
        float r = config.rewardNeutralBase;
        if (!highCorruption)
            r += config.rewardNeutralIfLowCorruption;
        if (readUnknownText)
            r -= 1f;
        return r;
    }

    private float ComputeMischievousReward(bool readUnknownText, bool highCorruption)
    {
        float r = config.rewardMischievousBase;
        if (readUnknownText)
            r += config.rewardMischievousIfReadUnknownText;
        if (highCorruption)
            r += config.rewardMischievousIfCorruptionHigh;
        return r;
    }

    private void FillTransitionsForNeutral(int state, float[,,] transitions)
    {
        int clearState = state & 2;
        float stay = config.selfLoopProbability;
        float improve = 1f - stay;

        transitions[state, 0, state] = stay;
        transitions[state, 0, clearState] += improve;
    }

    private void FillTransitionsForMischievous(int state, float[,,] transitions)
    {
        int elevatedState = state | 1;
        float stay = config.selfLoopProbability;
        float worsen = 1f - stay;

        transitions[state, 1, state] = stay;
        transitions[state, 1, elevatedState] += worsen;
    }
}
";

    private static string GetBaristaWelcomeControllerCode() => @"using UnityEngine;

public class BaristaWelcomeController : MonoBehaviour
{
    [SerializeField] private BaristaWelcomeBrain brain;
    [SerializeField] private bool debugLog = true;

    public BaristaIntroTone ResolveOpeningTone()
    {
        BaristaIntroTone tone = brain != null ? brain.DecideOpeningTone() : BaristaIntroTone.Neutral;
        if (debugLog)
            Debug.Log(""[ROAE][BaristaWelcomeController] ResolveOpeningTone result="" + tone);
        return tone;
    }

    public void MarkIntroDone()
    {
        BaristaWelcomeState.SetFlag(BaristaWelcomeKeys.BaristaIntroDone, true);
        if (debugLog)
            Debug.Log(""[ROAE][BaristaWelcomeController] Intro marked done.""
            );
    }

    public void ApplyNaiveResponse()
    {
        BaristaWelcomeState.ApplyNaiveResponseEffects();
        if (debugLog)
            Debug.Log(""[ROAE][BaristaWelcomeController] Applied naive response effects.""
            );
    }

    public void ApplyGuardedResponse()
    {
        BaristaWelcomeState.ApplyGuardedResponseEffects();
        if (debugLog)
            Debug.Log(""[ROAE][BaristaWelcomeController] Applied guarded response effects.""
            );
    }

    public void GiveAcceptedDrinkIfPossible()
    {
        BaristaWelcomeState.GiveFirstAcceptedDrinkIfPossible();
        if (debugLog)
            Debug.Log(""[ROAE][BaristaWelcomeController] GiveAcceptedDrinkIfPossible heldDrink="" + BaristaWelcomeState.GetHeldDrink());
    }

    public bool ShouldShowWaitLine()
    {
        return BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.AcceptedFirstDrink)
            && BaristaWelcomeState.GetHeldDrink() == BaristaDrinkType.None
            && !BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.DrankPhotosyntheticDrink);
    }

    public bool ShouldShowOrderMenu()
    {
        return !BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.AcceptedFirstDrink)
            && !BaristaWelcomeState.HasHeldDrink();
    }

    public bool HasHeldDrink()
    {
        return BaristaWelcomeState.HasHeldDrink();
    }

    public BaristaDrinkType GetHeldDrink()
    {
        return BaristaWelcomeState.GetHeldDrink();
    }

    public bool TryOrderCola()
    {
        bool ok = BaristaWelcomeState.TryOrderCola();
        if (debugLog)
            Debug.Log(""[ROAE][BaristaWelcomeController] TryOrderCola success="" + ok + "" heldDrink="" + BaristaWelcomeState.GetHeldDrink());
        return ok;
    }

    public bool TryOrderPhotosyntheticSap()
    {
        bool ok = BaristaWelcomeState.TryOrderPhotosyntheticSap();
        if (debugLog)
            Debug.Log(""[ROAE][BaristaWelcomeController] TryOrderPhotosyntheticSap success="" + ok + "" heldDrink="" + BaristaWelcomeState.GetHeldDrink());
        return ok;
    }

    public bool TryDrinkHeldDrink()
    {
        bool ok = BaristaWelcomeState.TryDrinkHeldDrink();
        if (debugLog)
            Debug.Log(""[ROAE][BaristaWelcomeController] TryDrinkHeldDrink success="" + ok +
                      "" drankCola="" + BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.DrankCola) +
                      "" drankPhotosynthetic="" + BaristaWelcomeState.GetFlag(BaristaWelcomeKeys.DrankPhotosyntheticDrink));
        return ok;
    }

    public bool IsMomentComplete()
    {
        return BaristaWelcomeState.IsMomentComplete();
    }

    public void ResetMomentState()
    {
        BaristaWelcomeState.ResetAll();
        if (debugLog)
            Debug.Log(""[ROAE][BaristaWelcomeController] Moment reset.""
            );
    }
}
";

    private static string GetBaristaWelcomeDebugMenuCode() => @"using UnityEngine;

public class BaristaWelcomeDebugMenu : MonoBehaviour
{
    [SerializeField] private BaristaWelcomeController controller;

    [ContextMenu(""Barista Welcome/Reset State"")]
    public void ResetState()
    {
        if (controller != null)
            controller.ResetMomentState();
    }

    [ContextMenu(""Barista Welcome/Resolve Opening Tone"")]
    public void ResolveTone()
    {
        if (controller != null)
            controller.ResolveOpeningTone();
    }

    [ContextMenu(""Barista Welcome/Apply Naive Response"")]
    public void ApplyNaive()
    {
        if (controller != null)
            controller.ApplyNaiveResponse();
    }

    [ContextMenu(""Barista Welcome/Apply Guarded Response"")]
    public void ApplyGuarded()
    {
        if (controller != null)
            controller.ApplyGuardedResponse();
    }

    [ContextMenu(""Barista Welcome/Give Accepted Drink"")]
    public void GiveAcceptedDrink()
    {
        if (controller != null)
            controller.GiveAcceptedDrinkIfPossible();
    }

    [ContextMenu(""Barista Welcome/Order Cola"")]
    public void OrderCola()
    {
        if (controller != null)
            controller.TryOrderCola();
    }

    [ContextMenu(""Barista Welcome/Order Photosynthetic Sap"")]
    public void OrderPhotosyntheticSap()
    {
        if (controller != null)
            controller.TryOrderPhotosyntheticSap();
    }

    [ContextMenu(""Barista Welcome/Drink Held Drink"")]
    public void DrinkHeldDrink()
    {
        if (controller != null)
            controller.TryDrinkHeldDrink();
    }
}
";

    private static string GetReadmeText() =>
@"ROAE BARISTA WELCOME SCAFFOLDING

What this bootstrap created:
- Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome/Core
- Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome/Config
- Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome/AI
- Assets/ROAE_2/Scripts/NarrativeV2/BaristaWelcome/Runtime
- Assets/ROAE_2/Data/NarrativeV2/BaristaWelcome/Configs/BaristaWelcomeConfig_Default.asset

How to use the vertical slice:
1. Create an empty GameObject in BarInside scene called BaristaWelcomeSystem.
2. Add BaristaWelcomeBrain.
3. Add BaristaWelcomeController.
4. Assign BaristaWelcomeConfig_Default to the brain.
5. Optional: add BaristaWelcomeDebugMenu for right-click context debugging.
6. Use controller methods from buttons / ActionLists / events:
   - ResolveOpeningTone
   - ApplyNaiveResponse
   - ApplyGuardedResponse
   - GiveAcceptedDrinkIfPossible
   - TryOrderCola
   - TryOrderPhotosyntheticSap
   - TryDrinkHeldDrink
7. Completion condition:
   - barista_intro_done == true
   - drank_photosynthetic_drink == true

Recommended next manual cleanup:
- keep DialogueTrigger
- keep CreativeCore
- keep CreativeHUD
- stop growing DialogueTrigger2
- stop using DialogueFlags for new content

Planning highlight:
- plannerMode = ValueIteration uses dynamic programming for best opening tone
- plannerMode = PolicyIteration uses policy evaluation + policy improvement
";
}
