using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNpcResponseSet", menuName = "ROAE/NPC/Response Set")]
public class NpcResponseSet : ScriptableObject
{
    [SerializeField] private List<NpcResponseEntry> responses = new List<NpcResponseEntry>();

    public IReadOnlyList<NpcResponseEntry> Responses => responses;

    public DialogueData ResolveDialogue(NpcDecisionContext context, NpcActionType action)
    {
        NpcResponseEntry best = null;
        int bestPriority = int.MinValue;

        for (int i = 0; i < responses.Count; i++)
        {
            NpcResponseEntry entry = responses[i];
            if (entry == null || !entry.Matches(context, action))
                continue;

            if (best == null || entry.Priority > bestPriority)
            {
                best = entry;
                bestPriority = entry.Priority;
            }
        }

        return best != null ? best.Dialogue : null;
    }
}

[System.Serializable]
public class NpcResponseEntry
{
    [SerializeField] private string responseId = "response";
    [SerializeField] private NpcActionType action = NpcActionType.Neutral;
    [SerializeField] private int priority;
    [SerializeField] private DialogueData dialogue;

    [Header("Narrative gates")]
    [SerializeField] private string requiredChapterId = "";
    [SerializeField] private string requiredSceneId = "";
    [SerializeField] private string requiredNarrativeMomentId = "";
    [SerializeField] private List<string> requiredTrueFlags = new List<string>();
    [SerializeField] private List<string> requiredFalseFlags = new List<string>();

    public string ResponseId => responseId;
    public NpcActionType Action => action;
    public int Priority => priority;
    public DialogueData Dialogue => dialogue;

    public bool Matches(NpcDecisionContext context, NpcActionType candidateAction)
    {
        if (candidateAction != action || dialogue == null)
            return false;

        if (!NarrativeProgressState.MatchesCurrentChapter(requiredChapterId))
            return false;

        if (!NarrativeProgressState.MatchesCurrentScene(requiredSceneId))
            return false;

        if (!NarrativeProgressState.MatchesCurrentMoment(requiredNarrativeMomentId))
            return false;

        if (!NpcDecisionContextFlags.AllMatch(requiredTrueFlags, true))
            return false;

        if (!NpcDecisionContextFlags.AllMatch(requiredFalseFlags, false))
            return false;

        return true;
    }
}
