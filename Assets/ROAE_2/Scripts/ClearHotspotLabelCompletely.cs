using UnityEngine;
using UnityEngine.UI;

public class ClearHotspotLabelCompletely : MonoBehaviour
{
    public GameObject hotspotMenu; // intregul menu (text + icon)

    void Start()
    {
        if (hotspotMenu != null)
            hotspotMenu.SetActive(false);
    }
}
