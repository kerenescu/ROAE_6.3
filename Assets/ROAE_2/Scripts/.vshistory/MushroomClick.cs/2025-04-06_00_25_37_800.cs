using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MushroomWithTextUI : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public List<Color> clickColors;
    public List<string> clickLines;
    public TextMeshProUGUI textBox;
    public TypewriterEffect typewriterEffect;

    private int clickIndex = 0;
    private bool isSelected = false;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (textBox == null)
            textBox = FindObjectOfType<TextMeshProUGUI>();

        if (typewriterEffect == null)
            typewriterEffect = FindObjectOfType<TypewriterEffect>();

        if (textBox != null)
            textBox.gameObject.SetActive(false);
    }

    void Update()
    {
        // Dacă jucătorul dă click în altă parte
        if (Input.GetMouseButtonDown(0) && !isSelected)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (!hit || hit.collider.gameObject != gameObject)
            {
                if (textBox != null)
                    textBox.gameObject.SetActive(false);

                isSelected = false;
            }
        }

        isSelected = false; // resetăm starea în fiecare frame
    }

    void OnMouseDown()
    {
        isSelected = true;

        if (spriteRenderer != null && clickColors.Count > 0)
        {
            spriteRenderer.color = clickColors[clickIndex % clickColors.Count];
        }

        if (textBox != null && clickLines.Count > 0)
        {
            textBox.gameObject.SetActive(true);
            typewriterEffect.Run(clickLines[clickIndex % clickLines.Count]);
        }

        clickIndex++;
    }
}
