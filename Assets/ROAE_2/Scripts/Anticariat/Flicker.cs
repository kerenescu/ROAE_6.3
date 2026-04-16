using UnityEngine;

public class FlickerDarknessMask : MonoBehaviour
{
    public SpriteRenderer darknessMask; // SpriteRenderer-ul măștii
    public float minAlpha = 0f;         // luminozitate maximă (masca e invizibilă)
    public float maxAlpha = 0.7f;       // întunecare maximă
    public float flickerSpeed = 0.05f;  // cât de rapid se schimbă
    public float flickerInterval = 0.2f; // timp aleator între pâlpâiri

    private float nextFlickerTime;

    void Start()
    {
        if (darknessMask == null)
        {
            Debug.LogError("Nu ai setat darknessMask!");
            this.enabled = false;
        }

        nextFlickerTime = Time.time + Random.Range(0.1f, flickerInterval);
    }

    void Update()
    {
        if (Time.time >= nextFlickerTime)
        {
            float newAlpha = Random.Range(minAlpha, maxAlpha);
            Color currentColor = darknessMask.color;
            currentColor.a = newAlpha;
            darknessMask.color = currentColor;

            nextFlickerTime = Time.time + Random.Range(flickerSpeed, flickerInterval);
        }
    }
}
