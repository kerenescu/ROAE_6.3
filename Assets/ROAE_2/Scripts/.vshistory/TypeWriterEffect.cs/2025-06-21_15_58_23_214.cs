using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    public float typingSpeed = 0.05f; // viteza de apariție pe literă
    public AudioSource typingSound;   // sunetul de mașină de scris

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
        ShowText(fullText); // doar un alias spre ShowText
    }

    private IEnumerator TypeText(string fullText)
    {
        textBox.text = "";
        foreach (char c in fullText)
        {
            textBox.text += c;

            // Redă sunet dacă nu e spațiu
            if (typingSound != null && c != ' ')
            {
                //typingSound.Stop(); // opțional, evită suprapuneri
                typingSound.PlayOneShot(typingSound.clip);
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

            // Redă sunet dacă nu e spațiu
            if (typingSound != null && c != ' ')
            {
                typingSound.Stop(); // opțional
                typingSound.PlayOneShot(typingSound.clip);
            }

            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
