using UnityEngine;
using UnityEngine.UI;

public class FlickerEffect : MonoBehaviour
{
    public float flickerSpeed = 0.1f;  // cât de rapid pâlpâie
    public float minAlpha = 0.6f;      // luminozitate minimă
    public float maxAlpha = 1.0f;      // luminozitate maximă

    private Image image;
    private float timer;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= flickerSpeed)
        {
            float alpha = Random.Range(minAlpha, maxAlpha);
            Color color = image.color;
            color.a = alpha;
            image.color = color;

            timer = 0f;
        }
    }
}
