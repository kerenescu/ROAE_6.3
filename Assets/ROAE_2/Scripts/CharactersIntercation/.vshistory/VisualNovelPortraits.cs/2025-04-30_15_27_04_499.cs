using UnityEngine;
using UnityEngine.UI;

public class VisualNovelPortraits : MonoBehaviour
{
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;

    public void ShowCharacters(Sprite left, Sprite right)
    {
        leftPortrait.sprite = left;
        rightPortrait.sprite = right;

        leftPortrait.color = new Color(1, 1, 1, 1); // fully visible
        rightPortrait.color = new Color(1, 1, 1, 1);
    }

    public void HighlightSpeaker(bool isLeft)
    {
        leftPortrait.color = isLeft ? Color.white : new Color(1, 1, 1, 0.5f);
        rightPortrait.color = isLeft ? new Color(1, 1, 1, 0.5f) : Color.white;
    }

    public void HideAll()
    {
        leftPortrait.color = new Color(1, 1, 1, 0);
        rightPortrait.color = new Color(1, 1, 1, 0);
    }
}
