using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MushroomWithTextUI : MonoBehaviour
{
    [Header("Referințe UI")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TypewriterEffect typewriterEffect;
    [SerializeField] private TextMeshProUGUI textBox;

    [Header("Conținut")]
    [SerializeField] private List<Sprite> clickSprites;
    [SerializeField] private List<string> clickTexts;

    private int clickIndex = 0;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (textBox != null)
            textBox.text = "";

        if (spriteRenderer != null && clickSprites.Count > 0)
            spriteRenderer.sprite = clickSprites[0];
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(worldPos.x, worldPos.y);

            Collider2D hitCollider = Physics2D.OverlapPoint(clickPos);

            if (hitCollider != null && hitCollider.gameObject == gameObject)
            {
                // Click pe ciupercă
                clickIndex++;

                if (spriteRenderer != null && clickSprites.Count > 0)
                    spriteRenderer.sprite = clickSprites[clickIndex % clickSprites.Count];

                if (typewriterEffect != null && clickTexts.Count > 0)
                    typewriterEffect.Run(clickTexts[clickIndex % clickTexts.Count]);
            }
            else
            {
                // Click în altă parte → ascunde textul
                if (textBox != null)
                    textBox.text = "";
            }
        }
    }
}
