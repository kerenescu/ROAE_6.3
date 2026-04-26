using UnityEngine;

public class MushroomCycle : MonoBehaviour
{
    public Sprite[] mushroomStates; // cele 4 sprite-uri
    private SpriteRenderer sr;
    private int currentIndex = 0;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (mushroomStates.Length > 0)
        {
            sr.sprite = mushroomStates[0]; // începem cu prima stare
        }
    }

    void OnMouseDown()
    {
        if (mushroomStates.Length == 0) return;

        currentIndex = (currentIndex + 1) % mushroomStates.Length;
        sr.sprite = mushroomStates[currentIndex];
    }
}
