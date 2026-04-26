using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroDialogue : MonoBehaviour
{
    [TextArea(2, 4)]
    public string[] replici;

    public float timpPeReplica = 3f;
    public TextMeshProUGUI textUI;
    public string scenaUrmatoare;

    void Start()
    {
        if (textUI != null)
            StartCoroutine(AfiseazaReplici());
    }

    IEnumerator AfiseazaReplici()
    {
        foreach (string replica in replici)
        {
            textUI.text = replica;
            yield return new WaitForSeconds(timpPeReplica);
        }

        // Curățăm textul
        textUI.text = "";

        // Trecem la scena următoare
        SceneManager.LoadScene(scenaUrmatoare);
    }
}
