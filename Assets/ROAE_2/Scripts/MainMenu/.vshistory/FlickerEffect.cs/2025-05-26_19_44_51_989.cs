using UnityEngine;
using UnityEngine.UI;

public class FlickerEffect : MonoBehaviour
{
    public float flickerSpeed = 0.05f;   // timp între pâlpâiri
    public float minAlpha = 0.6f;        // transparență minimă
    public float maxAlpha = 1.0f;        // transparență maximă

    private Image img;
    private float timer;

    void Start()
    {
        img = GetComponent<Image>();
    }

    void Update()
    {
        if (img == null) return;

        timer += Time.deltaTime;
        if (timer >= flickerSpeed)
        {
            float alpha = Random.Range(minAlpha, maxAlpha);
            Color c = img.color;
            c.a = alpha;
            img.color = c;
            timer = 0f;
        }
    }
}
