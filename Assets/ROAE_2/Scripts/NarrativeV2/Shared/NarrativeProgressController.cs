using UnityEngine;

public class NarrativeProgressController : MonoBehaviour
{
    [SerializeField] private bool debugLog = true;

    public void SetChapterId(string chapterId)
    {
        NarrativeProgressState.SetCurrentChapterId(chapterId);
        Log("chapter=" + NarrativeProgressState.GetCurrentChapterId());
    }

    public void SetMomentId(string momentId)
    {
        NarrativeProgressState.SetCurrentMomentId(momentId);
        Log("moment=" + NarrativeProgressState.GetCurrentMomentId());
    }

    public void OverrideSceneId(string sceneId)
    {
        NarrativeProgressState.SetSceneOverride(sceneId);
        Log("sceneOverride=" + NarrativeProgressState.GetCurrentSceneId());
    }

    public void ClearSceneOverride()
    {
        NarrativeProgressState.ClearSceneOverride();
        Log("sceneOverride cleared; activeScene=" + NarrativeProgressState.GetCurrentSceneId());
    }

    public void PrintCurrentProgress()
    {
        Log(
            "chapter=" + NarrativeProgressState.GetCurrentChapterId() +
            " scene=" + NarrativeProgressState.GetCurrentSceneId() +
            " moment=" + NarrativeProgressState.GetCurrentMomentId());
    }

    private void Log(string message)
    {
        if (!debugLog)
            return;

        Debug.Log("[ROAE][NarrativeProgressController] " + message);
    }
}
