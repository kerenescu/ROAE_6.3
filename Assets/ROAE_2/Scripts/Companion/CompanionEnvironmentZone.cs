using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CompanionEnvironmentZone : MonoBehaviour
{
    [SerializeField] private string zoneId = "zone";
    [SerializeField] private bool contributesSafeSpace = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private CompanionThreatLevel threatLevel = CompanionThreatLevel.None;
    [SerializeField] private List<CompanionEnvironmentTag> tags = new List<CompanionEnvironmentTag>();

    public string ZoneId => string.IsNullOrWhiteSpace(zoneId) ? name : zoneId.Trim();
    public bool ContributesSafeSpace => contributesSafeSpace;
    public CompanionThreatLevel ThreatLevel => threatLevel;
    public IReadOnlyList<CompanionEnvironmentTag> Tags => tags;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            CompanionSystem.Instance?.EnterZone(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            CompanionSystem.Instance?.ExitZone(this);
    }
}
