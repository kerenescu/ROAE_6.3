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
    [SerializeField] private string[] clickTexts;

    [Header("Puzzle Config")]
    public int targetIndex = 2; // indexul corect pentru această ciupercă
    public static MushroomWithTextUI[] ciuperciImportante;
    public static bool evenimentDeclansat = false;

    private int clickIndex = 0;
    private Camera mainCamera;

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
        if (MushroomWithTextUI.overrideTextLock) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(worldPos.x, worldPos.y);

            Collider2D hitCollider = Physics2D.OverlapPoint(clickPos);

            if (hitCollider != null && hitCollider.gameObject == gameObject)
            {
                clickIndex++;

                if (spriteRenderer != null && clickSprites.Length > 0)
                    spriteRenderer.sprite = clickSprites[clickIndex % clickSprites.Length];

                if (clickTexts.Length > 0 && typewriterEffect != null)
                    typewriterEffect.Run(clickTexts[clickIndex % clickTexts.Length]);

                VerificaToateCiupercile();
            }
        }
    }

    void VerificaToateCiupercile()
    {
        if (evenimentDeclansat || ciuperciImportante == null || ciuperciImportante.Length == 0)
            return;

        foreach (var shroom in ciuperciImportante)
        {
            int currentIndex = shroom.clickIndex % shroom.clickSprites.Length;
            if (currentIndex != shroom.targetIndex)
                return; // cel puțin una NU e în starea ei
        }

        // toate sunt în starea corectă
        evenimentDeclansat = true;
        Debug.Log("🌈 TOATE ciupercile sunt în starea lor magică!");
        // AICI PUI CE DECLANȘEZI
    }
}
