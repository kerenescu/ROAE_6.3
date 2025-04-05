using UnityEngine;
using TMPro; 

public class MushroomClickWithText : MonoBehaviour
{
    public Sprite[] mushroomStates;             // Sprite-urile de rotit
    [TextArea] public string[] clickLines;      // Replici pentru fiecare click
    public TextMeshProUGUI uiText;              // Textul care afișează replicile

    private SpriteRenderer sr;
    private int currentIndex = 0;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (mushroomStates.Length > 0)
            sr.sprite = mushroomStates[0];

        if (uiText != null && clickLines.Length > 0)
            uiText.text = clickLines[0];
    }

    void OnMouseDown()
    {
        if (mushroomStates.Length == 0) return;

        currentIndex = (currentIndex + 1) % mushroomStates.Length;
        sr.sprite = mushroomStates[currentIndex];

        if (uiText != null && currentIndex < clickLines.Length)
            uiText.text = clickLines[currentIndex];
    }
}
