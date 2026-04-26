using UnityEngine;
using TMPro;

public class MirrorClickText : MonoBehaviour
{
    public TextMeshProUGUI lineText;
    [TextArea(2, 5)]
    public string[] lines;

    private int index = 0;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (lineText != null)
            lineText.text = "";
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(worldPos.x, worldPos.y);

            Collider2D hit = Physics2D.OverlapPoint(clickPos);

            if (hit != null && hit.gameObject == this.gameObject)
            {
                if (lineText != null && lines.Length > 0)
                {
                    lineText.text = lines[index % lines.Length];
                    index++;
                }
            }
        }
    }
}
