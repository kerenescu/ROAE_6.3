using UnityEngine;

public class BootstrapLoader : MonoBehaviour
{
    public GameObject uiContainerPrefab;

    private void Awake()
    {
        if (GameObject.FindGameObjectWithTag("UIContainer") == null)
        {
            GameObject ui = Instantiate(uiContainerPrefab);
            ui.name = "UIContainer";
        }

        SceneManager.LoadScene("Flower_Field");
    }
}
