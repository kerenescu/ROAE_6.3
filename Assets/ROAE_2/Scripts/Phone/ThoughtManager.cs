using UnityEngine;

public class ThoughtManager : MonoBehaviour
{
    public void ShowThought(string text)
    {
        if (CaptionUIManager.Instance != null)
            CaptionUIManager.Instance.ShowCaption(text);
        else
            Debug.LogWarning("CaptionUIManager.Instance e null!");
    }
}
