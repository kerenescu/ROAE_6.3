using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MushroomWithTextUI : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;             // Sprite-ul ciupercii
    public Sprite[] clickSprites;                     // 4 sprite-uri pentru click
    public string[] textReplies;                      // 4 replici
    public TypewriterEffect typewriterEffect;         // Scriptul efectului de scris
    public TextMeshProUGUI textBox;                   // Textul care apare
    //public Image backgroundImage;                     // Fundalul alb din UI

    private int clickCount = 0;

    void Start()
    {
        // Asigură-te că totul e dezactivat la început
        if (textBox != null)
            textBox.gameObject.SetActive(false);

        //if (backgroundImage != null)
        //    backgroundImage.gameObject.SetActive(false);
    }

    void OnMouseDown()
    {
        if (clickSprites.Length == 0 || textReplies.Length == 0)
            return;

        // Ciclu circular prin sprite-uri și replici
        clickCount = (clickCount + 1) % Mathf.Min(clickSprites.Length, textReplies.Length);

        // Schimbă sprite
        if (spriteRenderer != null)
            spriteRenderer.sprite = clickSprites[clickCount];

        //// Activează fundalul și textul
        //if (backgroundImage != null)
        //    backgroundImage.gameObject.SetActive(true);

        if (textBox != null)
        {
            textBox.gameObject.SetActive(true);
            textBox.text = ""; // curăță textul pentru efect

            // Rulează efectul mașină de scris
            if (typewriterEffect != null)
                typewriterEffect.Run(textReplies[clickCount]);

            //// Redimensionează fundalul după text
            //Vector2 newSize = new Vector2(textBox.preferredWidth + 40f, textBox.preferredHeight + 30f);
            //backgroundImage.rectTransform.sizeDelta = newSize;
        }
    }
}
