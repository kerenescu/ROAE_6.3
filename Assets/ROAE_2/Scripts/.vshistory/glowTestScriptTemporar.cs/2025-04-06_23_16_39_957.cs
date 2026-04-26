using UnityEngine;

public class GlowManualTest : MonoBehaviour
{
    public GlowEffectController glow;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            glow.TriggerVignette();
        }
    }
}
