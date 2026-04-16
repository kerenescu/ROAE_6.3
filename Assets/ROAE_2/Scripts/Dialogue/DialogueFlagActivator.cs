using UnityEngine;

public class DialogueFlagActivator : MonoBehaviour
{
    [SerializeField] private DialogueFlag flag;
    [SerializeField] private GameObject objectToActivate;

    public void ActivateIfFlagSet()
    {
        if (flag != null && flag.WasTriggered && objectToActivate != null)
        {
            objectToActivate.SetActive(true);
            Debug.Log("[TAROT] Deck activat după dialog!");
        }
    }
}
