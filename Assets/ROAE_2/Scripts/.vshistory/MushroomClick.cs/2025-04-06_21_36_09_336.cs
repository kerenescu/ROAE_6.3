using UnityEngine;
using TMPro;

public class MushroomWithTextUI : MonoBehaviour
{
    [Header("Referințe UI")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TypewriterEffect typewriterEffect;
    [SerializeField] private TextMeshProUGUI textBox;

    [Header("Conținut")]
    [SerializeField] private Sprite[] clickSprites;
    [TextArea(2, 4)]
    [SerializeField] private string[] clickTexts;

    [Header("Puzzle Config")]
    public int targetIndex = 2; // indexul stării magice pentru această ciupercă
    public static MushroomWithTextUI[] ciuperciImportante;
    public static bool evenimentDeclansat = false;

    private int clickIndex = 0;
    private Camera mainCamera;

    public static bool overrideTextLock = false;

    void Start()
    {
        mainCamera = Camera.main;

        if (textBox != null)
            textBox.text = "";

        if (spriteRenderer != null && clickSprites.Length > 0)
            spriteRenderer.sprite = clickSprites[0];
    }

    void Update()
    {
        if (overrideTextLock) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(worldPos.x, worldPos.y);

            Collider2D hit = Physics2D.OverlapPoint(clickPos);

            if (hit != null && hit.gameObject == this.gameObject)
            {
                Debug.Log("✅ Click pe: " + gameObject.name);

                if (clickSprites.Length > 0 && spriteRenderer != null)
                    spriteRenderer.sprite = clickSprites[clickIndex % clickSprites.Length];

                if (clickTexts.Length > 0 && typewriterEffect != null)
                    typewriterEffect.Run(clickTexts[clickIndex % clickTexts.Length]);

                clickIndex++;

                VerificaToateCiupercile(); // <== verificăm dacă se declanșează ceva!
            }
        }
    }

    void VerificaToateCiupercile()
    {
        if (evenimentDeclansat || ciuperciImportante == null || ciuperciImportante.Length == 0)
            return;

        foreach (var shroom in ciuperciImportante)
        {
            int current = shroom.clickIndex % shroom.clickSprites.Length;
            if (current != shroom.targetIndex)
                return;
        }

        evenimentDeclansat = true;
        Debug.Log("🌟 TOATE ciupercile sunt în starea lor magică! ✨");

        // TODO: Aici declanșează ce vrei: efect, replică, deschidere, sunet etc.
    }
}
