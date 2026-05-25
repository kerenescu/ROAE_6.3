using UnityEngine;

public static class CreativeStatScale
{
    public const int MinValue = 0;
    public const int MaxValue = 100;

    public const int DefaultCreativity = 50;
    public const int DefaultEmpathy = 50;
    public const int DefaultCorruption = 0;

    public const int DevResetCreativity = 40;
    public const int DevResetEmpathy = 50;
    public const int DevResetCorruption = 0;

    public const int CreativityLowMax = 34;
    public const int CreativityHighMin = 67;
    public const int EmpathyLowMax = 34;
    public const int EmpathyHighMin = 67;
    public const int CorruptionLowMax = 29;
    public const int CorruptionHighMin = 70;
    public const int RelationshipBadMax = -2;
    public const int RelationshipGoodMin = 2;

    public const int LegacyEmpathyMin = -5;
    public const int LegacyEmpathyMax = 5;
    public const int LegacyCorruptionMin = 0;
    public const int LegacyCorruptionMax = 10;
    public const int LegacyDeltaStepMultiplier = 10;
    public const int CurrentScaleVersion = 2;

    public static int ClampCreativity(int value) => Mathf.Clamp(value, MinValue, MaxValue);
    public static int ClampEmpathy(int value) => Mathf.Clamp(value, MinValue, MaxValue);
    public static int ClampCorruption(int value) => Mathf.Clamp(value, MinValue, MaxValue);

    public static int ConvertLegacyEmpathyValue(int legacyValue)
    {
        int normalized = legacyValue - LegacyEmpathyMin;
        return ClampEmpathy(normalized * 10);
    }

    public static int ConvertLegacyCorruptionValue(int legacyValue)
    {
        return ClampCorruption(Mathf.Max(LegacyCorruptionMin, legacyValue) * 10);
    }

    public static int ConvertLegacyEmpathyDelta(int legacyDelta)
    {
        return legacyDelta * LegacyDeltaStepMultiplier;
    }

    public static int ConvertLegacyCorruptionDelta(int legacyDelta)
    {
        return legacyDelta * LegacyDeltaStepMultiplier;
    }

    public static bool LooksLikeLegacyEmpathyThresholds(int lowMax, int highMin)
    {
        return lowMax >= LegacyEmpathyMin &&
               lowMax <= LegacyEmpathyMax &&
               highMin >= LegacyEmpathyMin &&
               highMin <= LegacyEmpathyMax;
    }

    public static bool LooksLikeLegacyCorruptionThresholds(int lowMax, int highMin)
    {
        return lowMax >= LegacyCorruptionMin &&
               lowMax <= LegacyCorruptionMax &&
               highMin >= LegacyCorruptionMin &&
               highMin <= LegacyCorruptionMax;
    }

    public static int ConvertLegacyEmpathyThreshold(int legacyValue)
    {
        return ConvertLegacyEmpathyValue(legacyValue);
    }

    public static int ConvertLegacyCorruptionLowMax(int legacyValue)
    {
        return ClampCorruption((legacyValue * 10) + 9);
    }

    public static int ConvertLegacyCorruptionHighMin(int legacyValue)
    {
        return ClampCorruption(legacyValue * 10);
    }

    public static CreativityBucket BucketCreativity(int value)
    {
        if (value <= CreativityLowMax)
            return CreativityBucket.Low;

        if (value >= CreativityHighMin)
            return CreativityBucket.High;

        return CreativityBucket.Medium;
    }

    public static EmpathyBucket BucketEmpathy(int value)
    {
        if (value <= EmpathyLowMax)
            return EmpathyBucket.Low;

        if (value >= EmpathyHighMin)
            return EmpathyBucket.High;

        return EmpathyBucket.Neutral;
    }

    public static CorruptionBucket BucketCorruption(int value)
    {
        if (value <= CorruptionLowMax)
            return CorruptionBucket.Low;

        if (value >= CorruptionHighMin)
            return CorruptionBucket.High;

        return CorruptionBucket.Medium;
    }

    public static RelationshipBucket BucketRelationship(int value)
    {
        if (value <= RelationshipBadMax)
            return RelationshipBucket.Bad;

        if (value >= RelationshipGoodMin)
            return RelationshipBucket.Good;

        return RelationshipBucket.Neutral;
    }
}

public class CreativeCore : MonoBehaviour
{
    private const string ScaleVersionKey = "creative_stats_scale_version";

    public static CreativeCore Instance;

    [Header("Valori principale")]
    [Range(0, 100)]
    public int creativity = CreativeStatScale.DefaultCreativity;

    [Range(0, 100)]
    public int empathy = CreativeStatScale.DefaultEmpathy;

    [Range(0, 100)]
    public int plantCorruption = CreativeStatScale.DefaultCorruption;

    public int Creativity => creativity;
    public int Empathy => empathy;
    public int PlantCorruption => plantCorruption;

    public void ForceSetStats(int newCreativity, int newEmpathy, int newCorruption)
    {
        if (Instance != null && Instance != this)
        {
            Instance.ForceSetStats(newCreativity, newEmpathy, newCorruption);
            return;
        }

        creativity = CreativeStatScale.ClampCreativity(newCreativity);
        empathy = CreativeStatScale.ClampEmpathy(newEmpathy);
        plantCorruption = CreativeStatScale.ClampCorruption(newCorruption);
        SaveStatsToPrefs();

        Debug.Log("[STATS][ForceSetStats] creativity=" + creativity +
                  " empathy=" + empathy +
                  " plantCorruption=" + plantCorruption);

        PrintStats();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            MigrateLegacyScaleIfNeeded();
            ClampAllStats();
            SaveStatsToPrefs();
            Debug.Log("[STATS][CreativeCore.Awake] instance ready");
        }
        else
        {
            Debug.Log("[STATS][CreativeCore.Awake] duplicate instance destroyed");
            Destroy(gameObject);
        }
    }

    public void AdjustCreativity(int amount)
    {
        int before = creativity;
        creativity = CreativeStatScale.ClampCreativity(creativity + amount);
        SaveStatsToPrefs();
        Debug.Log("[STATS][AdjustCreativity] before=" + before + " delta=" + amount + " after=" + creativity);
    }

    public void AdjustEmpathy(int amount)
    {
        int before = empathy;
        empathy = CreativeStatScale.ClampEmpathy(empathy + amount);
        SaveStatsToPrefs();
        Debug.Log("[STATS][AdjustEmpathy] before=" + before + " delta=" + amount + " after=" + empathy);
    }

    public void AdjustCorruption(int amount)
    {
        int before = plantCorruption;
        plantCorruption = CreativeStatScale.ClampCorruption(plantCorruption + amount);
        SaveStatsToPrefs();
        Debug.Log("[STATS][AdjustCorruption] before=" + before + " delta=" + amount + " after=" + plantCorruption);
    }

    public void PrintStats()
    {
        Debug.Log("[STATS][PrintStats] creativity=" + creativity + " empathy=" + empathy + " plantCorruption=" + plantCorruption);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[STATS][Hotkey] key=T action=Creativity+10");
            AdjustCreativity(10);
            PrintStats();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("[STATS][Hotkey] key=Y action=Empathy+10");
            AdjustEmpathy(10);
            PrintStats();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("[STATS][Hotkey] key=U action=Corruption+10");
            AdjustCorruption(10);
            PrintStats();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("[STATS][Hotkey] key=G action=Creativity-10");
            AdjustCreativity(-10);
            PrintStats();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("[STATS][Hotkey] key=H action=Empathy-10");
            AdjustEmpathy(-10);
            PrintStats();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("[STATS][Hotkey] key=J action=Corruption-10");
            AdjustCorruption(-10);
            PrintStats();
        }
    }

    private void ClampAllStats()
    {
        creativity = CreativeStatScale.ClampCreativity(creativity);
        empathy = CreativeStatScale.ClampEmpathy(empathy);
        plantCorruption = CreativeStatScale.ClampCorruption(plantCorruption);
    }

    private void SaveStatsToPrefs()
    {
        PlayerPrefs.SetInt("creativity", creativity);
        PlayerPrefs.SetInt("empathy", empathy);
        PlayerPrefs.SetInt("plantCorruption", plantCorruption);
        PlayerPrefs.SetInt(ScaleVersionKey, CreativeStatScale.CurrentScaleVersion);
        PlayerPrefs.Save();
    }

    private void MigrateLegacyScaleIfNeeded()
    {
        int storedVersion = PlayerPrefs.GetInt(ScaleVersionKey, 0);
        if (storedVersion >= CreativeStatScale.CurrentScaleVersion)
            return;

        creativity = PlayerPrefs.HasKey("creativity")
            ? CreativeStatScale.ClampCreativity(PlayerPrefs.GetInt("creativity"))
            : CreativeStatScale.ClampCreativity(creativity);

        if (PlayerPrefs.HasKey("empathy"))
        {
            empathy = CreativeStatScale.ConvertLegacyEmpathyValue(PlayerPrefs.GetInt("empathy"));
        }
        else if (empathy >= CreativeStatScale.LegacyEmpathyMin && empathy <= CreativeStatScale.LegacyEmpathyMax)
        {
            empathy = CreativeStatScale.ConvertLegacyEmpathyValue(empathy);
        }
        else
        {
            empathy = CreativeStatScale.ClampEmpathy(empathy);
        }

        if (PlayerPrefs.HasKey("plantCorruption"))
        {
            plantCorruption = CreativeStatScale.ConvertLegacyCorruptionValue(PlayerPrefs.GetInt("plantCorruption"));
        }
        else if (plantCorruption >= CreativeStatScale.LegacyCorruptionMin &&
                 plantCorruption <= CreativeStatScale.LegacyCorruptionMax)
        {
            plantCorruption = CreativeStatScale.ConvertLegacyCorruptionValue(plantCorruption);
        }
        else
        {
            plantCorruption = CreativeStatScale.ClampCorruption(plantCorruption);
        }
    }
}
