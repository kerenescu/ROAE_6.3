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
            GVar variable = GlobalVariables.GetVariable(globalACVariableName);
            if (variable != null && !variable.BooleanValue)
            {
                variable.BooleanValue = true;
                variable.Upload(); // trimite schimbarea către AC runtime
                Debug.Log($"✅ Variabila AC {globalACVariableName} setată pe true din flag!");

                Destroy(this); // rulează o singură dată
            }
        }
    }
}
