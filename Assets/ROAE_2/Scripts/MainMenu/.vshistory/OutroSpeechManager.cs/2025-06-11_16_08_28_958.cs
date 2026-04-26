using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class OutroSpeechManager : MonoBehaviour
{
    public TextMeshProUGUI textUI;
    [TextArea(2, 4)]
    public string[] replici;
    public float delayIntreReplici = 3f;
    public string mainMenuScene = "MainMenu";

    void Start()
    {
        StartCoroutine(RepliciCinematic());
    }

    IEnumerator RepliciCinematic()
    {
        foreach (string r in replici)
        {
            textUI.text = r;
            yield return new WaitForSeconds(delayIntreReplici);
        }

        // Curăță textul
        textUI.text = "";

        // Trecere în Main Menu
        SceneManager.LoadScene(mainMenuScene);
    }
}
