using UnityEngine;
using TMPro;

public class MirrorSimpleInteraction : MonoBehaviour
{
    [Header("Referințe")]
    public GameObject mirrorShattered;
    public GameObject mirrorComplete;
    public TextMeshProUGUI lineText;

    [Header("Replici")]
    [TextArea(2, 5)]
    public string[] lines;

    private int index = 0;
    private bool isRestored = false;

    void Update()
    {
        // Apăsarea tastei C = Oglinda se repară
        if (!isRestored && Input.GetKeyDown(KeyCode.C))
        {
            RestoreMirror();
        }
    }

    void OnMouseDown()
    {
        if (isRestored) return;

        if (lines.Length == 0 || lineText == null)
            return;

        // Afișăm replică pe GUI în buclă
        lineText.text = lines[index];
        index = (index + 1) % lines.Length;
    }

    void RestoreMirror()
    {
        isRestored = true;
        lineText.text = ""; // opțional, golește textul

        if (mirrorShattered != null) mirrorShattered.SetActive(false);
        if (mirrorComplete != null) mirrorComplete.SetActive(true);
    }
}
