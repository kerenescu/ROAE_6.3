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

                Debug.Log($"📐 RectTransform pos: {rt.anchoredPosition}, localPos: {rt.localPosition}, scale: {rt.localScale}");

                rt.anchoredPosition = Vector2.zero;
                rt.localPosition = Vector3.zero;
                rt.localScale = Vector3.one;

                // 🔍 Forțează toate CanvasGroup-urile să fie vizibile și active
                foreach (var cg in phoneGO.GetComponentsInChildren<CanvasGroup>(true))
                {
                    Debug.Log($"🎯 CanvasGroup on {cg.name}: alpha={cg.alpha}, interactable={cg.interactable}, blocksRaycasts={cg.blocksRaycasts}");
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }

                // ✅ Colorează toate imaginile și setează alpha la 1
                foreach (var img in phoneGO.GetComponentsInChildren<Image>(true))
                {
                    var color = img.color;
                    color = new Color(1f, 0f, 1f, 1f); // magenta complet opac
                    img.color = color;
                }

                // ✅ Activează toate butoanele
                foreach (var btn in phoneGO.GetComponentsInChildren<Button>(true))
                {
                    btn.interactable = true;
                    btn.gameObject.SetActive(true);
                }

                Debug.Log("📱 PhoneUI fully forced visible.");
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
