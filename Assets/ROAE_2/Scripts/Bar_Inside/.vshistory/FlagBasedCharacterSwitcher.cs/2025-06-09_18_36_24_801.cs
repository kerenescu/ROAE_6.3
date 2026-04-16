using UnityEngine;

public class FlagBasedCharacterSwitcher : MonoBehaviour
{
    [Header("Personaje")]
    public GameObject barista;
    public GameObject madameLichenia;

    [Header("Flag de verificat")]
    public DialogueFlag alreadyReadTarotFlag;

    void Start()
    {
        if (alreadyReadTarotFlag != null && alreadyReadTarotFlag.IsTriggered())
        {
            if (barista != null) barista.SetActive(false);
            if (madameLichenia != null) madameLichenia.SetActive(true);
        }
        else
        {
            if (barista != null) barista.SetActive(true);
            if (madameLichenia != null) madameLichenia.SetActive(false);
        }
    }
}
