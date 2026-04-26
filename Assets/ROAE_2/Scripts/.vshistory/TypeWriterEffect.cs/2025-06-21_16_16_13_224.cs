using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    public float typingSpeed = 0.05f;           // viteza de apariție pe literă
    public AudioSource typingSound;             // sursa sunetului de tastare

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

    public void Run(string fullText) // 👉 metodă compatibilă cu MushroomClick
    {
        ShowText(fullText);
    }

    private IEnumerator TypeText(string fullText)
    {
        textBox.text = "";
        foreach (char c in fullText)
        {
            textBox.text += c;

            // Redă sunetul de tastare dacă există și nu e spațiu
            if (typingSound != null && typingSound.clip != null && c != ' ')
            {
                Vector3 soundPosition = Camera.main != null ? Camera.main.transform.position : transform.position;
                AudioSource.PlayClipAtPoint(typingSound.clip, soundPosition);
            }

            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public IEnumerator TypeTextWithWait(string fullText)
    {
        textBox.text = "";
        foreach (char c in fullText)
        {
            textBox.text += c;

            // Redă sunetul de tastare dacă există și nu e spațiu
            if (typingSound != null && typingSound.clip != null && c != ' ')
            {
                Vector3 soundPosition = Camera.main != null ? Camera.main.transform.position : transform.position;
                AudioSource.PlayClipAtPoint(typingSound.clip, soundPosition);
            }

            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
