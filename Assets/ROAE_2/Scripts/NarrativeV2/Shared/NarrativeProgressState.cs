using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NarrativeProgressState
{
    private const string CurrentChapterKey = "narrative_current_chapter";
    private const string CurrentMomentKey = "narrative_current_moment";
    private const string SceneOverrideKey = "narrative_scene_override";

    public static string GetCurrentChapterId()
    {
        return PlayerPrefs.GetString(CurrentChapterKey, string.Empty);
    }

    public static void SetCurrentChapterId(string chapterId)
    {
        PlayerPrefs.SetString(CurrentChapterKey, chapterId ?? string.Empty);
        PlayerPrefs.Save();
    }

    public static string GetCurrentMomentId()
    {
        return PlayerPrefs.GetString(CurrentMomentKey, string.Empty);
    }

    public static void SetCurrentMomentId(string momentId)
    {
        PlayerPrefs.SetString(CurrentMomentKey, momentId ?? string.Empty);
        PlayerPrefs.Save();
    }

    public static string GetCurrentSceneId()
    {
        string sceneOverride = PlayerPrefs.GetString(SceneOverrideKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(sceneOverride))
            return sceneOverride;

        Scene activeScene = SceneManager.GetActiveScene();
        return activeScene.IsValid() ? activeScene.name : string.Empty;
    }

    public static void SetSceneOverride(string sceneId)
    {
        PlayerPrefs.SetString(SceneOverrideKey, sceneId ?? string.Empty);
        PlayerPrefs.Save();
    }

    public static void ClearSceneOverride()
    {
        PlayerPrefs.DeleteKey(SceneOverrideKey);
        PlayerPrefs.Save();
    }

    public static bool MatchesCurrentChapter(string requiredChapterId)
    {
        return Matches(requiredChapterId, GetCurrentChapterId());
    }

    public static bool MatchesCurrentScene(string requiredSceneId)
    {
        return Matches(requiredSceneId, GetCurrentSceneId());
    }

    public static bool MatchesCurrentMoment(string requiredMomentId)
    {
        return Matches(requiredMomentId, GetCurrentMomentId());
    }

    private static bool Matches(string expected, string actual)
    {
        if (string.IsNullOrWhiteSpace(expected))
            return true;

        return string.Equals(expected.Trim(), actual ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }
}
