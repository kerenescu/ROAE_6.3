using System.Collections.Generic;
using UnityEngine;

public class CompanionSummonPoint : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string pointId = "summon_point";
    [SerializeField] private CompanionSummonPointType pointType = CompanionSummonPointType.Generic;
    [SerializeField] private Transform anchor;

    [Header("Behavior")]
    [SerializeField] private bool isEmotionallySafeSpace = true;
    [SerializeField] private bool allowManualSummon = true;
    [SerializeField] private bool autoSummonOnPlayerEnter;
    [SerializeField] private bool autoSpeakOnSummon = true;
    [SerializeField] private bool ignoreCorruptionRestrictionForTesting;
    [SerializeField] private bool requirePlayerInsideTrigger = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Atmosphere")]
    [SerializeField] private CompanionThreatLevel ambientThreatLevel = CompanionThreatLevel.None;
    [SerializeField] private List<CompanionEnvironmentTag> ambientTags = new List<CompanionEnvironmentTag>();

    [Header("Restrictions")]
    [SerializeField] private CompanionConditionSet summonConditions = new CompanionConditionSet();

    private int playerInsideCount;

    public string PointId => string.IsNullOrWhiteSpace(pointId) ? name : pointId.Trim();
    public CompanionSummonPointType PointType => pointType;
    public bool IsEmotionallySafeSpace => isEmotionallySafeSpace;
    public bool AllowManualSummon => allowManualSummon;
    public bool AutoSummonOnPlayerEnter => autoSummonOnPlayerEnter;
    public bool AutoSpeakOnSummon => autoSpeakOnSummon;
    public bool IgnoreCorruptionRestrictionForTesting => ignoreCorruptionRestrictionForTesting;
    public bool IsPlayerNearby => !requirePlayerInsideTrigger || playerInsideCount > 0;
    public CompanionThreatLevel AmbientThreatLevel => ambientThreatLevel;
    public CompanionConditionSet SummonConditions => summonConditions;

    private void OnEnable()
    {
        CompanionSystem.Instance?.RegisterSummonPoint(this);
    }

    private void OnDisable()
    {
        if (CompanionSystem.Instance != null)
            CompanionSystem.Instance.UnregisterSummonPoint(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInsideCount++;
        Debug.Log(
            "[ROAE][CompanionSummonPoint] Player entered point=" + PointId +
            " object=" + other.name +
            " autoSummon=" + autoSummonOnPlayerEnter +
            " ignoreCorruption=" + ignoreCorruptionRestrictionForTesting +
            " safeSpace=" + isEmotionallySafeSpace);
        CompanionSystem.Instance?.NotifyPointAvailability(this, true);

        if (autoSummonOnPlayerEnter)
            TrySummonHere();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInsideCount = Mathf.Max(0, playerInsideCount - 1);
        CompanionSystem.Instance?.NotifyPointAvailability(this, IsPlayerNearby);
    }

    public Vector3 GetAnchorPosition()
    {
        return anchor != null ? anchor.position : transform.position;
    }

    public List<CompanionEnvironmentTag> BuildTags()
    {
        List<CompanionEnvironmentTag> tags = new List<CompanionEnvironmentTag>();
        if (ambientTags != null)
            tags.AddRange(ambientTags);

        switch (pointType)
        {
            case CompanionSummonPointType.Mirror:
                AddTag(tags, CompanionEnvironmentTag.Reflection);
                AddTag(tags, CompanionEnvironmentTag.Falsehood);
                break;

            case CompanionSummonPointType.Water:
                AddTag(tags, CompanionEnvironmentTag.Water);
                AddTag(tags, CompanionEnvironmentTag.Reflection);
                break;

            case CompanionSummonPointType.GlowingPlant:
                AddTag(tags, CompanionEnvironmentTag.Bloom);
                AddTag(tags, CompanionEnvironmentTag.Warmth);
                break;

            case CompanionSummonPointType.QuietSpace:
                AddTag(tags, CompanionEnvironmentTag.Silence);
                AddTag(tags, CompanionEnvironmentTag.Shelter);
                break;
        }

        if (isEmotionallySafeSpace)
            AddTag(tags, CompanionEnvironmentTag.Shelter);

        return tags;
    }

    public bool TrySummonHere()
    {
        return CompanionSystem.Instance != null && CompanionSystem.Instance.TrySummon(this);
    }

    private static void AddTag(List<CompanionEnvironmentTag> tags, CompanionEnvironmentTag tag)
    {
        if (tags != null && !tags.Contains(tag))
            tags.Add(tag);
    }
}
