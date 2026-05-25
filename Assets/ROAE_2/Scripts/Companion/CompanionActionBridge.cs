using System.Collections.Generic;
using UnityEngine;

public class CompanionActionBridge : MonoBehaviour
{
    [SerializeField] private CompanionSummonPoint summonPoint;
    [SerializeField] private CompanionObservationTarget observationTarget;
    [SerializeField] private CompanionDialogueIntent intent = CompanionDialogueIntent.Ambient;
    [SerializeField] private List<CompanionEnvironmentTag> extraTags = new List<CompanionEnvironmentTag>();

    public void TrySummon()
    {
        if (CompanionSystem.Instance == null)
            return;

        if (summonPoint != null)
            CompanionSystem.Instance.TrySummon(summonPoint);
        else
            CompanionSystem.Instance.TrySummonAtNearestPoint();
    }

    public void DespawnCompanion()
    {
        CompanionSystem.Instance?.Despawn("bridge");
    }

    public void Speak()
    {
        CompanionSystem.Instance?.TrySpeak(new CompanionSpeechRequest
        {
            intent = intent,
            extraTags = new List<CompanionEnvironmentTag>(extraTags),
            focusId = gameObject.name
        });
    }

    public void Observe()
    {
        if (observationTarget != null)
            observationTarget.Observe();
    }

    public void UnlockDialoguePool(string poolId)
    {
        CompanionSystem.Instance?.UnlockDialoguePool(poolId);
    }

    public void SetNarrativeFlag(string flagKey)
    {
        CompanionSystem.Instance?.SetNarrativeFlag(flagKey, true);
    }

    public void ClearNarrativeFlag(string flagKey)
    {
        CompanionSystem.Instance?.SetNarrativeFlag(flagKey, false);
    }

    public void RaiseSelfAwareness(int amount)
    {
        CompanionSystem.Instance?.AdjustSelfAwareness(amount);
    }

    public void RaiseContradiction(int amount)
    {
        CompanionSystem.Instance?.AdjustContradiction(amount);
    }
}
