using UnityEngine;
using TMPro;

public class MirrorSimpleInteraction : MonoBehaviour
{
    public GameObject mirrorShattered;
    public GameObject mirrorComplete;

    public TextMeshProUGUI lineText;
    public string[] lines;
    private int index = 0;

    private void OnMouseDown()
    {
        if (index < lines.Length)
        {
            lineText.text = lines[index];
            index++;

            if (index >= lines.Length)
            {
                mirrorShattered.SetActive(false);
                mirrorComplete.SetActive(true);
            }
        }
    }
}
