using UnityEngine;
using TMPro;

public class MirrorSimpleInteraction : MonoBehaviour
{
    public TextMeshProUGUI lineText; // Referință la MushroomLineText
    public string[] lines;
    private int index = 0;
    private float clearDelay = 3f;

    void OnMouseDown()
    {
        if (index < lines.Length)
        {
            lineText.text = lines[index];
            CancelInvoke("ClearText");
            Invoke("ClearText", clearDelay);
            index++;
        }
    }

    void ClearText()
    {
        lineText.text = "";
    }
}
