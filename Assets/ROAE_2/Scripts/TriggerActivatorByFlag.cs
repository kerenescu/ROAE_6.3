using UnityEngine;

public class TriggerActivatorByFlag : MonoBehaviour
{
    public DialogueFlag requiredFlag; // ex: AlreadyReadTarotFlag
    private Collider2D col;

    void Start()
    {
        col = GetComponent<Collider2D>();

        if (requiredFlag != null && col != null)
        {
            col.enabled = requiredFlag.IsTriggered();
        }
    }

    void Update()
    {
        if (requiredFlag != null && col != null && !col.enabled && requiredFlag.IsTriggered())
        {
            col.enabled = true;
            Debug.Log($"✅ Trigger activat deoarece flagul {requiredFlag.name} este setat.");
            Destroy(this); // nu mai trebuie să verifice în fiecare frame
        }
    }
}
