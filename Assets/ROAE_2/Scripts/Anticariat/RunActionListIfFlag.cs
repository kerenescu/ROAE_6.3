using UnityEngine;
using AC;

public class FlagToACVariableSync : MonoBehaviour
{
    public DialogueFlag flagToWatch;
    public string acVariableName = "AnticarulPrezent";

    void Update()
    {
        if (flagToWatch == null) return;

        GVar acVar = GlobalVariables.GetVariable(acVariableName);
        if (acVar != null)
        {
            bool newValue = !flagToWatch.IsTriggered(); // inversează flagul
            if (acVar.BooleanValue != newValue)
            {
                acVar.BooleanValue = newValue;
                acVar.Upload();
                Debug.Log($"🔁 Variabila AC '{acVariableName}' setată la: {newValue}");
            }
        }
    }
}
