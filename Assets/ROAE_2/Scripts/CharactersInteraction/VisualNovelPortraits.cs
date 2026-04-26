using UnityEngine;
using UnityEngine.UI;

public class VisualNovelPortraits : MonoBehaviour
{
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;

    public void ShowCharacters(Sprite left, Sprite right)
    {
        if (leftPortrait != null)
        {
            leftPortrait.sprite = left;
            SetAlpha(leftPortrait, 1f);
        }

        if (rightPortrait != null)
        {
            rightPortrait.sprite = right;
            SetAlpha(rightPortrait, 1f);
        }
    }

    public void HighlightSpeaker(bool isLeft)
    {
        if (leftPortrait != null)
            SetAlpha(leftPortrait, isLeft ? 1f : 0.4f);
        if (rightPortrait != null)
            SetAlpha(rightPortrait, isLeft ? 0.4f : 1f);
    }

    public void HideAll()
    {
        if (leftPortrait != null)
            SetAlpha(leftPortrait, 0f);
        if (rightPortrait != null)
            SetAlpha(rightPortrait, 0f);
    }

    private void SetAlpha(Image img, float alpha)
    {
        var color = img.color;
        color.a = alpha;
        img.color = color;
    }
}
