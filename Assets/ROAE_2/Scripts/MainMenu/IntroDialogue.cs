using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroDialogue : MonoBehaviour
{
    [TextArea(2, 4)]
    public string[] replici;

    public float timpDupaReplica = 1f; // timp de pauză după ce scrie tot textul
    public TextMeshProUGUI textUI;
    public string scenaUrmatoare;
    public TypewriterEffect typewriter; // referință către scriptul de efect

    void Start()
    {
        if (textUI != null && typewriter != null)
            StartCoroutine(AfiseazaReplici());
    }

    IEnumerator AfiseazaReplici()
    {
        foreach (string replica in replici)
        {
            // Afișează replica cu efect de mașină de scris
            yield return StartCoroutine(typewriter.TypeTextWithWait(replica));
            yield return new WaitForSeconds(timpDupaReplica);
        }

        textUI.text = "";
        SceneManager.LoadScene(scenaUrmatoare);
    }
}
