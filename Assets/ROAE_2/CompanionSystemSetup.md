# ROAE Companion System

## Runtime Pieces

- `CompanionSystem`: persistent orchestration layer for summon rules, relationship drift, save/load, emotional state resolution, and dialogue routing.
- `CompanionSummonPoint`: place this in mirrors, water edges, glowing plants, and quiet shelters; it gates when the companion may appear.
- `CompanionEnvironmentZone`: feeds emotional tags like `Sadness`, `Falsehood`, `Warmth`, or `Loneliness` into the system while Rina is inside a space.
- `CompanionObservationTarget`: marks objects the companion can quietly notice, face, pulse toward, and hint around without solving the puzzle outright.
- `CompanionReactionTrigger`: one-shot or repeatable emotional beats for scene scripting and hidden discoveries.
- `CompanionActionBridge`: bridge component for Adventure Creator interactions, triggers, or custom event objects.

## Authoring Assets

- `CompanionProfile.asset`: summon cooldowns, safe-space thresholds, overuse/abandonment tuning, emotion rules, unpredictability.
- `CompanionDialogueLibrary.asset`: non-literal hint lines, comfort lines, warning lines, contradiction lines, and their state/tag conditions.

Use `ROAE/Companion/Create Starter Assets` to generate both assets.
Use `ROAE/Companion/Create Runtime Host In Scene` to place the manager in the active scene.
Use `ROAE/Companion/Build Melc Companion From Frames` to generate the Melc summon clip, idle clip, animator controller, and manifestation prefab from `Assets/ROAE_2/characters/Melc/Frames`.
Use `ROAE/Companion/Create Quiet Safe Summon Cluster` or `ROAE/Companion/Create Mirror Summon Cluster` to inject a ready-made summon setup directly into the currently open scene.

## Suggested Folder Layout

- `Assets/ROAE_2/Scripts/Companion`
- `Assets/ROAE_2/Data/Companion`
- `Assets/ROAE_2/Prefabs/Companion`
- `Assets/ROAE_2/Audio/Companion`
- `Assets/ROAE_2/VFX/Companion`

## Prefab Hierarchy

- `CompanionManifestation`
- `ArtRoot`
- `ShellRoot`
- `Glow`
- `Particles/Summon`
- `Particles/Pulse`
- `Particles/Trail`
- `Audio`

Attach `CompanionManifestationController` on the prefab root and wire the existing summon frames / animator there.

## Scene Integration Flow

1. Add or keep a single `CompanionSystem` in bootstrap or persistent scene flow.
2. Place `CompanionSummonPoint` on safe anchors where summon is emotionally justified.
3. Wrap areas with `CompanionEnvironmentZone` to feed mood tags.
4. Tag mirrors, suspicious flowers, hidden props, or memory objects with `CompanionObservationTarget`.
5. Use `CompanionActionBridge` from hotspots / AC actions to call summon, speak, observe, unlock dialogue pools, or set narrative flags.

## Save / Load

The system stores its state as JSON in PlayerPrefs via `CompanionPersistence`.
Persisted values include:

- affection
- trust
- irritation
- abandonment
- self-awareness
- contradiction index
- summon history
- discovered interactions
- unlocked dialogue pools
- narrative flags
- dialogue cooldown memory

## Narrative Hooks

Late-game ambiguity lives in:

- `selfAwareness`
- `contradictionIndex`
- `CompanionNarrativeReliability`
- `requiredTrueFlags` / `requiredFalseFlags`
- `unlockPoolId`

Raise those through `CompanionActionBridge` or direct scene scripting when the story starts questioning whether the companion is external, internal, or both.

## Performance Notes

- The manager is event-driven for scene triggers and only reevaluates mood on a short configurable interval.
- Summon history trims itself to the configured recent-use window.
- Dialogue selection is data-driven and avoids repeated lines with per-entry cooldown memory.
- Manifestation visuals update only while the companion is present.
