using UnityEngine;

public class RunActionListIfFlag : MonoBehaviour
{
    public DialogueFlag flagToCheck;
    public AC.ActionList actionListIfTrue;
    public AC.ActionList actionListIfFalse;

    private bool hasRun = false;

    void Update()
    {
        if (!hasRun && flagToCheck != null)
        {
            hasRun = true;

            if (flagToCheck.IsTriggered())
            {
                if (actionListIfTrue != null)
                {
                    actionListIfTrue.Interact();
                    Debug.Log($"✅ Flag {flagToCheck.name} este TRUE → rulez ActionListIfTrue: {actionListIfTrue.name}");
                }
            }
            else
            {
                if (actionListIfFalse != null)
                {
                    actionListIfFalse.Interact();
                    Debug.Log($"❌ Flag {flagToCheck.name} este FALSE → rulez ActionListIfFalse: {actionListIfFalse.name}");
                }
            }

            Destroy(this); // rulează o singură dată
        }
    }
}
