using UnityEngine;
using TMPro;

public class MushroomWithTextUI : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] clickSprites;
    public string[] textReplies;
    public TypewriterEffect typewriterEffect;
    public TextMeshProUGUI textBox;

    private int clickCount = 0;
    private static bool isTextActive = false;

    void Start()
    {
        if (textBox != null)
            textBox.gameObject.SetActive(false);
    }

    void OnMouseDown()
    {
        clickCount = (clickCount + 1) % Mathf.Min(clickSprites.Length, textReplies.Length);

        if (spriteRenderer != null)
            spriteRenderer.sprite = clickSprites[clickCount];

        if (textBox != null)
        {
            textBox.gameObject.SetActive(true);
            typewriterEffect.Run(textReplies[clickCount]);
        }

        isTextActive = true;
    }

    void Update()
    {
        if (isTextActive && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider == null || hit.collider.gameObject != gameObject)
            {
                if (textBox != null)
                    textBox.gameObject.SetActive(false);

                isTextActive = false;
            }
        }

        if (Input.GetMouseButtonDown(0)) // doar când apeși click stânga
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            // dacă NU ai dat click pe acest obiect (floarea)
            if (hit.collider == null || hit.collider.gameObject != gameObject)
            {
                Debug.Log("Ai dat click în altă parte!");
                textBox.gameObject.SetActive(false);
            }
        }

    }
}
