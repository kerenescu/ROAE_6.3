using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MirrorWithTextUI : MonoBehaviour
{
    [Header("Referințe UI")]
    [SerializeField] private TextMeshProUGUI textBox;

    [Header("Replici")]
    [TextArea(2, 5)]
    [SerializeField] private List<string> lines;

    private int index = 0;
    private Camera mainCamera;
    private bool isRestored = false;

    void Start()
    {
        mainCamera = Camera.main;
        if (textBox != null)
            textBox.text = "";
    }

    void Update()
    {
        // Restore oglinda cu tasta C
        if (!isRestored && Input.GetKeyDown(KeyCode.C))
        {
            isRestored = true;
            if (textBox != null)
                textBox.text = "";
            gameObject.SetActive(false); // ascunde oglinda spartă
            // TODO: activează oglinda întreagă aici, dacă vrei
        }

        // Click pe oglindă
        if (!isRestored && Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(worldPos.x, worldPos.y);

            Collider2D hitCollider = Physics2D.OverlapPoint(clickPos);

            if (hitCollider != null && hitCollider.gameObject == gameObject)
            {
                if (textBox != null && lines.Count > 0)
                    textBox.text = lines[index++ % lines.Count];
            }
        }
    }
}
