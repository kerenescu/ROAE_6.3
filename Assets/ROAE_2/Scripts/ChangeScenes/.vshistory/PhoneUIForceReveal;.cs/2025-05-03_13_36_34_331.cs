using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhoneUIForceReveal : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null; // așteaptă un frame

        var phone = FindObjectOfType<PhoneUIFlow>();
        if (phone != null)
        {
            GameObject phoneGO = phone.GetInterfataVizuala();

            if (phoneGO != null)
            {
                var rt = phoneGO.GetComponent<RectTransform>();

                // 🔎 Loguri pentru debug
                Debug.Log($"📐 RectTransform pos: {rt.anchoredPosition}, localPos: {rt.localPosition}, scale: {rt.localScale}");

                // ✅ Forțează poziționare corectă și scală vizibilă
                rt.anchoredPosition = Vector2.zero;
                rt.localPosition = Vector3.zero;
                rt.localScale = Vector3.one;

                // 🔎 Dacă există CanvasGroup
                var cg = phoneGO.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    Debug.Log($"🔍 CanvasGroup alpha: {cg.alpha}, interactable: {cg.interactable}, blocksRaycasts: {cg.blocksRaycasts}");
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }

                // ✅ Colorează tot UI-ul și activează butoanele
                foreach (var img in phoneGO.GetComponentsInChildren<Image>(true))
                {
                    img.color = Color.magenta;
                }

                foreach (var btn in phoneGO.GetComponentsInChildren<Button>(true))
                {
                    btn.interactable = true;
                    btn.gameObject.SetActive(true);
                }

                Debug.Log("📱 PhoneUI forced visible via component.");
            }
            else
            {
                Debug.LogWarning("⚠️ phone.interfataVizuala e NULL");
            }
        }
        else
        {
            Debug.LogWarning("❌ PhoneUIFlow component NOT FOUND");
        }
    }
}
