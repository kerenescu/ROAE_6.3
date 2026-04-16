using UnityEngine;

public class RunActionListIfFlag : MonoBehaviour
{
    public DialogueFlag flagToCheck;
    public AC.ActionList actionListToRun;
    private bool hasRun = false;

    void Update()
    {
        if (!hasRun && flagToCheck != null && flagToCheck.IsTriggered())
        {
            if (actionListToRun != null)
            {
                actionListToRun.Interact();
                Debug.Log($"🎬 Rulează ActionList: {actionListToRun.name} deoarece flagul {flagToCheck.name} este activat.");
            }

            hasRun = true;
            Destroy(this); // rulează o singură dată
        }
    }
}
