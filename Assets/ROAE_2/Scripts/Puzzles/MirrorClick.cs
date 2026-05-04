using UnityEngine;
using TMPro;

public class MirrorClickText_Debug : MonoBehaviour
{
    public TextMeshProUGUI lineText;
    [TextArea(2, 5)]
    public string[] lines;

    private int index = 0;
    private Camera mainCamera;

    private Transform myTransform;
    private Vector3 originalScale;


    void Start()
    {
        mainCamera = Camera.main;
        // Debug.Log("✔ Mirror script STARTED");

        if (lineText != null)
            lineText.text = "";
        else
            Debug.LogWarning("⚠️ LineText nu e setat!");

        myTransform = transform;
        originalScale = myTransform.localScale;

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Debug.Log("🖱 Click detectat");

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(worldPos.x, worldPos.y);

            Collider2D hit = Physics2D.OverlapPoint(clickPos);

            if (hit != null)
            {
                // Debug.Log("🎯 Collider lovit: " + hit.gameObject.name);

                if (hit.gameObject == this.gameObject)
                {
                    // Debug.Log("✅ Click pe oglindă confirmat");
                    StartCoroutine(GlitchEffect());


                    if (lines.Length > 0)
                    {
                        string text = lines[index % lines.Length];
                        var thoughtManager = FindObjectOfType<ThoughtManager>();
                        if (thoughtManager != null)
                            thoughtManager.ShowThought(text);

                        index++;

                        MushroomWithTextUI.overrideTextLock = true;
                        CancelInvoke();
                        Invoke(nameof(UnlockTextControl), 2.5f);
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ Nu există texte sau lineText e null!");
                    }
                }
                else
                {
                    // Debug.Log("❌ Ai dat click pe altceva: " + hit.gameObject.name);
                }
            }
            else
            {
                // Debug.Log("❌ Nu s-a lovit niciun collider 2D.");
            }
        }
    }

    void UnlockTextControl()
    {
        MushroomWithTextUI.overrideTextLock = false;
        // Debug.Log("🔓 Controlul textului a fost eliberat");
    }

    private System.Collections.IEnumerator GlitchEffect()
    {
        float duration = 0.15f;
        float timer = 0f;
        float magnitude = 0.05f;

        while (timer < duration)
        {
            float x = Random.Range(-magnitude, magnitude);
            float y = Random.Range(-magnitude, magnitude);
            myTransform.localScale = originalScale + new Vector3(x, y, 0);

            timer += Time.deltaTime;
            yield return null;
        }

        myTransform.localScale = originalScale;
    }

}
