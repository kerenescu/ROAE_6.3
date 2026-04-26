using UnityEngine;
using TMPro;

public class ThoughtBubble : MonoBehaviour
{
    public TextMeshProUGUI thoughtText;
    public float displayTime = 3f;

    public void SetText(string text)
    {
        thoughtText.text = text;
        StartCoroutine(DestroyAfterSeconds());
    }

    private System.Collections.IEnumerator DestroyAfterSeconds()
    {
        yield return new WaitForSecondsRealtime(displayTime);
        Destroy(gameObject);
    }

}
