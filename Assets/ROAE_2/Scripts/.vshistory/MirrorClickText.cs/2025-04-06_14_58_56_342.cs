using UnityEngine;
using TMPro;

public class MirrorClickText_Debug : MonoBehaviour
{
    public TextMeshProUGUI lineText;
    [TextArea(2, 5)]
    public string[] lines;

    private int index = 0;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        Debug.Log("✔ Mirror script STARTED");

        if (lineText != null)
            lineText.text = "";
        else
            Debug.LogWarning("⚠️ LineText nu e setat!");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("🖱 Click detectat");

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(worldPos.x, worldPos.y);

            Collider2D hit = Physics2D.OverlapPoint(clickPos);

            if (hit != null)
            {
                Debug.Log("🎯 Collider lovit: " + hit.gameObject.name);

                if (hit.gameObject == this.gameObject)
                {
                    Debug.Log("✅ Click pe oglindă confirmat");

                    if (lineText != null && lines.Length > 0)
                    {
                        lineText.text = lines[index % lines.Length];
                        Debug.Log("📝 Linie afișată: " + lines[index % lines.Length]);
                        index++;
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ Nu există texte sau linieText e null!");
                    }
                }
                else
                {
                    Debug.Log("❌ Ai dat click pe altceva: " + hit.gameObject.name);
                }
            }
            else
            {
                Debug.Log("❌ Nu s-a lovit niciun collider 2D.");
            }
        }
    }
}
