using UnityEngine;

public class PhoneUI_Debug : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log($"🟢 {gameObject.name} was ENABLED at frame {Time.frameCount} by: {GetCaller()}");
    }

    private void OnDisable()
    {
        Debug.LogWarning($"🔴 {gameObject.name} was DISABLED at frame {Time.frameCount} by: {GetCaller()}");
    }

    string GetCaller()
    {
        var st = new System.Diagnostics.StackTrace();
        for (int i = 2; i < st.FrameCount; i++)
        {
            var frame = st.GetFrame(i);
            var method = frame.GetMethod();
            if (method.DeclaringType != typeof(PhoneUI_Debug))
                return $"{method.DeclaringType}.{method.Name}";
        }
        return "Unknown";
    }
}
