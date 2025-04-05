using UnityEngine;
using UnityEngine.UI; // dacă folosești Text clasic
// using TMPro; // dacă folosești TextMeshPro

public class MushroomWithTextUI : MonoBehaviour
{
    public Sprite[] mushroomStates;
    [TextArea] public string[] clickLines;

    public Text uiText; // ← dacă folosești Text clasic
    // public TextMeshProUGUI uiText; // ← dacă folosești TextMeshPro

    private SpriteRenderer sr;
    private int currentIndex = 0;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (mushroomStates.Length > 0)
            sr.sprite = mushroomStates[0];

        if (uiText != null)
            uiText.text = clickLines[0];
    }

    void OnMouseDown()
    {
        if (mushroomStates.Length == 0) return;

        currentIndex = (currentIndex + 1) % mushroomStates.Length;
        sr.sprite = mushroomStates[currentIndex];

        if (currentIndex < clickLines.Length && uiText != null)
        {
            uiText.text = clickLines[currentIndex];
        }
    }
}
