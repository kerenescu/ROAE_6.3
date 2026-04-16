using UnityEngine;
using UnityEngine.SceneManagement;

public class UsaSchimbaScena : MonoBehaviour
{
    public string numeScenaTinta = "Anticariat";

    private void OnMouseDown()
    {
        SceneManager.LoadScene(numeScenaTinta);
    }
}
