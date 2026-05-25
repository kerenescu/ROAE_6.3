using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CompanionReactionTrigger : MonoBehaviour
{
    [SerializeField] private string triggerId = "reaction_trigger";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool oneShot = true;
    [SerializeField] private bool requireVisibleCompanion = true;
    [SerializeField] private bool markAsDiscovery = true;
    [SerializeField] private string discoveryId = string.Empty;
    [SerializeField] private CompanionDialogueIntent intent = CompanionDialogueIntent.Reaction;
    [SerializeField] private List<CompanionEnvironmentTag> extraTags = new List<CompanionEnvironmentTag>();

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered && oneShot)
            return;

        if (!other.CompareTag(playerTag))
            return;

        CompanionSystem system = CompanionSystem.Instance;
        if (system == null)
            return;

        if (requireVisibleCompanion && !system.IsCompanionVisible)
            return;

        triggered = true;

        if (markAsDiscovery)
            system.MarkInteractionDiscovered(string.IsNullOrWhiteSpace(discoveryId) ? triggerId : discoveryId);

        system.PulseCompanion(0.8f);
        system.TrySpeak(new CompanionSpeechRequest
        {
            intent = intent,
            extraTags = new List<CompanionEnvironmentTag>(extraTags),
            focusId = triggerId
        });
    }
}
