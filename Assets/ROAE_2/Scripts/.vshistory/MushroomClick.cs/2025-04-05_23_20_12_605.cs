using UnityEngine;

public class MushroomWithLines : MonoBehaviour
{
    public Sprite[] mushroomStates;         // Sprite-urile pentru fiecare stare
    [TextArea] public string[] clickLines;  // Replica pentru fiecare stare

    private SpriteRenderer sr;
    private int currentIndex = 0;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (mushroomStates.Length > 0)
        {
            sr.sprite = mushroomStates[0];
        }
    }

    void OnMouseDown()
    {
        if (mushroomStates.Length == 0) return;

        // Trecem la următorul index, ciclic
        currentIndex = (currentIndex + 1) % mushroomStates.Length;

        // Schimbăm sprite-ul
        sr.sprite = mushroomStates[currentIndex];

        // Afișăm replica în consolă (sau oriunde vrei)
        if (currentIndex < clickLines.Length)
        {
            Debug.Log(clickLines[currentIndex]);
        }
    }
}
