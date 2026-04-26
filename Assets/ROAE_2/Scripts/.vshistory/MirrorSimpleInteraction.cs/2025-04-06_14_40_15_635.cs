using UnityEngine;
using TMPro;

public class MirrorSimpleInteraction : MonoBehaviour
{
    public GameObject mirrorShattered;
    public GameObject mirrorComplete;

    public TextMeshProUGUI lineText;
    public string[] lines;
    private int index = 0;
    private bool hasCompleted = false;

    private void OnMouseDown()
    {
        if (hasCompleted) return;

        if (index < lines.Length)
        {
            Debug.Log($"🪞 Mirror line {index}: {lines[index]}");
            if (lineText != null)
            {
                lineText.text = lines[index];
            }
            index++;

            if (index == lines.Length)
            {
                Invoke("CompleteMirror", 2.5f); // așteaptă 2.5 secunde înainte de a deveni întreagă
            }
        }
    }

    void CompleteMirror()
    {
        hasCompleted = true;

        if (mirrorShattered != null) mirrorShattered.SetActive(false);
        if (mirrorComplete != null) mirrorComplete.SetActive(true);
        if (lineText != null) lineText.text = ""; // golește textul după final
    }
}
