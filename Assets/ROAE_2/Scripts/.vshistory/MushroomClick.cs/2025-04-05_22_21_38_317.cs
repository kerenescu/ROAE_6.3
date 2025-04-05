using UnityEngine;

public class MushroomClick : MonoBehaviour
{
    public Sprite spriteInitial;    // Sprite-ul inițial (opțional)
    public Sprite spriteOnClick;    // Sprite-ul pe care vrei să-l setezi la click

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (spriteInitial != null)
        {
            sr.sprite = spriteInitial;
        }
    }

    void OnMouseDown()
    {
        if (spriteOnClick != null)
        {
            sr.sprite = spriteOnClick;
        }
    }
}
