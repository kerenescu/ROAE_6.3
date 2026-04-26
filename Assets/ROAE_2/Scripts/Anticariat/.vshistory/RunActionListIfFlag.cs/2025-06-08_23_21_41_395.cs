using UnityEngine;

public class FlagReader : MonoBehaviour
{
    public DialogueFlag flag;

    public bool IsFlagTriggered()
    {
        return flag != null && flag.IsTriggered();
    }
}
