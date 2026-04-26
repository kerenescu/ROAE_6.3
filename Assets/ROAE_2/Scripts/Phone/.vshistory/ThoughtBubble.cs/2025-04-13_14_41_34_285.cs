using UnityEngine;
using TMPro;

public class ThoughtBubble : MonoBehaviour
{
    public TextMeshProUGUI thoughtText;
    public float displayTime = 3f;

    public void SetText(string text)
    {
        thoughtText.text = text;
        Invoke("DestroySelf", displayTime);
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }
}
