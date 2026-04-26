using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhoneUIForceReveal : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null; // așteaptă ca UI-ul să fie gata

        var phone = FindObjectOfType<PhoneUIFlow>();
        if (phone != null)
        {
            GameObject phoneGO = phone.GetInterfataVizuala();


            if (phoneGO != null)
            {
                var rt = phoneGO.GetComponent<RectTransform>();
                rt.position = Vector3.zero;
                rt.localScale = Vector3.one;

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
