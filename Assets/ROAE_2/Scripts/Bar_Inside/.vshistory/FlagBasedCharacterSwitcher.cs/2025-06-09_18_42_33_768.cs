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
            if (barista != null)
            {
                // Rotire pe Y cu 180°
                Vector3 baristaRotation = barista.transform.eulerAngles;
                baristaRotation.y += 180f;
                barista.transform.eulerAngles = baristaRotation;

                // Shift la dreapta cu 10 unități pe axa X
                Vector3 baristaPosition = barista.transform.position;
                baristaPosition.x += 10f;
                barista.transform.position = baristaPosition;

             
            }

            if (madameLichenia != null)
                madameLichenia.SetActive(true);
        }
        else
        {
            if (barista != null)
                barista.SetActive(true);

            if (madameLichenia != null)
                madameLichenia.SetActive(false);
        }
    }
}
