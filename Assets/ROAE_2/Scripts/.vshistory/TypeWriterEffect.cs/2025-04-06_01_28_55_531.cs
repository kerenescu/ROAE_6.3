using System.Collections;
using UnityEngine;
using TMPro;


public class TypewriterEffect : MonoBehaviour
{

    public float typingSpeed = 0.05f; // viteza de apariție pe literă

    private TextMeshProUGUI textBox;
    private Coroutine typingCoroutine;

    void Awake()
    {
        textBox = GetComponent<TextMeshProUGUI>();
    }

    public void ShowText(string fullText)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(fullText));
    }

    public void Run(string fullText) // 👉 metoda compatibilă cu MushroomClick
    {
        ShowText(fullText); // doar un alias spre ShowText
    }

    private IEnumerator TypeText(string fullText)
    {
        textBox.text = "";
        foreach (char c in fullText)
        {
            textBox.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
