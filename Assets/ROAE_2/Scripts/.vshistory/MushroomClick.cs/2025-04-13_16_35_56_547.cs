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
    public int targetIndex = 2; // indexul sprite-ului considerat "magic"
    [HideInInspector] public bool isInMagicState = false;

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
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(worldPos.x, worldPos.y);

            Collider2D hit = Physics2D.OverlapPoint(clickPos);

            if (hit != null)
            {
                if (hit.gameObject == this.gameObject && !overrideTextLock)
                {
                    Debug.Log("✅ Click pe: " + gameObject.name);

                    int currentSpriteIndex = clickIndex % clickSprites.Length;

                    if (spriteRenderer != null && clickSprites.Length > 0)
                        spriteRenderer.sprite = clickSprites[currentSpriteIndex];

                    if (clickTexts.Length > 0)
                    {
                        string thought = clickTexts[currentSpriteIndex];
                        var thoughtManager = FindObjectOfType<ThoughtManager>();
                        if (thoughtManager != null)
                            thoughtManager.ShowThought(thought);
                    }

                    clickIndex++;

                    isInMagicState = (currentSpriteIndex == targetIndex);

                    VerificaToateCiupercile();
                }
                else if (!overrideTextLock)
                {
                    // Ai dat click pe altceva → ascundem textul
                    if (textBox != null)
                        textBox.text = "";
                }
            }
            else if (!overrideTextLock)
            {
                // Click în gol → ascundem textul
                if (textBox != null)
                    textBox.text = "";
            }
        }
    }

    void VerificaToateCiupercile()
    {
        if (evenimentDeclansat || ciuperciImportante == null || ciuperciImportante.Length == 0)
            return;

        foreach (var shroom in ciuperciImportante)
        {
            Debug.Log($"🔍 {shroom.name} → Magic? {shroom.isInMagicState}");

            if (!shroom.isInMagicState)
                return;
        }

        evenimentDeclansat = true;
        Debug.Log("🌟 TOATE ciupercile sunt în starea lor magică! ✨");

        // Efect final aici:
        GlowEffectController glow = FindObjectOfType<GlowEffectController>();
        if (glow != null)
            glow.TriggerVignette();
    }
}
