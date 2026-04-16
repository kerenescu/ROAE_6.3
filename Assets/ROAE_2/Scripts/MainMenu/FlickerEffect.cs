using UnityEngine;
using UnityEngine.UI;

public class FlickerEffect : MonoBehaviour
{
    public float minInterval = 0.02f;
    public float maxInterval = 0.15f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 1.0f;
    public float shakeIntensity = 2f;

    private Image img;
    private float timer;
    private float nextFlickTime;
    private Vector3 originalPos;

    void Start()
    {
        img = GetComponent<Image>();
        originalPos = transform.localPosition;
        ScheduleNextFlick();
    }

    void Update()
    {
        if (img == null) return;

        timer += Time.deltaTime;
        if (timer >= nextFlickTime)
        {
            float alpha = Random.Range(minAlpha, maxAlpha);
            Color c = img.color;
            c.a = alpha;
            img.color = c;

            transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * shakeIntensity;

            ScheduleNextFlick();
        }
    }

    void ScheduleNextFlick()
    {
        timer = 0f;
        nextFlickTime = Random.Range(minInterval, maxInterval);
    }
}
