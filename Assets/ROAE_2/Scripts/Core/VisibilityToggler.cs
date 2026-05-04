using UnityEngine;

public class VisibilityToggler : MonoBehaviour
{
    public GameObject target;

    public void Show()
    {
        if (target != null)
            target.SetActive(true);
    }

    public void Hide()
    {
        if (target != null)
            target.SetActive(false);
    }
}
