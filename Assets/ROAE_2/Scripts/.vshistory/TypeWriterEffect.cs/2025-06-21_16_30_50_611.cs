using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    public float typingSpeed = 0.05f;               // viteza de apariție pe literă
    public AudioSource typingSound;                 // sursa sunetului de tastare
    [Range(0f, 1f)] public float soundVolume = 0.3f; // volum redus
    public int soundEveryNChars = 2;                // sunet o dată la N caractere

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

    public void Run(string fullText)
    {
        ShowText(fullText);
    }

    private IEnumerator TypeText(string fullText)
    {
        textBox.text = "";
        for (int i = 0; i < fullText.Length; i++)
        {
            char c = fullText[i];
            textBox.text += c;

            if (typingSound != null && typingSound.clip != null && c != ' ' && i % soundEveryNChars == 0)
            {
                Vector3 pos = Camera.main != null ? Camera.main.transform.position : transform.position;
                AudioSource.PlayClipAtPoint(typingSound.clip, pos, soundVolume);
            }

            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public IEnumerator TypeTextWithWait(string fullText)
    {
        textBox.text = "";
        for (int i = 0; i < fullText.Length; i++)
        {
            char c = fullText[i];
            textBox.text += c;

            if (typingSound != null && typingSound.clip != null && c != ' ' && i % soundEveryNChars == 0)
            {
                Vector3 pos = Camera.main != null ? Camera.main.transform.position : transform.position;
                AudioSource.PlayClipAtPoint(typingSound.clip, pos, soundVolume);
            }

            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
