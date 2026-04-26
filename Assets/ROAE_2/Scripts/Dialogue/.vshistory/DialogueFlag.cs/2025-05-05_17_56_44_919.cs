using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueFlag", menuName = "Dialogue System/Dialogue Flag")]
public class DialogueFlag : ScriptableObject
{
    [SerializeField] private string flagKey;

    // ✅ compatibilitate cu codul existent
    public bool WasTriggered => IsTriggered();

    public bool IsTriggered()
    {
        return PlayerPrefs.GetInt(flagKey, 0) == 1;
    }

    public void MarkAsTriggered()
    {
        PlayerPrefs.SetInt(flagKey, 1);
        PlayerPrefs.Save();
    }

    public void ResetFlag()
    {
        PlayerPrefs.SetInt(flagKey, 0);
        PlayerPrefs.Save();
    }
}
