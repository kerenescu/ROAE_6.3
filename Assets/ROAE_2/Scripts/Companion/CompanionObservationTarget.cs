using System.Collections.Generic;
using UnityEngine;

public class CompanionObservationTarget : MonoBehaviour
{
    [SerializeField] private string targetId = "observation";
    [SerializeField] private Transform focusPoint;
    [SerializeField] private bool hiddenThing;
    [SerializeField] private bool requestHintOnObserve = true;
    [SerializeField] private bool oneShotHint;
    [SerializeField] private float pulseStrength = 0.7f;
    [SerializeField] private List<CompanionEnvironmentTag> tags = new List<CompanionEnvironmentTag>();
    [SerializeField] private string discoveryId = string.Empty;

    private bool consumed;

    public string TargetId => string.IsNullOrWhiteSpace(targetId) ? name : targetId.Trim();
    public bool HiddenThing => hiddenThing;
    public bool RequestHintOnObserve => requestHintOnObserve;
    public float PulseStrength => pulseStrength;
    public string DiscoveryId => discoveryId;
    public IReadOnlyList<CompanionEnvironmentTag> Tags => tags;

    public Vector3 GetFocusPosition()
    {
        return focusPoint != null ? focusPoint.position : transform.position;
    }

    public bool CanObserve()
    {
        return !oneShotHint || !consumed;
    }

    public void Observe()
    {
        if (!CanObserve())
            return;

        consumed = true;
        CompanionSystem.Instance?.ObserveTarget(this);
    }
}
