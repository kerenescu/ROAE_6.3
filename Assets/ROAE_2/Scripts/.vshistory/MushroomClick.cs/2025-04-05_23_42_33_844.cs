using UnityEngine;
using TMPro;

public class MushroomClickWithText : MonoBehaviour
{
    [Header("Sprites și replici")]
    public Sprite[] mushroomStates;             // Sprite-urile pe care le rotești
    [TextArea] public string[] clickLines;      // Replici pentru fiecare click

    [Header("Referințe UI")]
    public SpriteRenderer spriteRenderer;       // SpriteRenderer-ul ciupercii
    public TypewriterEffect typewriterEffect;   // Scriptul care scrie replicile cu efect
    public TextMeshProUGUI textBox;             // Textul pe care apare replica

    private int currentIndex = 0;

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (mushroomStates.Length > 0)
            spriteRenderer.sprite = mushroomStates[0];

        if (clickLines.Length > 0 && textBox != null)
            typewriterEffect.ShowText(clickLines[0]);
    }

    void OnMouseDown()
    {
        if (mushroomStates.Length == 0) return;

        currentIndex = (currentIndex + 1) % mushroomStates.Length;
        spriteRenderer.sprite = mushroomStates[currentIndex];

        if (currentIndex < clickLines.Length && typewriterEffect != null)
            typewriterEffect.ShowText(clickLines[currentIndex]);
    }
}
