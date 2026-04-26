using UnityEngine;
using UnityEngine.UI;

public class PhoneUIForceReveal : MonoBehaviour
{
    void Start()
    {
        GameObject phone = GameObject.Find("PhoneUI");
        if (phone != null)
        {
            var rt = phone.GetComponent<RectTransform>();
            rt.position = Vector3.zero;
            rt.localScale = Vector3.one;

            foreach (var img in phone.GetComponentsInChildren<Image>(true))
            {
                img.color = Color.magenta; // vizibil instant
            }

            foreach (var btn in phone.GetComponentsInChildren<Button>(true))
            {
                btn.interactable = true;
                btn.gameObject.SetActive(true);
            }

            Debug.Log("📱 PhoneUI should now be VISIBLE & INTERACTABLE.");
        }
        else
        {
            Debug.LogWarning("❌ PhoneUI NOT FOUND");
        }
    }
}
