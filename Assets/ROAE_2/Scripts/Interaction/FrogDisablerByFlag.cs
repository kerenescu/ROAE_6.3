using UnityEngine;

public class FrogDisablerByFlag : MonoBehaviour
{
    public DialogueFlag requiredFlag;

    void Update()
    {
        if (requiredFlag != null && requiredFlag.IsTriggered())
        {
            gameObject.SetActive(false);
            Debug.Log($"[FROG] Broasca a fost distrasă (flag: {requiredFlag.name})");
            Destroy(this); // oprește Update-ul după ce e gata
        }
    }
}
