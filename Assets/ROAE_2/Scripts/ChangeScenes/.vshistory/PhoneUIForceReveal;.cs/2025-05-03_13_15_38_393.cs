using UnityEngine;
using UnityEngine.UI;

public class PhoneUIForceReveal : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return null; // așteaptă un frame
        var phoneUI = FindObjectOfType<PhoneUIFlow>();
        if (phoneUI == null)
        {
            Debug.LogWarning("❌ PhoneUI NOT FOUND after delay");
        }
        else
        {
            Debug.Log("✅ PhoneUI FOUND after delay");
            phoneUI.ForceOpenPhone(); // sau ce metodă folosești
        }
    }

}
