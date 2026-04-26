using UnityEngine;
using AC;

public class SyncFlagWithAC : MonoBehaviour
{
    public DialogueFlag flagToWatch;
    public string globalACVariableName = "DistractAnticarFlag_AC";

    void Update()
    {
        if (flagToWatch != null && flagToWatch.IsTriggered())
        {
            int varID = GlobalVariables.GetVariableID(globalACVariableName);
            if (varID >= 0)
            {
                GlobalVariables.SetBooleanValue(varID, true);
                Destroy(this); // o singură dată
            }
        }
    }
}
