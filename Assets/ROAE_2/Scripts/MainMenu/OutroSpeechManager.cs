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

    public TypewriterEffect typewriter; // referință către scriptul TypewriterEffect

    void Start()
    {
        if (textUI != null && typewriter != null)
            StartCoroutine(RepliciCinematic());
    }

    IEnumerator RepliciCinematic()
    {
        foreach (string r in replici)
        {
            yield return StartCoroutine(typewriter.TypeTextWithWait(r));
            yield return new WaitForSeconds(delayIntreReplici);
        }

        textUI.text = "";
        SceneManager.LoadScene(mainMenuScene);
    }
}
