using UnityEngine;
using UnityEngine.Events;

public class FlagReader : MonoBehaviour
{
    public DialogueFlag flag;
    public UnityEvent onTrue;
    public UnityEvent onFalse;

    public void CheckFlag()
    {
        if (flag != null && flag.IsTriggered())
        {
            onTrue.Invoke();
        }
        else
        {
            onFalse.Invoke();
        }
    }
}
